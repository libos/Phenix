require 'socket'

s = TCPSocket.new '210.51.4.178', 9005
s.setsockopt(:SOCKET, :REUSEADDR, true)
result = []
while line = s.gets 
  puts line
  result<< line
end
=begin
time
AF_INET
46161
103.8.222.17
103.8.222.17
=end
puts s.addr
server=TCPServer.open(s.addr[1].to_i)
server.setsockopt(:SOCKET, :REUSEADDR, true)
loop{
  puts s.addr[1].to_i
  client = server.accept
  client.puts(client.addr)
  puts(client.peeraddr)
  client.close}
