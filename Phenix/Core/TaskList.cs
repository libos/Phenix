using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace Phenix.Core
{
    public class TaskList
    {
        public static List<Task> taskList = new List<Task>();

        public static void preLoadTasks()
        {
            List<string> taskJsons = new List<string>();
            try
            {
                QueueModule qm = new QueueModule();
                taskJsons = qm.getAllTasksList();
                foreach (string item in taskJsons)
                {
                    taskList.Add(JsonSerializer.DeserializeFromString<Task>(item));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
