using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Phenix
{
    public delegate void msgQueueEventHandler(string sParam);  
    class Pipeline
    {
        public Pipeline()
        {
        }
        private static msgQueueEventHandler OnMsgQueueEvent;
        public event msgQueueEventHandler msgQueue
        {
            add { OnMsgQueueEvent += new msgQueueEventHandler(value); }
            remove { OnMsgQueueEvent -= new msgQueueEventHandler(value); }
        }  
        /// <summary>
        /// 启动端口监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void TCPListener(object obj)
        {
            string[] tmp;
            tmp = obj.ToString().Split(':');
            string ip = tmp[0];
            int port = Convert.ToInt16(tmp[1]);
            try
            {
                //1.监听端口
                TcpListener server = new TcpListener(IPAddress.Parse(ip), port);
                server.Start();
                //Console.Write("{0:HH:mm:ss}->监听端口{1}...", DateTime.Now, port);

                //2.等待请求
                while (true)
                {
                    try
                    {
                        //2.1 收到请求
                        TcpClient client = server.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();

                        //2.2 解析数据,长度<1024字节
                        string data = string.Empty;
                        byte[] bytes = new byte[1024];
                        int length = stream.Read(bytes, 0, bytes.Length);
                        if (length > 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, length);
                            Console.WriteLine("{0:HH:mm:ss}->收到数据：{1}", DateTime.Now, data);
                            OnMsgQueueEvent(data);
                        }

                        //2.3 返回状态
                        byte[] messages = System.Text.Encoding.ASCII.GetBytes("OK");
                        stream.Write(messages, 0, messages.Length);

                        //2.4 关闭客户端
                        stream.Close();
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0:HH:mm:ss}->{1}", DateTime.Now, ex.Message);
                    }
                }
            }
            catch (SocketException socketEx)
            {
                //10013 The requested address is a broadcast address, but flag is not set.
                if (socketEx.ErrorCode == 10013)
                    Console.WriteLine("{0:HH:mm:ss}->启动失败,请检查{1}端口有无其他程序占用.", DateTime.Now, port);
                else
                    Console.WriteLine("{0:HH:mm:ss}->{1}", DateTime.Now, socketEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0:HH:mm:ss}->{1}", DateTime.Now, ex.Message);
            }
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        public static void TCPClient(string ip, int port, string message)
        {
            try
            {
                //1.发送数据
                byte[] messages = System.Text.Encoding.ASCII.GetBytes(message);
                TcpClient client = new TcpClient(ip, port);
                NetworkStream stream = client.GetStream();
                stream.Write(messages, 0, messages.Length);
                Console.WriteLine("{0:HH:mm:ss}->发送数据：{1}", DateTime.Now, message);

                //2.接收状态,长度<1024字节
                byte[] bytes = new Byte[1024];
                string data = string.Empty;
                int length = stream.Read(bytes, 0, bytes.Length);
                if (length > 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, length);
                    Console.WriteLine("{0:HH:mm:ss}->接收状态：{1}", DateTime.Now, data);
                }

                //3.关闭对象
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0:HH:mm:ss}->{1}", DateTime.Now, ex.Message);
            }
        }
    }
}
