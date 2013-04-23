require 'socket'

s = TCPSocket.new '210.51.4.178', 9005
s.setsockopt(:SOCKET, :REUSEADDR, true)
result = []
while line = s.gets 
  puts line
  result<< line
end

s.sent "ehllo"
=begin
time
AF_INET
46161
103.8.222.17
103.8.222.17
=end

