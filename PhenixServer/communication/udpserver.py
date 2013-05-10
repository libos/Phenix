#!/usr/bin/python
import socket, sys
SERVER_PORT = 1234

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
sock.bind(('', SERVER_PORT))

user_list = []
class User:
    def __init__(self,name,address):
        self.user_name = name
        self.public_address = address
    def __str__(self):
        return "[%s,%s]" % (self.user_name , self.public_address)

def dictFromString(headder):
	line_list = headder.split('\n')
	ret = {}
	for one_line in line_list:
		compoments = one_line.split(': ')
		if len(compoments) >= 2 and len(compoments[0]) > 0:
			ret[compoments[0]] = compoments[1]
	return ret

def server_handle():
	while True:
		data, address = sock.recvfrom(8192)
		if data == None or len(data) == 0:
			continue
		command = dictFromString(data)
		command_type = command['type']
		if command_type == 'login':
			name = command['user_name']
			address_str = command['address']
			user_private_ip = address_str.split(':')[0]
			user_private_port = long(address_str.split(':')[1])
			# now_user = User(name, address, (user_private_ip, user_private_port))
			now_user = None
			for one_user in user_list:
				# if one_user.user_name == name and one_user.public_address == address and one_user.private_address == (user_private_ip, user_private_port):
				if one_user.user_name == name and one_user.public_address == address:
					now_user = one_user
					break
			if now_user == None:
				print "add user"
				now_user = User(name, address)
				user_list.append(now_user)
				send_add_user(now_user)
				
		elif command_type == 'logout':
			name = command['user_name']
			address_str = command['address']
			user_private_ip = address_str.split(':')[0]
			user_private_port = long(address_str.split(':')[1])
			now_user = None
			for one_user in user_list:
				# if one_user.user_name == name and one_user.public_address == address and one_user.private_address == (user_private_ip, user_private_port):
				if one_user.user_name == name and one_user.public_address == address:
					now_user = one_user
					break
			if now_user != None and len(user_list) != 0:
				user_list.remove(now_user)
				send_remove_user(now_user)
			
		elif command_type == 'get_all_user':
			for one_user in user_list:
				if one_user.user_name != None and one_user.public_address != None:
					send_add_user(one_user)
		
		elif command_type == 'p2ptrans':
			print data
			connect_address_str = command['connect_address']
			connect_ip = connect_address_str.split(':')[0]
			connect_port = long(connect_address_str.split(':')[1])
			p2pconnect((connect_ip, connect_port), address)
	
		elif command_type == 'puserlist':
			print_user_list()
		elif command_type == 'echo':
			print "echo: "
			print data
			print address
			sendnum = sock.sendto(data, address)
		
def send_remove_user(user):
	headder = "type: rmuser\nuser_name: %s\naddress: %s:%d\n" % (user.user_name, user.public_address[0] , user.public_address[1])
	for user in user_list:
		sock.sendto(headder, user.public_address)
			
def send_add_user(user):
	headder = "type: adduser\nuser_name: %s\naddress: %s:%d\n" % (user.user_name, user.public_address[0] , user.public_address[1])
	for user in user_list:
		sock.sendto(headder, user.public_address)
			
def p2pconnect(connect_addr, address):
	headder = "type: p2pconnect\naddress: %s:%d\n" % address
	sock.sendto(headder, connect_addr)
	
def print_user_list():
	for user in user_list:
		print user
if __name__ == '__main__':
	server_handle()
	
