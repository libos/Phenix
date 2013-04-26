using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;
namespace Phenix
{
    public static class Constants
    {
        public static readonly byte[] Quit = "QUIT".ToUtf8Bytes();
        public static readonly string Passwd = "";


        public static int listenPort = 9999;
        public static string listenObj = "127.0.0.1:" + listenPort.ToString();
        public static readonly string selfTaskList = "selfTasks";
        public static readonly string allTaskList = "allTasks";

        public static readonly string SPACE = " ";
        //public static readonly byte[] QueueList
        //priority

        public static readonly string[] statusFilter = { "*BEGIN", "*END" };
        public static readonly string StartListening = "开始监听";
        public static readonly string StopListening = "停止监听";
        /// <summary>
        /// Service
        /// </summary>
        public static readonly string ServiceName = "PhenixDeamon";
        public static readonly string ServiceInstall = "安装后台服务";
        public static readonly string ServiceStart = "启动后台服务";
        public static readonly string ServiceStarted = "后台服务启动";
        public static readonly string ServiceStop = "停止后台服务";
        public static readonly string ServiceStopping = "正在停止后台服务";
        public static readonly string ServiceStopped = "后台服务停止";
        public static readonly string ServiceInstalled = "服务安装成功！";
        public static readonly string ServiceUninstalled = "服务卸载成功！";
        public static readonly string InstallServiceCommand = "%cd%\\InstallUtil.exe %cd%\\PhenixDeamon.exe" ;
        public static readonly string UninstallServiceCommand = "%cd%\\InstallUtil.exe %cd%\\PhenixDeamon.exe  -u";


        /// <summary>
        ///  Errors
        /// </summary>
        public static readonly string NoServiceError = "服务尚未安装！";
        public static readonly string CannotListen = "无法开始监听";


        public static readonly string DelProgramData = "del /F/Q  \"%ProgramData%\\Microsoft Visual Studio\\10.0\\TraceDebugging\\*\"";


 
    }
}
