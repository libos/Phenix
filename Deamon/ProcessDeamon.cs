using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Configuration;
using System.Threading;

namespace PhenixDeamon
{
    public partial class ProcessDeamon : ServiceBase
    {
        
        private string[] _processAddress;
        private object _lockerForLog = new object();
        private string _logPath = string.Empty;
        private string _redisServer;
        private string _redisConf;
        private string _redisCmd;

        public ProcessDeamon()
        {
            InitializeComponent();

            try
            {
                //读取监控进程全路径
                string strProcessName = "Phenix.exe";
                string strProcessAddress = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    strProcessName);
                _redisServer = ConfigurationManager.AppSettings["RedisServer"];
                _redisConf = ConfigurationManager.AppSettings["RedisConf"];
                if (_redisConf.Trim() != "")
                {
                    _redisCmd = _redisServer + " " + _redisConf;
                }
                else
                {
                    _redisCmd = _redisServer;
                }

                string RelatedProcessAddress = ConfigurationManager.AppSettings["RelatedProcessAddress"].ToString();
                if (_redisCmd.Trim() != "")
                {
                    List<string> addr = new List<string>();
                    addr.Add(strProcessAddress);
                    addr.Add(this._redisServer);
                    foreach (string str in RelatedProcessAddress.Split(','))
                    {
                        string tstr = str.Trim();
                        if (tstr != "")
                        {
                            addr.Add(tstr);
                        }
                    }
                    this._processAddress = addr.ToArray();
                }
                else
                {
                    throw new Exception("请配置Redis-Server路径！");
                }
              

                //创建日志目录
                this._logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PhenixDeamonLog");
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch (Exception ex)
            {
                this.SaveLog("Watcher()初始化出错！错误描述为：" + ex.Message.ToString());
            } 
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.StartWatch();
            }
            catch (Exception ex)
            {
                this.SaveLog("OnStart() 出错，错误描述：" + ex.Message.ToString());
            }
        } 
        protected override void OnStop()
        {
            try
            {

            }
            catch (Exception ex)
            {
                this.SaveLog("OnStop 出错，错误描述：" + ex.Message.ToString());
            }
        } 
        /// <summary>
        /// 开始监控
        /// </summary>
        private void StartWatch()
        {
            if (this._processAddress != null)
            {
                if (this._processAddress.Length > 0)
                {
                    foreach (string str in _processAddress)
                    {
                        if (str.Trim() != "")
                        {
                            if (File.Exists(str.Trim()))
                            {
                                this.ScanProcessList(str.Trim());
                            }
                        }
                    }
                }
            }
        } 
        /// <summary>
        /// 扫描进程列表，判断进程对应的全路径是否与指定路径一致
        /// 如果一致，说明进程已启动
        /// 如果不一致，说明进程尚未启动
        /// </summary>
        /// <param name="strAddress"></param>
        private void ScanProcessList(string address)
        {
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                //System、Idle进程会拒绝访问其全路径
                if (p.ProcessName != "System" && p.ProcessName != "Idle" && p.ProcessName != "audiodg")
                {
                    try
                    {
                        if (this.FormatPath(address) == this.FormatPath(p.MainModule.FileName.ToString()))
                        {
                            //进程已启动
                            /**/
                            if (address == this._redisServer)
                            {
                                this.WatchProcess(p, address,this._redisCmd);
                            }
                            else
                            {
                                this.WatchProcess(p, address);
                            }
                            return;
                        }
                    }
                    catch
                    {
                        //拒绝访问进程的全路径
                        this.SaveLog("进程(" + p.Id.ToString() + ")(" + p.ProcessName.ToString() + ")拒绝访问全路径！");
                    }
                }
            }
            //进程尚未启动
            if (address == this._redisServer)
            {
                Process process = new Process();
                ProcessStartInfo startInfo =new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C "+this._redisCmd;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                process.StartInfo = startInfo;
                process.Start();
                SaveLog(process.StandardOutput.ReadToEnd());
                this.WatchProcess(process, address,_redisCmd);
            }
            else
            {
                //http://stackoverflow.com/questions/3798612/service-starting-a-process-wont-show-gui-c-sharp
                ApplicationLoader.PROCESS_INFORMATION procInfo;
                ApplicationLoader.StartProcessAndBypassUAC(address, out procInfo);
                //Process process = new Process();
                //process.StartInfo.FileName = address;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                //process.Start();
                Process process = getProcess(procInfo.dwProcessId);
                if (process == null)
                    SaveLog("无法启动用户进程"+address);
                this.WatchProcess(process, address);
            }
        }
        //通过WIN32API获取的ProcInfo 里的dwProcessId获取Process
        static public Process getProcess(uint id)
        {
             Process[] arrayProcess = Process.GetProcesses();
             foreach (Process p in arrayProcess)
             {
                 if (p.Id == id)
                 {
                     return p;
                 }
             }
             return null;
        }
        /// <summary>
        /// 监听进程
        /// </summary>
        /// <param name="p"></param>
        /// <param name="address"></param>
        private void WatchProcess(Process process, string address)
        {
            ProcessRestart objProcessRestart = new ProcessRestart(process, address);
            Thread thread = new Thread(new ThreadStart(objProcessRestart.RestartProcess));
            thread.Start();
        }
        private void WatchProcess(Process process, string address,string cmd)
        {
            ProcessRestart objProcessRestart = new ProcessRestart(process, address, cmd);
            Thread thread = new Thread(new ThreadStart(objProcessRestart.RestartProcess));
            thread.Start();
        } 
        /// <summary>
        /// 格式化路径,去除前后空格,去除最后的"\",字母全部转化为小写
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string FormatPath(string path)
        {
            return path.ToLower().Trim().TrimEnd('\\');
        } 
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="content"></param>
        public void SaveLog(string content)
        {
            try
            {
                lock (_lockerForLog)
                {
                    FileStream fs;
                    fs = new FileStream(Path.Combine(this._logPath, DateTime.Now.ToString("yyyyMMdd") + ".log"), FileMode.OpenOrCreate);
                    StreamWriter streamWriter = new StreamWriter(fs);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    streamWriter.WriteLine("[" + DateTime.Now.ToString() + "]：" + content);
                    streamWriter.Flush();
                    streamWriter.Close();
                    fs.Close();
                }
            }
            catch
            {
            }
        }
    }

    public class ProcessRestart
    {
        //字段
        private Process _process;
        private string _address;
        private string _cmd = string.Empty;
        public ProcessRestart()
        { 
        } 
        public ProcessRestart(Process process, string address)
        {
            this._process = process;
            this._address = address;
        }
        public ProcessRestart(Process process, string address , string cmd)
        {
            this._process = process;
            this._address = address;
            this._cmd = cmd;
        }  

        public void RestartProcess()
        {
            try
            {
                while (true)
                {
                    if (_cmd != string.Empty)
                    {
                        this._process.WaitForExit();
                        this._process.Close();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/C " + this._cmd;
                        startInfo.RedirectStandardError = true;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.UseShellExecute = false;
                        this._process.StartInfo = startInfo;
                        this._process.Start();
                    }
                    else
                    { 
                        this._process.WaitForExit();

                        this._process.Close();    //释放已退出进程的句柄
                        ApplicationLoader.PROCESS_INFORMATION procInfo;
                        ApplicationLoader.StartProcessAndBypassUAC( this._address, out procInfo);
                        this._process = ProcessDeamon.getProcess(procInfo.dwProcessId);
                        if (this._process == null)
                        {
                            ProcessDeamon objProcessDeamon = new ProcessDeamon();
                            objProcessDeamon.SaveLog("无法启动用户进程" +  this._address);
                        }

                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                ProcessDeamon objProcessDeamon = new ProcessDeamon();
                objProcessDeamon.SaveLog("RestartProcess() 出错，监控程序已取消对进程("
                    + this._process.Id.ToString() + ")(" + this._process.ProcessName.ToString()
                    + ")的监控，错误描述为：" + ex.Message.ToString());
            }
        }
    }
}

 

 

     