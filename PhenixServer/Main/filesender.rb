require 'socket'
require 'zlib'
require 'pry'
def CRC32(string)
	[Zlib::crc32(string)].pack('L').unpack('l')
end
BLOCK_SIZE = 1048576
FILE_BLOCK_HEADER = 0
STRING_HEADER = 1
NET_BLOCK_MAX_SIZE = BLOCK_SIZE + 9
DEFAULT_IO_BUFFER_SIZE = 8
SPACE = ' '
SPACE_REPLACE = '<SPACE>'

class FileSender
  
    def Send(client,filename)
	_totalBlock = 0
	_lastBlockSize = 0
	fileSize = File.stat(filename).size
	_totalBlock = fileSize / BLOCK_SIZE  + 1
	_lastBlockSize = fileSize - ((_totalBlock - 1 )	* BLOCK_SIZE)
	fileBlock = []
	fileBlockHash = []
	File.open(filename,'r') do |file|
		size = (fileBlock.length == _totalBlock-1) ? (_lastBlockSize ) : (BLOCK_SIZE)
		while chunk =  file.read(size)
			fileBlock << chunk
			fileBlockHash << CRC32(chunk.to_s)
		end
	end
  	while line = client.recv(1024)
		if line[0] != "#{[STRING_HEADER].pack('C')}" 
			next
		end
		recv = line[1..-1].split(' ')
		if recv[0] == 'Exit'
		    puts "Exit"
		    break
		elsif (recv[0] == 'GET')
			if recv[1] == 'FileBlock'
				puts "FileBlock"
			 	blockIndex = recv[2].to_i
				#binding.pry
				#Thread.new  do 
					client.write "#{[FILE_BLOCK_HEADER].pack("C")}#{[blockIndex].pack('l')}#{[fileBlockHash[blockIndex][0]].pack('l')}#{fileBlock[blockIndex]}"
				#end
				next	
			elsif recv[1] == 'BlockHash'
			 	blockIndex = recv[2].to_i
				puts "BlockHash"
				client.write "#{[STRING_HEADER].pack("C")}BlockHash #{blockIndex} #{fileBlockHash[blockIndex]}"
				next
			elsif recv[1] == 'FileName'
				puts "FileName"
				client.write "#{[STRING_HEADER].pack("C")}SET FileName #{File.basename(filename).gsub("#{SPACE}", "#{SPACE_REPLACE}")}"
				next
			elsif recv[1] == 'TotalBlock'
				puts "TotalBlock"
				client.write "#{[STRING_HEADER].pack("C")}SET TotalBlock #{_totalBlock}"
				next
			elsif recv[1] == 'LastBlockSize'
				puts "LastBlockSize"
				client.write "#{[STRING_HEADER].pack("C")}SET LastBlockSize #{_lastBlockSize}"
				next
			else
				puts "Bad Command"
				next
 			end
		elsif recv[0]=='Finished'
		#start recv	
		else
			puts "Bad Command"
			next
		end
	end
   end
end
