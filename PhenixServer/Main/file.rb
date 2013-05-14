require 'socket'
require 'zlib'
require 'pry'
require File.join(File.dirname(__FILE__),'filesender')

class Recieve
 def listen
  server = TCPServer.new 9981
   loop do
    Thread.start(server.accept) do |client|
	filename = ""
	total_block = 0
	last_block_size = 0
	puts "GET FileName"
	client.write "#{[STRING_HEADER].pack("C")}GET FileName"
	while line = client.recv(1024)
		recv =  line[1..-1].split(' ')
		if recv[0] == 'SET' and recv[1] == 'FileName'
			filename = recv[2]
			break	
		end
		client.write "#{[STRING_HEADER].pack("C")}GET FileName"
	end
	filename.gsub(/#{SPACE_REPLACE}/,SPACE)	
	puts filename
  	file = File.open("tmp/#{filename}", 'wb')
	puts "GET TotalBlock"
	client.write "#{[STRING_HEADER].pack("C")}GET TotalBlock"
	while line = client.recv(1024)
		recv =  line[1..-1].split(' ')
		if recv[0] == 'SET' and recv[1] == 'TotalBlock'
			total_block = recv[2].to_i
			break	
		end
		client.write "#{[STRING_HEADER].pack("C")}GET TotalBlock"
	end
	puts total_block
	puts "GET LastBlockSize"
	client.write "#{[STRING_HEADER].pack("C")}GET LastBlockSize"
	while line = client.recv(1024)
		recv =  line[1..-1].split(' ')
		if recv[0] == 'SET' and recv[1] == 'LastBlockSize'
			last_block_size = recv[2].to_i
			break	
		end
		client.write "#{[STRING_HEADER].pack("C")}GET LastBlockSize"
	end
	puts last_block_size
	puts "Gets File"
	fileBlocks = []
	(0...total_block).each do |index|
		client.write "#{[STRING_HEADER].pack("C")}GET BlockHash #{index}"
		while line = client.recv(1024)
			puts "Abandon #{line[1..-1]}"
			break	
		end
		size = (index == total_block-1) ? (last_block_size +9) : (BLOCK_SIZE+9)
		
		client.write "#{[STRING_HEADER].pack("C")}GET FileBlock #{index}"
		while chunk = client.read(size)
			header = chunk[0]
			indexhash = chunk[1..4]
			hash = chunk[5..8]
			fileBlocks << chunk[9..-1]	
			#IO.binwrite("tmp/#{filename}",chunk[9..-1],index * size) 
			break
		end
	end
	puts "Write File"
	data = fileBlocks.join.force_encoding("UTF-8")
	file.set_encoding('UTF-8')
      	file.write(data)
	file.close
	
	client.write "#{[STRING_HEADER].pack("C")}Finished"
	puts "Send"
	sender = FileSender.new
	sender.Send(client,"/root/a.sql")
	puts "Exit"
	client.write "#{[STRING_HEADER].pack("C")}Exit"
	client.close
   end
  end
 end
end

recv = Recieve.new
recv.listen
