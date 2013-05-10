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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;

namespace Phenix
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Window
    {
        private MainWindow _mw;
        public About(MainWindow mw)
        {
            InitializeComponent();
            _mw = mw;
        }

        private void label2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Constants.UninstallServiceCommand;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            ThreadPool.QueueUserWorkItem(new WaitCallback(_mw.EndServiceProcess), Constants.ServiceName);
            _mw.statusBox.Items.Add(Constants.ServiceUninstalling);
 
            process.Start();

           object list = new object[] { _mw,process};
           ThreadPool.QueueUserWorkItem(new WaitCallback(WaitingForServiceUninstalled), list);
            
        }
        private delegate void StringDelegate(MainWindow m);
        private void WaitingForServiceUninstalled(object obj)
        {
            object[] list = obj as object[];
            MainWindow mw = (MainWindow)list[0];
            Process p = (Process)list[1];
            Thread.Sleep(2000);
            mw.Dispatcher.BeginInvoke(new StringDelegate(m =>
            {
                m.statusBox.Items.Add(Constants.ServiceUninstalled);
                m.service_img.Source = Properties.Resources.Inactive.ToImageSource();
                m.service_lb.Content = Constants.NoServiceError;
                m.startService.Header = Constants.ServiceInstall;
            }), mw);
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://me.alipay.com/liber");
        }
    }
}
