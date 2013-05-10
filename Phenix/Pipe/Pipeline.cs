using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Phenix.Pipe
{
    public delegate void msgQueueEventHandler(string sParam);  
    public delegate void UIEventHandler(string sParam);
    public class Pipeline
    {
        private string _ip;
        private int _port;
        private  ManualResetEvent _event;
        private static msgQueueEventHandler OnMsgQueueEvent;
        private static UIEventHandler OnWaitingFor;
        MainWindow _mw;
        public Pipeline(ManualResetEvent en,MainWindow mw)
        {
            _event = en;
            _mw = mw;
        }
        public event msgQueueEventHandler msgQueue
        {
            add { OnMsgQueueEvent += new msgQueueEventHandler(value); }
            remove { OnMsgQueueEvent -= new msgQueueEventHandler(value); }
        }
        public event UIEventHandler waitingHandle
        {
            
            add { OnWaitingFor += new UIEventHandler(value); }
            remove { OnWaitingFor -= new UIEventHandler(value);}
        }

        public void WaitingFor(object startOrstop)
        {
            OnWaitingFor((string)startOrstop);
            Thread.Sleep(1000);
            OnWaitingFor((string)startOrstop.ToString().Substring(2,4));
        }
        public void WaitingForService(object startOrstop)
        {
            OnWaitingFor("正在停止服务...");
            Thread aa = (Thread)startOrstop;
            while(aa.ThreadState != ThreadState.Stopped)
            {
            }
            OnWaitingFor("服务停止");
        }

        #region 启动端口监听
        /// <summary>
        /// obj "IP:Port"
        /// </summary>
        /// <param name="obj"></param>
        public void TCPListener(object obj)
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
                        if (_event.WaitOne())
                        {
                            if (server.Pending())//判断是否有挂起的链接
                            {
                                server.Stop();
                                Console.Write("{0:HH:mm:ss}->监听端口{1}...", DateTime.Now, port);
                                server.Start();
                            }
                        }
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
                        byte[] messages = System.Text.Encoding.ASCII.GetBytes("+OK");
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
        #endregion

        #region 发送命令
        public delegate void changeUI(MainWindow wm);
        /// <summary>
        /// msg  "IP:Port:Message"
        /// </summary>
        /// <param name="msg"></param>
        public void TCPClient(object msg)
        {
            string[] tmp;
            tmp = msg.ToString().Split(':');
            string ip = tmp[0];
            int  port = Convert.ToInt16(tmp[1]);
            tmp[0] = "";
            tmp[1] = "";
            string message = String.Join(":",tmp);
            message = message.Substring(2);
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

                while (!stream.DataAvailable) ;
                        int length = stream.Read(bytes, 0, bytes.Length);
                        if (length > 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, length);
                            Console.WriteLine("{0:HH:mm:ss}->接收状态：{1}", DateTime.Now, data);
                            if (data == Constants.LoginSUC)
                            {
                                this._mw.Dispatcher.BeginInvoke(new changeUI(r =>
                                {
                                    r.grpBefore.Visibility = System.Windows.Visibility.Hidden;
                                    r.lbEmail.Content = r.tbEmail.Text.Trim();
                                    r.lbUserState.Content = "ONLINE";
                                    r.grpAfter.Visibility = System.Windows.Visibility.Visible;
                                    r.onlineUser = r.tbEmail.Text.Trim();
                                }), this._mw);
                            }
                            else if (data.Contains(Constants.LoginFAIL))
                            {
                                this._mw.Dispatcher.BeginInvoke(new changeUI(r =>
                                {
                                    r.ShowMsg("用户名或密码错误");
                                }), this._mw);
                            }
                            if (data.Contains(Constants.LogoutSUC))
                            {
                                this._mw.Dispatcher.BeginInvoke(new changeUI(r =>
                                {
                                    r.onlineUser = string.Empty;
                                    r.tbEmail.Text = "";
                                    r.pbPassword.Password = "";
                                    r.grpBefore.Visibility = System.Windows.Visibility.Visible;
                                    r.lbEmail.Content = "";
                                    r.lbUserState.Content = "";
                                    r.grpAfter.Visibility = System.Windows.Visibility.Hidden;
                                }), this._mw);
                            }
                            else if (data.Contains(Constants.LogoutFAIL))
                            {
                                this._mw.Dispatcher.BeginInvoke(new changeUI(r =>
                                {
                                    r.ShowMsg("用户名或密码错误，无法注销");
                                }), this._mw);
                            }
                        }
              
              
                Console.Write("test1\n");
                //3.关闭对象
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0:HH:mm:ss}->{1}", DateTime.Now, ex.Message);
            }
        }
        #endregion
    }
}
