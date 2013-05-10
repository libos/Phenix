using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Phenix.Core
{
    public class Log
    {
        private static object _lockerForLog = new object();
        public static string _logPath = "Phenix.log";
        public static void Write(string content)
        {
            try
            {
                lock (_lockerForLog)
                {
                    FileStream fs;
                    fs = new FileStream(Path.Combine(_logPath, DateTime.Now.ToString("yyyyMMdd") + ".log"), FileMode.Append);
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
        public static void Write(string content,string taskID)
        {
            try
            {
                lock (_lockerForLog)
                {
                    FileStream fs;
                    fs = new FileStream(Path.Combine(_logPath, DateTime.Now.ToString("yyyyMMdd") + ".log"), FileMode.OpenOrCreate);
                    StreamWriter streamWriter = new StreamWriter(fs);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    streamWriter.WriteLine("[" + DateTime.Now.ToString() + "]：" + "任务" + taskID + ":" + content);
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
}
