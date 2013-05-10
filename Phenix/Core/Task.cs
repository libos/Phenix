using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ServiceStack.Text;
using Phenix.Core.Runner;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics; 
namespace Phenix.Core
{
    public class Task : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //
        private string _name;
        private int _needNum;
      

        public Task()
        {
            status = 0;
        }
        public string task_unique_no { set; get; }
        public DateTime created_at { set; get; }

        public string creator { get; set; }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public int priority
        {
            set;
            get;
        }
        int _curStep;
        public int curStep
        {
            set
            {
                if (_curStep != value)
                {
                    _curStep = value;
                    OnPropertyChanged();
                }
            }
            get
            {
                return _curStep;
            }
        }
        public int NeedNum
        {
            get
            {
                return _needNum;
            }
            set
            {
                _needNum = NeedNum;
            }
        }
        public int Count
        {
            get
            {
                return List.Count;
            }
        }
        public List<Step> List
        {
            set;
            get;
        }
        public void Add(Step aStep)
        {
            List.Add(aStep);
        }
        public void Remove(int index)
        {
            if (index > Count - 1 || index < 0)
            {
                System.Windows.Forms.MessageBox.Show("Index not valid!");
            }
            else
            {
                List.RemoveAt(index);
            }
        }
        public Step this[int index]
        {
            get
            {
                return (Step)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public Step Last
        {
            get
            {
                return (Step)List[List.Count - 1];
            }
        }
        public string generateUniqueID(string macAdress)
        {
            return Utils.EncryptDES(Utils.UnixTime(created_at).ToString(), macAdress);
        }

        public  string Task2Json()
        {
                JsConfig.IncludeNullValues = true;  
                var json = JsonSerializer.SerializeToString(this);
                return json;
        }
        public int totalStep
        {
            get
            {
                int total = 0 ;
                this.List.ForEach(iterat =>
                {
                    total += iterat.totalGroup;
                });
                return total;
            }
        }
        int _status;
        public int status
        {
            set
            {
                _status = value;
                OnPropertyChanged();
            }
            get
            {
                return _status;
            }
        }
        public void save()
        {
            QueueModule qm = new QueueModule();
            qm.push2selfTaskList(this.Task2Json());

        }
        public void update()
        {
            QueueModule qm = new QueueModule();
            qm.updateTaskList(this.Task2Json(), this.task_unique_no);
        }
        public void start()
        {
            IRunner runner;
            string className = "Phenix.Core.Runner." + StepSupport.Runner[this.List[curStep].Runner];
            runner = (IRunner)(Activator.CreateInstance(Type.GetType(className)));
            runner.Run(this);
        }

        public void stop()
        {

        }

        private void OnPropertyChanged()
        {
            var stackTrace = new StackTrace();
            string propertyName = stackTrace.GetFrame(1).GetMethod().Name;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public static Task ParseJson(string json)
        //{
        // //   List<Step> steps = JsonObject.Parse(json).ArrayObjects("");

        //    return new Task
        //    {
             
        //    };
        //}
    }

}
