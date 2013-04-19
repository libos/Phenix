using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Threading;
using ServiceStack.Redis;
using ServiceStack.Text;

using Phenix.Pipe;
using Phenix.Core;
using Phenix.Notify;

#region Windows
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Net;
#endregion

namespace Phenix
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window 
    {

        public static ManualResetEvent events = new ManualResetEvent(false);
        public static bool startListenButton = false;
        RedisClient Redis = new RedisClient("localhost", 6380, Constants.Passwd);
        ServiceController sc2 = new ServiceController(Constants.ServiceName);
        Pipeline pl = new Pipeline(events);

        FilePipe fp;
        private Thread tcplistener = null;
        private Thread tcpSender = null;
        private static string param = "";
        private static string paramStatus = "";
        private bool rollback = false;

        ObservableCollection<Task> _TasksList = new ObservableCollection<Task>();

        #region 初始化
        public MainWindow()
        {
            _TasksList.Add(Task.generateTasks());
            _TasksList.Add(Task.generateTasks());
            InitializeComponent();
 


            #region Events Bound
            pl.msgQueue += this.onRecieveCmd;
            pl.waitingHandle += this.OnChangeStatus;

            ((INotifyCollectionChanged)statusBox.Items).CollectionChanged += statusBox_Filter;
             #endregion
        
            #region NoNeed
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Constants.DelProgramData;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            #endregion

            #region Service
            try
            {
                startListen();
            }
            catch (Exception ex)
            {
                this.listen_img.Source = Properties.Resources.Stop.ToImageSource();
                this.listen_lb.Content = Constants.CannotListen;
                this.statusBox.Items.Add(Constants.CannotListen);
            }
            SendMessage(Constants.listenObj + ":*BEGIN");
            checkListenerStatus();
            checkServiceStatus();
            #endregion

            #region LocalInfo

            this.IPAddress_lb.Content = Utils.GetIPv4();
            Utils.MacAddress = Utils.GetMacAddress();
            this.Mac_lb.Content = Utils.MacAddress;
            #endregion
            fp = new FilePipe(this);
            this.Hide(); 

        }


        public ObservableCollection<Task> TasksList
        {
            get
            {
                return _TasksList;
            }
        }
        public void checkListenerStatus()
        {
            if (startListenButton)
            {
                this.startListening.Header = Constants.StopListening;
                this.listen_img.Source = Properties.Resources.Active.ToImageSource();
                this.listen_lb.Content = Constants.StartListening.Substring(2)  + Constants.listenPort + "端口";
            }
            else
            {
                this.startListening.Header = Constants.StartListening;
                this.listen_img.Source = Properties.Resources.Stop.ToImageSource();
                this.listen_lb.Content = Constants.StopListening;
            }
        }
        public void checkServiceStatus()
        {
            try
            {
                if (sc2.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Running))
                {
                    this.startService.Header = Constants.ServiceStop;
                    this.service_img.Source = Properties.Resources.Active.ToImageSource();
                    this.service_lb.Content = Constants.ServiceStarted;
                }
                else
                {
                    this.startService.Header = Constants.ServiceStart;
                    this.service_img.Source = Properties.Resources.Stop.ToImageSource();
                    this.service_lb.Content = Constants.ServiceStopped;
                }
            }
            catch (Exception ex)
            {
                //statusBox.Items.Add(Constants.NoServiceError);
                this.service_img.Source = Properties.Resources.Inactive.ToImageSource();
                this.service_lb.Content = Constants.NoServiceError;
                this.startService.Header = Constants.ServiceInstall;
            }
        }
        #endregion

        #region TCP
        private void SendMessage(string msg)
        {
            //tcpSender = new Thread(new ParameterizedThreadStart(pl.TCPClient));
            bool rc= ThreadPool.QueueUserWorkItem(new WaitCallback(pl.TCPClient), msg);

            //tcpSender.Name = "TCPSender";
            //tcpSender.Start(msg);
        }
        private void startListen()
        { 
      
            tcplistener = new Thread(new ParameterizedThreadStart(pl.TCPListener));
            tcplistener.Name = "TCPListener"; 
            tcplistener.IsBackground = true;
            tcplistener.Start(Constants.listenObj);      
            events.Set();

            startListenButton = true;
        }
        #endregion

        #region Delegate
        protected void statusBox_Filter(object sender,NotifyCollectionChangedEventArgs e)
        {
            if (rollback) return;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (Constants.statusFilter.Any(e.NewItems[0].ToString().Contains))
                {
                    rollback = true;
                    statusBox.Items.Remove(e.NewItems[0].ToString());
                    rollback = false;

                }
            }    
        }


        private void OnChangeStatus(string sParam)
        {
            paramStatus = sParam;
            Object[] list = { this, System.EventArgs.Empty };
            statusBox.Dispatcher.BeginInvoke(new EventHandler(onChangeStatusBox), list);
        }
        private void onRecieveCmd(string sParam)
        {
            param = "收到：" + sParam;
            Object[] list = {this,System.EventArgs.Empty };
            statusBox.Dispatcher.BeginInvoke(new EventHandler(onRecieve), list);//.Items.Add(param);
        }
        protected void onRecieve(Object o, EventArgs e)
        {
            this.statusBox.Items.Add(param);
        }
        protected void onChangeStatusBox(Object o, EventArgs e)
        {
            this.statusBox.Items.Add(paramStatus);
            checkServiceStatus();
        }
        #endregion

        #region Events

        private void startListening_Click(object sender, RoutedEventArgs e)
        {
            if (startListenButton)
            {
                try
                {
                    events.Reset();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingFor), "正在停止监听...");
                    SendMessage("127.0.0.1:9999:*END");
                    startListenButton = false;
                    checkListenerStatus();

                }
                catch (Exception ex)
                {
                    statusBox.Items.Add("停止出错");
                }
                this.startListening.Header = Constants.StartListening;
            }
            else
            {
                events.Set();
                ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingFor), "正在开始监听...");
                startListenButton = true;
                SendMessage("127.0.0.1:9999:*BEGIN");
                this.startListening.Header = Constants.StopListening;
            }
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
           
            try
            {
                //QueueModule rm = new QueueModule();
                //rm.inQueue("FLUSHDB");
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
        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void getMissionList_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("127.0.0.1:9999:" + "hello" + new Random().Next());
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            QueueModule qm = new QueueModule();
            qm.inQueue("OK");
        }

        private void aboutItem_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About(this);
            ab.Owner = this;
            ab.ShowDialog();
        }

        private void startMenu_Click(object sender, RoutedEventArgs e)
        {
            checkServiceStatus();
            checkListenerStatus();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //fp.FullFileName = textBox1.Text;
            ThreadPool.QueueUserWorkItem(new WaitCallback(fp.BeginSendTo), "a:1");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(fp.Recieve),"a:1");

        }

        #endregion

        #region Service
        private void NoServiceHandler()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Constants.InstallServiceCommand;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingFor), "正在安装服务...");
            this.startService.Header = Constants.ServiceStart;
            this.statusBox.Items.Add(Constants.ServiceInstalled);
            checkServiceStatus();
        }
        private void startService_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (!sc2.Status.Equals(ServiceControllerStatus.Running))
                {
                    sc2.Start();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingFor), "正在启动服务...");
                    sc2.WaitForStatus(ServiceControllerStatus.Running);
                    this.startService.Header = Constants.ServiceStop;
                }
                else
                {
                    Thread th = new Thread(new ParameterizedThreadStart(StopServiceAndWaitForExit)); 
                    th.Start(sc2.ServiceName);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingForService),th);

                    this.startService.Header = Constants.ServiceStart;
                }
             }
            catch (Exception ex)
            {
                NoServiceHandler();   
            }

        }
        #endregion

    
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            MyNotifyIcon.Dispose();

            base.OnClosing(e);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }


        private void Taskbar_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                this.Hide();
            else
                this.Show();
        }
 
        private void Window_Initialized(object sender, EventArgs e)
        {
            string title = this.Title;
            string text = this.Title;
            MyNotifyIcon.ShowBalloonTip(title, text,BalloonIcon.Info);
        }

        private void MyNotifyIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
        { 
            if (this.Visibility == Visibility.Visible)
                this.tb_showHide.Header = "隐藏";
            else
                this.tb_showHide.Header = "显示";
            try
            {
                if (sc2.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Stopped))
                {
                    this.tb_startService.Header = Constants.ServiceStart;
                }
                else
                {
                    this.tb_startService.Header = Constants.ServiceStop;
                }
            }
            catch (Exception ex)
            {
                this.tb_startService.Header = Constants.ServiceInstall;
            }
            if (startListenButton)
            {
                this.tb_stopListen.Header = Constants.StopListening;
            }
            else
            {
                this.tb_stopListen.Header = Constants.StartListening;
            }
        }

        private void MyNotifyIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (this.Visibility != Visibility.Visible)
                this.Show();
        }

        #region Stop Service

        [DllImport("advapi32")]
        static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, ref SERVICE_STATUS_PROCESS lpBuffer, int cbBufSize, out int pcbBytesNeeded);

        const int SC_STATUS_PROCESS_INFO = 0;

        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_STATUS_PROCESS
        {
            public int dwServiceType;
            public int dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
            public int dwProcessId;
            public int dwServiceFlags;
        }

        const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

        const int SERVICE_RUNS_IN_SYSTEM_PROCESS = 0x00000001;

        public static void StopServiceAndWaitForExit(object obj)
        {
            string serviceName = obj.ToString();
            using (ServiceController controller = new ServiceController(serviceName))
            {
                SERVICE_STATUS_PROCESS ssp = new SERVICE_STATUS_PROCESS();
                int ignored;

                // Obtain information about the service, and specifically its hosting process,
                // from the Service Control Manager.
                if (!QueryServiceStatusEx(controller.ServiceHandle.DangerousGetHandle(), SC_STATUS_PROCESS_INFO, ref ssp, Marshal.SizeOf(ssp), out ignored))
                    throw new Exception("Couldn't obtain service process information.");

                // A few quick sanity checks that what the caller wants is *possible*.
                if (ssp.dwServiceType != SERVICE_WIN32_OWN_PROCESS)
                    throw new Exception("Can't wait for the service's hosting process to exit because there may be multiple services in the process (dwServiceType is not SERVICE_WIN32_OWN_PROCESS");

                if ((ssp.dwServiceFlags & SERVICE_RUNS_IN_SYSTEM_PROCESS) != 0)
                    throw new Exception("Can't wait for the service's hosting process to exit because the hosting process is a critical system process that will not exit (SERVICE_RUNS_IN_SYSTEM_PROCESS flag set)");

                if (ssp.dwProcessId == 0)
                    throw new Exception("Can't wait for the service's hosting process to exit because the process ID is not known.");

                // Note: It is possible for the next line to throw an ArgumentException if the
                // Service Control Manager's information is out-of-date (e.g. due to the process
                // having *just* been terminated in Task Manager) and the process does not really
                // exist. This is a race condition. The exception is the desirable result in this
                // case.
                using (Process process = Process.GetProcessById(ssp.dwProcessId))
                {
                    // EDIT: There is no need for waiting in a separate thread, because MSDN says "The handles are valid until closed, even after the process or thread they represent has been terminated." ( http://msdn.microsoft.com/en-us/library/windows/desktop/ms684868%28v=vs.85%29.aspx ), so to keep things in the same thread, the process HANDLE should be opened from the process id before the service is stopped, and the Wait should be done after that.

                    // Response to EDIT: What you report is true, but the problem is that the handle isn't actually opened by Process.GetProcessById. It's only opened within the .WaitForExit method, which won't return until the wait is complete. Thus, if we try the wait on the current therad, we can't actually do anything until it's done, and if we defer the check until after the process has completed, it won't be possible to obtain a handle to it any more.

                    // The actual wait, using process.WaitForExit, opens a handle with the SYNCHRONIZE
                    // permission only and closes the handle before returning. As long as that handle
                    // is open, the process can be monitored for termination, but if the process exits
                    // before the handle is opened, it is no longer possible to open a handle to the
                    // original process and, worse, though it exists only as a technicality, there is
                    // a race condition in that another process could pop up with the same process ID.
                    // As such, we definitely want the handle to be opened before we ask the service
                    // to close, but since the handle's lifetime is only that of the call to WaitForExit
                    // and while WaitForExit is blocking the thread we can't make calls into the SCM,
                    // it would appear to be necessary to perform the wait on a separate thread.
                    ProcessWaitForExitData threadData = new ProcessWaitForExitData();

                    threadData.Process = process;

                    Thread processWaitForExitThread = new Thread(ProcessWaitForExitThreadProc);

                    processWaitForExitThread.IsBackground = Thread.CurrentThread.IsBackground;
                    processWaitForExitThread.Start(threadData);

                    // Now we ask the service to exit.
                    controller.Stop();

                    // Instead of waiting until the *service* is in the "stopped" state, here we
                    // wait for its hosting process to go away. Of course, it's really that other
                    // thread waiting for the process to go away, and then we wait for the thread
                    // to go away.
                    lock (threadData.Sync)
                        while (!threadData.HasExited)
                            Monitor.Wait(threadData.Sync);
                }
            }
        }

        class ProcessWaitForExitData
        {
            public Process Process;
            public volatile bool HasExited;
            public object Sync = new object();
        }

        static void ProcessWaitForExitThreadProc(object state)
        {
            ProcessWaitForExitData threadData = (ProcessWaitForExitData)state;

            try
            {
                threadData.Process.WaitForExit();
            }
            catch { }
            finally
            {
                lock (threadData.Sync)
                {
                    threadData.HasExited = true;
                    Monitor.PulseAll(threadData.Sync);
                }
            }
        }
        #endregion

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NewTask_Click(object sender, RoutedEventArgs e)
        {

            NewTask nt = new NewTask();
            nt.Owner = this;
            if (nt.ShowDialog() == true)
            {

                nt.aTask.save();
                //string json = tmp.Task2Json();// Task.Task2Json(test);
                
                //Task clone = JsonSerializer.DeserializeFromString<Task>(json);//.FromJson<Task>();
                
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

  

    }

}
