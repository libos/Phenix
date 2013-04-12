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

using System.Threading;

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
        private Thread tcplisener = null;
        private static string param = "";
        public MainWindow()
        {
            InitializeComponent();
            //Pipeline.TCPClient("127.0.0.1", 9999, "");
            Pipeline pl = new Pipeline();
            tcplisener = new Thread(new ParameterizedThreadStart(Pipeline.TCPListener));
            pl.msgQueue += this.onRecieveCmd;
            tcplisener.IsBackground = true;
            tcplisener.Start("127.0.0.1:9999");
            
            //("127.0.0.1", 9998)
        }
        private void onRecieveCmd(string sParam)
        {
            param = sParam;
            Object[] list = {this,System.EventArgs.Empty };
            statusBox.Dispatcher.BeginInvoke(new EventHandler(onCmd), list);//.Items.Add(param);
        }
        protected void onCmd(Object o, EventArgs e)
        {
            this.statusBox.Items.Add(param);
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                RedisClient Redis = new RedisClient("localhost", 6380);
                QueueModule rm = new QueueModule();
                rm.inQueue("FLUSHDB");
                //Redis.SetEntry("hi", "hello");
                //var valueBytes = Redis.Get("hi");
                //var valueString = RedisExt.GetString(valueBytes);
                //display.Content = valueString;
                //statusBox.Items.Add("hi");
                /*
                using (var redisClient = new RedisClient())
                {
                    //IRedisTypedClient<Shipper> redis = redisClient.GetTypedClient<Shipper>();
                }
                */
            }
            catch (ServiceStack.Redis.RedisException ex)
            {
                statusBox.Items.Add("Redis服务未启动" + ex.Message);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void startService_Click(object sender, RoutedEventArgs e)
        {
            statusBox.Items.Add("hihih");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void getMissionList_Click(object sender, RoutedEventArgs e)
        {
            Pipeline.TCPClient("127.0.0.1", 9999, "hihi");
        }

        

    }

}
