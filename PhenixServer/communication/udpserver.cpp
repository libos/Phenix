/* P2P 程序服务端
 * 
 * 文件名：P2PServer.c
 *
 * 日期：2004-5-21
 *
 * 作者：shootingstars(zhouhuis22@sina.com)
 *
 */

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <pthread.h>

#include "protos.h"
#include "Exceptions.h"

#define SOCKET int

UserList ClientList;

void InitWinSock()
{
  int thisSocket;
	struct sockaddr_in destination;

	destination.sin_family = AF_INET;
	thisSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); 
	if (thisSocket < 0)
	{
  	printf("Error");
    exit(1);
	}
	printf("Startup\n");
}


SOCKET mksock(int type)
{
	SOCKET sock = socket(AF_INET, type, 0);
	if (sock < 0)
	{
        printf("create socket error");
		exit(1);
	}
	return sock;
}

stUserListNode GetUser(char *username)
{
	for(UserList::iterator UserIterator=ClientList.begin();
						UserIterator!=ClientList.end();
							++UserIterator)
	{
		if( strcmp( ((*UserIterator)->userName), username) == 0 )
			return *(*UserIterator);
	}
  printf("not find this user");
  exit(1);
}

int main(int argc, char* argv[])
{
	try{
		InitWinSock();
		
		SOCKET PrimaryUDP;
		PrimaryUDP = mksock(SOCK_DGRAM);

		sockaddr_in local;
		local.sin_family=AF_INET;
		local.sin_port= htons(SERVER_PORT); 
		local.sin_addr.s_addr = htonl(INADDR_ANY);
		int nResult=bind(PrimaryUDP,(sockaddr*)&local,sizeof(sockaddr));
		if(nResult)
    {
      printf("bind error");
      exit(1);
    }
			

		sockaddr_in sender;
		stMessage recvbuf;
		memset(&recvbuf,0,sizeof(stMessage));

		// 开始主循环.
		// 主循环负责下面几件事情:
		// 一:读取客户端登陆和登出消息,记录客户列表
		// 二:转发客户p2p请求
		for(;;)
		{
			int dwSender = sizeof(sender);
			int ret = recv(PrimaryUDP, (char *)&recvbuf, sizeof(stMessage), 0);
			if(ret <= 0)
			{
				printf("recv error");
				continue;
			}
			else
			{
				int messageType = recvbuf.iMessageType;
				switch(messageType){
				case LOGIN:
					{
						//  将这个用户的信息记录到用户列表中
						printf("has a user login : %s\n", recvbuf.message.loginmember.userName);
						stUserListNode *currentuser = new stUserListNode();
						strcpy(currentuser->userName, recvbuf.message.loginmember.userName);
						currentuser->ip = ntohl(sender.sin_addr.s_addr);
						currentuser->port = ntohs(sender.sin_port);
						
						ClientList.push_back(currentuser);

						// 发送已经登陆的客户信息
						int nodecount = (int)ClientList.size();
						send(PrimaryUDP, (const char*)&nodecount, sizeof(int), 0);
						for(UserList::iterator UserIterator=ClientList.begin();
								UserIterator!=ClientList.end();
								++UserIterator)
						{
							send(PrimaryUDP, (const char*)(*UserIterator), sizeof(stUserListNode), 0); 
						}

						break;
					}
				case LOGOUT:
					{
						// 将此客户信息删除
						printf("has a user logout : %s\n", recvbuf.message.logoutmember.userName);
            // UserList::iterator removeiterator = false;
            // for(UserList::iterator UserIterator=ClientList.begin();
            //   UserIterator!=ClientList.end();
            //   ++UserIterator)
            // {
            //   if( strcmp( ((*UserIterator)->userName), recvbuf.message.logoutmember.userName) == 0 )
            //   {
            //     removeiterator = UserIterator;
            //     break;
            //   }
            // }
            // if(removeiterator != NULL)
            //   ClientList.remove(*removeiterator);
						break;
					}
				case P2PTRANS:
					{
						// 某个客户希望服务端向另外一个客户发送一个打洞消息
						printf("%s wants to p2p %s\n",inet_ntoa(sender.sin_addr),recvbuf.message.translatemessage.userName);
						stUserListNode node = GetUser(recvbuf.message.translatemessage.userName);
						sockaddr_in remote;
						remote.sin_family=AF_INET;
						remote.sin_port= htons(node.port); 
						remote.sin_addr.s_addr = htonl(node.ip);

						in_addr tmp;
						tmp.s_addr = htonl(node.ip);
						printf("the address is %s,and port is %d\n",inet_ntoa(tmp), node.port);

						stP2PMessage transMessage;
						transMessage.iMessageType = P2PSOMEONEWANTTOCALLYOU;
						transMessage.iStringLen = ntohl(sender.sin_addr.s_addr);
						transMessage.Port = ntohs(sender.sin_port);
                        
						send(PrimaryUDP,(const char*)&transMessage, sizeof(transMessage), 0);

						break;
					}
				
				case GETALLUSER:
					{
						int command = GETALLUSER;
						send(PrimaryUDP, (const char*)&command, sizeof(int), 0);

						int nodecount = (int)ClientList.size();
						send(PrimaryUDP, (const char*)&nodecount, sizeof(int), 0);

						for(UserList::iterator UserIterator=ClientList.begin();
								UserIterator!=ClientList.end();
								++UserIterator)
						{
							send(PrimaryUDP, (const char*)(*UserIterator), sizeof(stUserListNode), 0); 
						}
						break;
					}
				}
			}
		}

	}
	catch(Exception &e)
	{
		printf(e.GetMessage());
		return 1;
	}

	return 0;
}

