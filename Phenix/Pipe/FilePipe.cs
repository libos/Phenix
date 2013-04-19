using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phenix.Support;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace Phenix.Pipe
{
    public class FilePipe
    {
        public string FullFileName;
        private MainWindow _mw;

        public FilePipe(MainWindow mw)
        {
            _mw = mw;
        }

        public void BeginSendTo(object obj)
        {
            string[] tmp;
            tmp = obj.ToString().Split(':');
            string ip = tmp[0];
            int port = Convert.ToInt16(tmp[1]);
            FileSender task = new FileSender();
            try
            {
                Socket listensocket;
                listensocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3477);
                listensocket.Bind(ep);
                listensocket.Listen(20);
           
                    Socket newsocket = listensocket.Accept();
                    //task = new FileTransmission(FileTransmission.TransmissionMode.Send);
                    task.FullFileName = FullFileName;
                    task.Socket = newsocket;
                    task.EnabledIOBuffer = true;
                    task.BlockFinished += new BlockFinishedEventHandler(task_s_BlockFinished);
                    task.CommandReceived += new CommandReceivedEventHandler(task_s_CommandReceived);
                    task.ConnectLost += new EventHandler(task_s_ConnectLost);
                    task.ErrorOccurred += new FileTransmissionErrorOccurEventHandler(task_s_ErrorOccurred);
                    task.Start();
                   
               
            }
            catch (ThreadAbortException)
            {
                if (task != null && task.IsAlive)
                    task.Stop(true);
            }
        }
        #region Send Handler

        void task_s_ErrorOccurred(object sender, FileTransmissionErrorOccurEventArgs e)
        {
            _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), e.InnerException.ToString());
        }

        void task_s_BlockFinished(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            //_mwInvoke(new Delegate_Progress(Set_s_Progress), task);
        }
        delegate void Delegate_Progress(FileTransmission task);
        void Set_s_Progress(FileTransmission task)
        {
            //this.toolStripStatusLabel1.Text = string.Format("已发送:{0:N2}%", task.Progress);
           // this.toolStripStatusLabel2.Text = string.Format("{0:N2}KB/s", task.KByteAverSpeed);
        }
        void task_s_ConnectLost(object sender, EventArgs e)
        {
          _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), "ConnectLost");
        }

        public delegate void Delegate_String(string s);
        void task_s_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)),"s_" +  e.CommandLine);         
        }
        #endregion

        public void Recieve(object obj)
        {
            string[] tmp;
            tmp = obj.ToString().Split(':');
            string ip = tmp[0];
            int port = Convert.ToInt16(tmp[1]);
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3477);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ep);
                FileReceiver task = new FileReceiver();
                task.Socket = socket;
                task.EnabledIOBuffer = true;
                task.BlockFinished += new BlockFinishedEventHandler(task_r_BlockFinished);
                task.ConnectLost += new EventHandler(task_r_ConnectLost);
                task.AllFinished += new EventHandler(task_r_AllFinished);
                task.BlockHashed += new BlockFinishedEventHandler(task_r_BlockHashed);
                task.ErrorOccurred += new FileTransmissionErrorOccurEventHandler(task_r_ErrorOccurred);

                task.FilePath = "tmp/";

                task.Start();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #region Recieve Handler
        void task_r_ErrorOccurred(object sender, FileTransmissionErrorOccurEventArgs e)
        {
            if (e.InnerException is IOException)
            {
                if (MessageBox.Show(e.InnerException.Message, "IO异常", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                {
                    e.Continue = false;
                }
                else
                    e.Continue = true;
            }
            else
                MessageBox.Show(e.InnerException.ToString(), "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void task_r_AllFinished(object sender, EventArgs e)
        {
            _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), "r_接收完毕");
        }

        void task_r_ConnectLost(object sender, EventArgs e)
        {
            _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), "r_ConnectLost");
        }

        void task_r_BlockFinished(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            //_mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), "ConnectLost");
              //  this.Invoke(new Delegate_Progress(SetProgress), task);
            
        }
        void task_r_BlockHashed(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            _mw.statusBox.Dispatcher.BeginInvoke(new Delegate_String(s => _mw.statusBox.Items.Add(s)), "r_接收端 校验中");
              //  this.Invoke(new Delegate_Progress(SetProgressBar), task);
        }
        void SetProgressBar(FileTransmission task)
        {
        //    this.progressBar1.Maximum = task.Blocks.Count;
        ///    this.progressBar1.Value = task.Blocks.CountValid;
        }
        void SetProgress(FileTransmission task)
        {
            //this.Text = "接收端 下载中";
            SetProgressBar(task);
            //this.label3.Text = string.Format("进度:{0:N2}%   总长度:{1}   已完成:{2}", task.Progress, task.TotalSize, task.FinishedSize);
            //this.label1.Text = string.Format("平均速度:{0:N2}KB/s   已用时:{1}   估计剩余时间:{2}", task.KByteAverSpeed, task.TimePast, task.TimeRemaining);
            //this.label2.Text = string.Format("瞬时速度:{0:N2}KB/s   丢弃的区块:{1}", task.KByteSpeed, task.Blocks.Cast.Count);
        }
        #endregion
    }


}
