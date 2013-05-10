require 'socket'
@hole_port = 8908
@main_port = 9005
puts "initialize..."



@clients = []
@waitinglist = []
def mainThread_handler(client)
  puts  "welcome:#{client.peeraddr[2]}:#{client.peeraddr[1]}"
  @clients << client
  if @clients.count  == 2
    @clients[0].puts @clients[1].peeraddr
    @clients[0].puts "+OK"
    @clients[1].puts "+2OK"
    while (tmp = @clients[0].recv(10)) 
	if tmp.length  == 0
		continue
	end
	puts tmp
	if tmp == "QUIT#{10.chr}"
		break
	end
     end
     while (tmp = @clients[1].recv(10))
	puts "Receive:#{tmp}"
	if tmp == "HOLE DONE#{10.chr}"
	   puts "Send:HOLE_B OK"
	   @clients[0].puts "HOLE_B OK"
	end
	if tmp == "OPEN HOLE#{10.chr}"
	   puts "Send:Wait"
	   @waitinglist[0].puts "Wait"
	end
     end

   end	
end
def Thread_TCPServer(port)
  server = TCPServer.open(port)
  server.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  loop do
    puts "one again"
    Thread.start(server.accept) do |client|
      # client.puts(Time.now.ctime)
      # client.puts(client.peeraddr)
      if client.addr[1].to_i == @main_port
	mainThread_handler(client) 
      else
      	@waitinglist << client
	if @waitinglist.count == 2
	  @waitinglist[0].puts "New Login"
 	  while (tmp = @waitinglist[0].recv(10))
		puts tmp
 		if tmp == "HELP HOLE#{10.chr}"
		   break
		end
	  end
	  @waitinglist[0].puts @waitinglist[1].peeraddr
	  @clients[1].puts @waitinglist[0].peeraddr
	  @clients[1].puts "START HOLE"
	end
      end
    end
   end
end

t1 = Thread.new do
  Thread_TCPServer(@main_port)
end
t2 = Thread.new do
  Thread_TCPServer(@hole_port)
end
t1.join
t2.join