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
        public static bool changed = false;
        public static string changedTaskId = null;
        public static Task changedTask = null;
        public static void preLoadTasks()
        {
            List<string> taskJsons = new List<string>();
            try
            {
                QueueModule qm = new QueueModule();
                taskJsons = qm.getAllTasksList();
                taskJsons.Reverse();
                foreach (string item in taskJsons)
                {
                    taskList.Add(JsonSerializer.DeserializeFromString<Task>(item));
                }
            }
            catch (Exception)
            {
            }
        }
        public static void updataList(string TaskId)
        {
            List<string> taskJsons = new List<string>();
            taskList.Clear();
            try
            {
                QueueModule qm = new QueueModule();
                taskJsons = qm.getAllTasksList();
                foreach (string item in taskJsons)
                {
                    if (item.Contains(TaskId))
                    {
                        changedTask = JsonSerializer.DeserializeFromString<Task>(item);
                    }
                }
            }
            catch (Exception)
            {
            }
            changedTaskId = TaskId;
            changed = true;
        }
        public static void updataList(Task atask)
        {
            List<string> taskJsons = new List<string>();
            taskList.Clear();
            
            changedTask = atask;

            changedTaskId = atask.task_unique_no;
            changed = true;
        }
    }
}
