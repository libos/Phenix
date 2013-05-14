require 'redis'
require 'mysql'
require 'digest/md5' 
require 'pry'
require 'json'
=begin
=end
class User
    def initialize
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
    def online?(email)
	rs = @con.query  "select * from users where email = \"#{email}\""
	num =  rs.num_rows
        if num == 0
           return false
        end
	row = rs.fetch_hash
	if row['status'] == '1'
	   return true
	end 
	return false
    end
    def getOnlieUsers
	rs = @con.query "select * from users where status = 1"
	num = rs.num_rows
	if num == 0 
	   return nil
	end
	onlineUsers = []
	rs.each_hash do |row|
	   onlineUsers << row
	end	
	return onlineUsers
    end
    def close
	@con.close
    end
end
class Task
	PriorityQueue = ['TaskQueue0','TaskQueue1','TaskQueue2','TaskQueue3','TaskQueue4']
	TaskDoneQueue = 'TaskDoneQueue'
	def initialize
	     @redis = Redis.new(:path=>"/tmp/redis.sock")
	end
	def queueTask(priority,task)
	     @redis.lpush(PriorityQueue[priority],task)	
	end
	def getList
	     taskList = []
	     PriorityQueue.each do |item|
	     	taskList.push(@redis.lrange(item,0,-1))
	     end
	     return taskList
	end
	def checkQueue
	     taskList = []
  	     PriorityQueue.each do |item|
		tmp = @redis.lrange(item,0,-1)
		if tmp.length > 0
		   return item
		end
	     end
	     return nil
	end
	def getOneQueue(list_name)
	     @redis.lrange(list_name,0,-1)
	end
	def taskDone(task)
	    @redis.multi do
		PriorityQueue.each do |q|
		   @redis.lrem(q,0,task)
		end
		@redis.lpush(TaskDoneQueue,task)
	    end
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
   	CreateTaskHeader = "*CreateTask*" 
	UpdateTaskHeader = "*UpdateTask*"
	TaskJson = "*TaskJson*"
	CreateTaskSuc = "*CreateTaskSuc*"
	UpdateTaskSuc = "*UpdateTaskSuc*"
    def initialize
	@clients = []
    	Thread.new do 
		puts "Distribute Start"
		task = Task.new
		user = User.new
		while true
		   if !task.checkQueue.nil?
			users = user.getOnlineUsers
		        binding.pry
			task.getList.each_with_index do |t,index| 
			   send2User(users[index],t)
			end	
 		   end
		end
	end
    end
    def send2User(user,task)
    end
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
		if recv[0] == CreateTaskHeader and recv[1] == UserNameHeader and recv[3] == TaskJson
			email = recv[2]
		 	task = recv[4]
			user = User.new
			if user.online?(email)
				redis = Task.new
				redis.queueTask(0,task)
				taskhash = JSON.parse task	
			end
		end 
		if recv[0] == RecieveProtocolHeader and recv[1] == UserNameHeader and recv[3] == FilePile
		
		end
	   end
	end
     end
end

s = Server.new
s.start
