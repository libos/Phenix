using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading; 

namespace Phenix.Core.Runner
{
    public class CMD : IRunner
    {
        private Task aTask;
        public CMD()
        { 

        }
        public void Run(Task aTask)
        {
            this.aTask = aTask;
            Step curStep =  aTask[aTask.curStep];
            ParamSupport ps = new ParamSupport(aTask.task_unique_no,curStep.inputParams);
            List<string[]> param = ps.getParams();
            List<int> seq = new List<int>();
            if (curStep.inputParamsFormat == null)
            {
                for (int i = 0; i < curStep.inputParams.Count; i++)
                {
                    seq.Add(i);
                }
            }
            else
            {
                seq = ps.getFormatSeq(curStep.inputParamsFormat);
            }
            
            for (int i = 0; i < ps.min_groups; i++)
            {
                string real_param = "";
                foreach(int s in seq)
                {
                    real_param += Constants.SPACE + param[s][i];
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(r =>
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(); 
                    startInfo.FileName = curStep.FilePath;
                    startInfo.Arguments = real_param;
                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    process.StartInfo = startInfo;
                    process.Start();

                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    curStep.outputResult.Add(result);

                    taskCallback();
                }));
            }
            
        }
 
        public void taskCallback()
        {
            this.aTask[this.aTask.curStep].finished += 1;
            this.aTask.status += 1;
            TaskList.updataList(this.aTask);
        }
    }
}
