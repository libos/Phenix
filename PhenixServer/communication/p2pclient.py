#!/usr/bin/python

import socket, SocketServer, threading, thread, time
CLIENT_PORT = 4321
SERVER_IP = "210.51.4.178"
SERVER_PORT = 1234
user_list = []
client_addresses = set([])
local_ip = socket.gethostbyname(socket.gethostname())
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
class User:
    def __init__(self,name,address):
        self.user_name = name
        self.public_address = address
    def __str__(self):
        return "[%s,%s]" % (self.user_name , self.public_address)

def print_users():
	for user in user_list:
		print user

def dictFromString(headder):
	line_list = headder.split('\n')
	ret = {}
	# print line_list
	for one_line in line_list:
		compoments = one_line.split(': ')
		# print compoments
		if len(compoments) >= 2:
			ret[compoments[0]] = compoments[1]
	return ret
	
def addressFromString(address):
	components = address.split(':')
	if len(components) < 2:
		return None
	else:
		ip = components[0]
		port = long(components[1])
		return (ip, port)
		
def create_headder(user_name, cmd_type):
	address = "%s:%d" % (local_ip, CLIENT_PORT)
	if cmd_type == 'login':
		ret_str = "type: login\nuser_name: %s\naddress: %s\n" % (user_name, address)
		return ret_str
	elif cmd_type == 'logout':
		ret_str = "type: logout\nuser_name: %s\naddress: %s\n" % (user_name, address)
		return ret_str
	elif cmd_type == 'getallusers':
		ret_str = "type: get_all_user\n"
		return ret_str
	elif cmd_type == 'connect':
		for a_user in user_list:
			if a_user != None and a_user.user_name == user_name:
				connect_address = "%s:%d" % a_user.public_address
				client_addresses.add(a_user.public_address)
				thread.start_new_thread(send_hand, (a_user.public_address,))
				ret_str = "type: p2ptrans\nconnect_address: %s\n" % connect_address
				return ret_str
		return None
	elif cmd_type == 'puserlist':
		ret_str = "type: puserlist\n"
		return ret_str
	elif cmd_type == 'echo':
		ret_str = 'type: echo\n'
		return ret_str
	return None

def send_hand(address):
	count = 0
	if address in client_addresses and count < 10:
		headder = "type: hand\n"
		sock.sendto(headder, address)
		time.sleep(6)
	
def server_handle():
	print 'new thread'
	while True:
		data, addr = sock.recvfrom(8192)
		# print data
		# print addr
		if addr[0] == SERVER_IP and addr[1] == SERVER_PORT and data != None and data != '':
			head_dict = dictFromString(data)
			if head_dict.has_key('type') == False:
				continue
			cmd_type = head_dict['type']
			if cmd_type == 'adduser':
				user_name = head_dict['user_name']
				address = addressFromString(head_dict['address'])
				now_user = User(user_name, address)
				flag = False
				for aUser in user_list:
					if aUser == now_user:
						flag = True
				if not flag:
					print now_user
					print flag
					user_list.append(now_user)
			elif cmd_type == 'rmuser':
				user_name = head_dict['user_name']
				address = addressFromString(head_dict['address'])
				now_user = User(user_name, address)
				for aUser in user_list:
					if aUser == now_user:
						user_list.remove(aUser)
			elif cmd_type == 'p2pconnect':
				address = addressFromString(head_dict['address'])
				if address not in client_addresses:
					client_addresses.add(address)
					thread.start_new_thread(send_hand, (address, ))
			elif cmd_type == 'echo':
				print "message: %s" % data
		else:
			head_dict = dictFromString(data)
			if 'type' not in head_dict:
				continue
			cmd_type = head_dict['type']
			if cmd_type == 'hand':
				print "hand from %s:%d" % address
				sock.sendto("type: helloworld", address)
				if address in client_addresses:
					client_addresses.remove(address)
			if cmd_type == 'helloworld':
				print "hello world from %s:%d" % address
				

if __name__ == '__main__':
	thread.start_new_thread(server_handle, ())
	cmd = raw_input('cmd>>')
	while True:
		args = cmd.split(' ')
		headder = None
		if args == None or args[0] == None or args[0] == '':
			continue
		
		if args[0] == 'login':
			user_name = args[1]
			headder = create_headder(user_name, 'login')
		elif args[0] == 'logout':
			headder = create_headder('', 'logout')
		elif args[0] == 'connect':
			headder = create_headder(args[1], 'connect')
		elif args[0] == 'getallusers':
			headder = create_headder('', 'getallusers')
		elif args[0] == 'puserlist':
			headder = create_headder('', 'puserlist')
		elif args[0] == 'echo':
			headder = create_headder('', 'echo')
		elif args[0] == 'users':
			print_users()
		if headder != None:
			# print headder
			sock.sendto(headder, (SERVER_IP, SERVER_PORT))
		cmd = raw_input('cmd>>')
