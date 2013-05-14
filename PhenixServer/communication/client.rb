require 'socket'
require 'timeout'
include Socket::Constants
@serverIP = '210.51.4.178' #'127.0.0.1'#
@MainPort = 9005
@HolePort = 8908
@selfHolePort = 9001
result = []
def clientA_handler(s)
  # s.send("HELP HOLE\n",0)
  socket = TCPSocket.new @serverIP, @HolePort
  socket.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  while line = socket.gets
    puts "Receive:#{line}"
    if line == "New Login#{10.chr}"
      break
    end
  end
  puts "Send:HELP HOLE"
  socket.send("HELP HOLE\n",0)
  addr_B = []
  s.send("QUIT\n",0)
  while line = socket.gets
    puts "Receive:#{line}"
    addr_B << line
    if line == "Wait#{10.chr}"
      break
    end
  end
  while line = s.gets
    puts "Receive:#{line}"
    if line == "HOLE_B OK#{10.chr}"
      break
    end
  end
  s = TCPSocket.new addr_B[2].gsub(10.chr,''),addr_B[1].gsub(10.chr,'').to_i
  s.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  while true
    s.send('hello B\n',0)
  end
end

def clientB_handler(s)
  socket = TCPSocket.new @serverIP, @HolePort
  socket.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  socket.send("hello\n",0)
  socket.close
  addr_A = []
  while line = s.gets
    addr_A << line
    puts "Receive:#{line}"
    if line == "START HOLE#{10.chr}"
      break
    end
  end
  ip_A = addr_A[2].gsub(10.chr,'')
  port_A = addr_A[1].gsub(10.chr,'').to_i
  puts "Open HOLE"
  s.send("OPEN HOLE\n",0)
  socket = Socket.new( AF_INET, SOCK_STREAM, 0 )
  for k in 0...10
    begin 
        timeout(0.2) do
          socket.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
          client, client_addrinfo = socket.accept
          sockaddr = Socket.pack_sockaddr_in( port_A, ip_A)
          socket.connect( sockaddr )
          # i = 0
          # while i < 5
          #   socket.send("hello",0)
          #   i += 1
          # end
          # begin
          #   pair = client.recvfrom_nonblock(20)
          # rescue IO::WaitReadable
          #   IO.select([client])
          #   retry
          # end
    end
    rescue Timeout::Error
        puts "Timed out!"
    end
  end
  sockaddr = Socket.pack_sockaddr_in(@selfHolePort, '0.0.0.0' )
  socket.bind(sockaddr)
  sh = TCPServer.open @selfHolePort
  sh.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))
  puts "Send:HOLE DONE"
  s.send("HOLE DONE\n",0)
  while true
    Thread.start(sh.accept) do |cli|
      cli.puts "hello"
      puts "#{cli}login"
    end
  end
end


s = TCPSocket.new @serverIP, @MainPort
s.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))

while line = s.gets
  result << line
  puts line
  if line == "+OK#{10.chr}"
    clientA_handler(s)
  end
  if line =="+2OK#{10.chr}"
    clientB_handler(s)
  end
end


=begin
time
AF_INET
46161
103.8.222.17
103.8.222.17
=end

  # socket = Socket.new( AF_INET, SOCK_STREAM, 0 )
#   sockaddr = Socket.pack_sockaddr_in(@selfHolePort, 'localhost' )
#   socket.bind( sockaddr )
#   sockaddr = Socket.pack_sockaddr_in( @HolePort, @serverIP)
#   socket.connect( sockaddr )
#   socket.setsockopt(Socket::Option.bool(:INET, :SOCKET, :REUSEADDR, true))

  