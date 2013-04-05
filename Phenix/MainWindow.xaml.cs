using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ServiceStack.Redis;
using System.Net.Sockets;
using System.Net;
 
namespace Phenix
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            
            RedisClient Redis = new RedisClient("localhost",6380);

            Redis.SetEntry("hi", "hello");
            var valueBytes = Redis.Get("hi");
            var valueString = RedisExt.GetString(valueBytes);
            display.Content = valueString;
            
            /*
            using (var redisClient = new RedisClient())
            {
                //IRedisTypedClient<Shipper> redis = redisClient.GetTypedClient<Shipper>();
            }
            */
        }


        /// <summary>
        /// 启动端口监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        static void TCPListener(string ip, int port)
        {
            try
            {
                //1.监听端口
                TcpListener server = new TcpListener(IPAddress.Parse(ip), port);
                server.Start();
                Console.WriteLine("{0:HH:mm:ss}->监听端口{1}...", DateTime.Now, port);

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
                        }

                        //2.3 返回状态
                        byte[] messages = System.Text.Encoding.ASCII.GetBytes("ok.");
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
        /// 发送数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        static void TCPClient(string ip, int port, string message)
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
