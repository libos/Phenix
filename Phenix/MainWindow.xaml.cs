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
using Phenix.Core.Database;
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
using System.ComponentModel;
using System.Security.Cryptography;
using System.Data;
using MySql.Data.MySqlClient;
#endregion

namespace Phenix
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Definition
        
        public static ManualResetEvent events = new ManualResetEvent(false);
        public static bool startListenButton = false;
        RedisClient Redis = new RedisClient("localhost", 6380, Constants.Passwd);
        Pipeline pl;

        FilePipe fp;
        private Thread tcplistener = null;
        private Thread tcpSender = null;
        private static string param = "";
        private static string paramStatus = "";
        private bool rollback = false;
        public string onlineUser = string.Empty;
        #endregion

        #region Initialize Lists
        public ObservableCollection<Task> _TasksList = new ObservableCollection<Task>();
        List<string> supportL = new List<string>();
        public delegate void listHandler(System.Windows.Controls.ListBox lb, string item);
        public delegate void checkServiceDelegate(MainWindow mw);
        List<string> lsupport {
            get
            {
                return this.supportL;
            }
            set 
            {
                foreach (var item in value )
                {
                    object[] lc = new object[2];
                    lc[0] = this.supportList;
                    lc[1] = item;
                    this.supportList.Dispatcher.BeginInvoke(new listHandler((lb,ie) => 
                    {
                        lb.Items.Add(ie);

                    }), lc);
                }
                this.supportL = value;
            }
        }
        #endregion

        #region Initializing
        private delegate void UpdateGridUIEvent(Task t);
        private void UpdateGridUI(Task t)
        {
            _TasksList[_TasksList.IndexOf(t)] = TaskList.changedTask;
            this.tasksList.Items.Refresh();
        }  
        public MainWindow()
        {
   
            #region Task Import
            TaskList.preLoadTasks();
            List<Task> lt = TaskList.taskList;
            
            if (lt.Count > 0)
            {
                foreach (var item in lt)
                {
                    _TasksList.Add((Task)item);
                }
            }
            System.Windows.Threading.Dispatcher dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            ThreadPool.QueueUserWorkItem(r =>
                {
                    while (true)
                    {
                        if (TaskList.changed)
                        {
                            foreach (var item in _TasksList)
                            {
                                if (item.task_unique_no == TaskList.changedTaskId)
                                {
                                    var d = (System.Windows.Threading.Dispatcher)r;
                                    d.Invoke(new UpdateGridUIEvent(UpdateGridUI), item);
                                    break;
                                }
                            }
                            TaskList.changed = false;
                        }
                    }
                }, dispatcher);
            #endregion

            InitializeComponent();

            #region Support Environment
            pl = new Pipeline(events,this);
            Thread supportListThread = new Thread(new ThreadStart(supportListDetetct));
            supportListThread.Start();
            #endregion

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

            #region Filepile
            fp = new FilePipe(this);
            this.Hide();
            #endregion
        }
        public void supportListDetetct()
        {
            while (true)
            {
                List<string> support = Utils.getSupport();

                while (support != this.lsupport)
                {
                    this.lsupport = support;
                    //foreach (string item in support)
                    //{
                    //    if (this.lsupport.IndexOf(item) == -1)
                    //    {
                    //        this.lsupport.Add(item);
                    //    }
                    //}
                }
                Thread.Sleep(1200000);
            }

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
            ServiceController sc2 = new ServiceController(Constants.ServiceName);
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
            sc2.Close();
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

        public void EndServiceProcess(object serviceName)
        {
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                //System、Idle进程会拒绝访问其全路径
                if (p.ProcessName != "System" && p.ProcessName != "Idle" && p.ProcessName != "audiodg")
                {
                    try
                    {
                        if (p.MainModule.ModuleName == ((string)serviceName + ".exe"))
                        {
                            p.Kill();
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        this.Dispatcher.BeginInvoke(new checkServiceDelegate(mm =>
                        {
                       
                                    mm.startService.Header = Constants.ServiceStart;
                                    mm.service_img.Source = Properties.Resources.Stop.ToImageSource();
                                    mm.service_lb.Content = Constants.ServiceStopped;
 
 
                        }), this);                       
                    }
                }
            }
        }
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
            //checkServiceStatus();
        }
        private void startService_Click(object sender, RoutedEventArgs e)
        {
            ServiceController sc2 = new ServiceController(Constants.ServiceName);
            try
            { 
                if (!sc2.Status.Equals(ServiceControllerStatus.Running))        //启动
                {
                    sc2.Start();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingFor), "正在启动服务...");
                    sc2.WaitForStatus(ServiceControllerStatus.Running);
                    this.startService.Header = Constants.ServiceStop;
                }
                else            //停止
                {
                    Thread th = new Thread(new ParameterizedThreadStart(StopServiceAndWaitForExit)); 
                    th.Start(sc2.ServiceName);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(pl.WaitingForService), th);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(EndServiceProcess), sc2.ServiceName);
                    this.startService.Header = Constants.ServiceStart;
                }
             }
            catch (Exception ex)
            {
                NoServiceHandler();   
            }
            sc2.Close();
        }
        #endregion

        #region OnClosing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            //MyNotifyIcon.Dispose();
            
            e.Cancel = true;
            this.Hide();
            //base.OnClosing(e);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.Dispose();
            App.Current.Shutdown();
        }
        #endregion

        #region Notification
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
                ServiceController sc2 = new ServiceController(Constants.ServiceName);
                if (sc2.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Stopped))
                {
                    this.tb_startService.Header = Constants.ServiceStart;
                }
                else
                {
                    this.tb_startService.Header = Constants.ServiceStop;
                }
                sc2.Close();
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
        #endregion

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
                try
                {
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
            
                    Process process = Process.GetProcessById(ssp.dwProcessId);
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
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

        #region Clicks
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
   
        }

        private void NewTask_Click(object sender, RoutedEventArgs e)
        {
            if (this.onlineUser == string.Empty)
            {
                System.Windows.MessageBox.Show("请登录之后在创建任务谢谢", "请登录", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NewTask nt = new NewTask();
            nt.Owner = this;
            if (nt.ShowDialog() == true)
            {
                nt.aTask.creator = this.onlineUser;
                nt.aTask.save();
                _TasksList.Add(nt.aTask);

                string email = tbEmail.Text.Trim();
                string inputPass = pbPassword.Password.Trim();
                string password = Utils.MD5(inputPass);
                object send = Constants.ServerMainTCP + Constants.CreateTaskHeader
                    + Constants.UserNameHeader + email + "\n"
                    + Constants.TaskJson + nt.aTask.Task2Json() + "\n" 
                    + "\n**\n";
                ThreadPool.QueueUserWorkItem(new WaitCallback(pl.TCPClient), send);
                //string json = tmp.Task2Json();// Task.Task2Json(test);
                
                //Task clone = JsonSerializer.DeserializeFromString<Task>(json);//.FromJson<Task>();
                
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void remove_tasksList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void start_tasksList_Click(object sender, RoutedEventArgs e)
        {
            if (tasksList.SelectedItems.Count >0)
            {
                Task tmp = (Task)tasksList.SelectedValue;
                tmp.start();
            }
        }

        private void stop_tasksList_Click(object sender, RoutedEventArgs e)
        {
            if (tasksList.SelectedItems.Count > 0)
            {
                Task tmp = (Task)tasksList.SelectedValue;
                tmp.start();
            }
        }

        private void tasksList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (tasksList.SelectedItems.Count <= 0)
            {
                if (tasksList.ContextMenu.Items.Count >= 5)
                {
                    tasksList.ContextMenu.Items.Remove(start_tasksList);
                    tasksList.ContextMenu.Items.Remove(stop_tasksList);
                    tasksList.ContextMenu.Items.Remove(remove_tasksList);
                } 
            }
            else
            {
                if (tasksList.ContextMenu.Items.Count < 5)
                {
                    tasksList.ContextMenu.Items.Insert(0, remove_tasksList);
                    tasksList.ContextMenu.Items.Insert(0, stop_tasksList);
                    tasksList.ContextMenu.Items.Insert(0, start_tasksList);
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.share58.com/support");
        }

 

        private void refresh_tasksList_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_TasksList);
            view.Refresh();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            checkServiceStatus();
            checkListenerStatus();
        }
       #endregion
 
        #region Setting Tab
        private void SQLiteTest(object sender, RoutedEventArgs e)
        {
            SQLiteHelper slp = new SQLiteHelper(@"TestApp\test.db");
            string sql = @"SELECT ('*4\r\n' || '$' || LENGTH(redis_cmd) 
                            || '\r\n'  || redis_cmd || '\r\n' || '$' || LENGTH(redis_key) 
                            || '\r\n' || redis_key || '\r\n' ||  '$' || LENGTH(hkey) 
                            || '\r\n' ||  hkey || '\r\n' || '$' || LENGTH(hval)
                            || '\r\n'|| hval || '\r') FROM (  SELECT  'HSET' as redis_cmd,  'MapDB' AS redis_key,  x AS hkey,  y AS hval  FROM test) AS t;";
            slp.execute(sql);
        }


        private void MysqlTest(object sender, RoutedEventArgs e)
        {
            MySQLHelper msh = new MySQLHelper(s_server.Text, s_port.Text, s_username.Text, s_password.Password, s_database.Text);
            DataTable dt = new DataTable();
            dt.Load((MySqlDataReader)msh.execute("select * from params"));
            resultData.DataContext = dt;
            // msh.getDataSet("select * from params");
            //msh.saveProto("params","id","x","y","z");
            //msh.execute(s_sql.Text);   
        }

        private void RedisTest(object sender, RoutedEventArgs e)
        {
            RedisHelper rh = new RedisHelper();
            rh.mergeRDB("dump.rdb","set.rdb",1);
        }
        private string OpenFileDialog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document";  
            dlg.DefaultExt = ".conf";  
            dlg.Filter = "Configuration File (.conf)|*.conf|*.*|*.*"; 
             
            Nullable<bool> result = dlg.ShowDialog();
            string filename = null; 
            if (result == true)
            {
                // Open document 
                 filename = dlg.FileName;
            }
            return filename;
        }
        private string OpenFolderDialog()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string path = null;
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dialog.SelectedPath;
            }
            return path;
        }

        private void btnBinDir_Click(object sender, RoutedEventArgs e)
        {
            this.BinDirectory.Text = OpenFolderDialog();
        }
     
        private void btnConfFile_Click(object sender, RoutedEventArgs e)
        {
            this.ConfDirectory.Text = OpenFileDialog();
        }
        #endregion

        #region UserInterface
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.share58.com");
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = tbEmail.Text.Trim(); 
            string inputPass= pbPassword.Password.Trim();
            string password   = Utils.MD5(inputPass);
            object send = Constants.ServerMainTCP + Constants.LoginHeader 
                + Constants.UserNameHeader + email + "\n" 
                + Constants.PasswordHeader + password 
                + "\n**\n";
            ThreadPool.QueueUserWorkItem(new WaitCallback(pl.TCPClient),send);
        }
        public void ShowMsg(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            string email = tbEmail.Text.Trim();
            string inputPass = pbPassword.Password.Trim();
            string password = Utils.MD5(inputPass);
            object send = Constants.ServerMainTCP + Constants.LogoutHeader
                + Constants.UserNameHeader + email + "\n"
                + Constants.PasswordHeader + password
                + "\n**\n";
            ThreadPool.QueueUserWorkItem(new WaitCallback(pl.TCPClient), send);

        }
        #endregion
    }

}
