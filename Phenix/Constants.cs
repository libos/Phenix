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
        //LoginHeader + Constants.UserNameHeader + email + Constants.PasswordHeader
        public static readonly string ServerIP = "210.51.4.178";
        public static readonly int PortServerMain = 9980;
        public static readonly int PortServerFile = 9981;
        public static readonly int PortServerControl = 9982;

        public static readonly string ServerMainTCP = ServerIP + ":" + PortServerMain + ":";
        public static readonly string LoginHeader = "*ClientLogin*\n";
        public static readonly string UserNameHeader = "*UserName*\n";
        public static readonly string PasswordHeader = "*Password*\n";
        public static readonly string LoginSUC = "*LoginSUC*";
        public static readonly string LoginFAIL = "*LoginFAIL*";
        public static readonly string LogoutHeader = "*ClientLogout*\n";
        public static readonly string LogoutSUC = "*LogoutSUC*";
        public static readonly string LogoutFAIL = "*LogoutFAIL*";

        public static readonly string CreateTaskHeader = "*CreateTask*\n";
        public static readonly string UpdateTaskHeader = "*UpdateTask*\n";
        public static readonly string TaskJson = "*TaskJson*\n";
        
        public static readonly string CreateTaskSuc = "*CreateTaskSuc*";
        public static readonly string UpdateTaskSuc = "*UpdateTaskSuc*";
        public static readonly string CreateTaskFail = "*CreateTaskFail*";
        public static readonly string UpdateTaskFail = "*UpdateTaskFail*";

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
        public static readonly string ServiceUninstalling = "正在卸载后台服务";
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
