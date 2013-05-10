require 'redis'
require 'mysql'
require 'digest/md5' 
require 'pry'
=begin
redis = Redis.new
redis = Redis.new(:path=>"/tmp/redis.sock")
redis.set("mykey","hello")
puts redis.get("mykey")
=end
class User
    def init
	begin
 	   @con = Mysql.new 'localhost','root','jknlff8-pro-17m7755','Phenix'	
        rescue Mysql::Error =>e
	   puts e.error
        end
    end
    def	verify(email,password)
	rs =  @con.query "select * from users where email = \"#{email}\""
	num =  rs.num_rows
	if num == 0 
           return false
	end
	row = rs.fetch_hash
	passwd = Digest::MD5.hexdigest("#{password}Phenix#{row['salt']}")
	if passwd == row['password']
		return true
	else
		return false
	end
    end
    def update(email,attr,value)
	@con.query "update users set #{attr}=#{value} where email = \"#{email}\""
    end
    def close
	@con.close
    end
end
class Server
	LoginHeader = '*ClientLogin*'
	UserNameHeader = '*UserName*'
	PasswordHeader = '*Password*'
	LoginSUC = '*LoginSUC*'
	LoginFAIL = '*LoginFAIL*'

	LogoutHeader = '*ClientLogout*'
        LogoutSUC = '*LogoutSUC*'
	LogoutFAIL = '*LogoutFAIL*'
     def start
	server = TCPServer.new 9980
	while true
	   Thread.start(server.accept) do |client|
		recv = []
		while line = client.gets
		    recv.push(line.strip)
		    puts line.strip
		    break if !(line !~ /\*\*/)
	        end
		if recv[0] ==  LoginHeader 
			email = recv[2]
			password = recv[4]
		  	user = User.new
			user.init
			if user.verify(email,password)
				puts LoginSUC
			 	client.puts LoginSUC
				user.update(email,"status",1)
			else
				puts LoginFAIL
				client.puts LoginFAIL
			end
			user.close
			client.close
		end
		if recv[0] == LogoutHeader
			email = recv[2]
			password = recv[4]
 			user = User.new
                        user.init
                        if user.verify(email,password)
				puts LogoutSUC
				client.puts LogoutSUC
				user.update(email,"status",0)
			else 
				puts LogoutFAIL
				client.puts LogoutFAIL
			end
			user.close
			client.close
		end
	   end
	end
     end
end

s = Server.new
s.start
