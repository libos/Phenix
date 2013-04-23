require 'socket'
@hole_port = 8908
@main_port = 9005
puts "initialize..."



@clients = []
def Thread_SockClient()
  
end
def Thread_TCPServer(port)
  server = TCPServer.open(port)
  server.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  loop{
    Thread.start(server.accept) do |client|
      # client.puts(Time.now.ctime)
      # client.puts(client.peeraddr)
      
      puts  "welcome:#{client.peeraddr[2]}:#{client.peeraddr[1]}"
      if @clients.count  == 2
        @clients[0].puts @clients[1].addr
      end
      @clients << client
      @clients.each do |c|
        if c.addr[1] != @main_port && client.addr != c.addr
          c.puts "#{client.addr} has log in just now"
        end
      end
    end}
end

t1 = Thread.new do
  Thread_TCPServer(@main_port)
end
t2 = Thread.new do
  Thread_TCPServer(@hole_port)
end
t1.join
t2.join




