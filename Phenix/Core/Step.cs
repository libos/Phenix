using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Phenix.UI;
using System.Diagnostics;

namespace Phenix.Core
{
    public class Step : INotifyPropertyChanged
    {
        int _No;
        int _Runner;
        int _os;
        public event PropertyChangedEventHandler PropertyChanged;

        public Step()
        {
            outputResult = new List<string>();
            sendTo = 1;
            finished = 0;
        }
        [Browsable(false)]
        public int sendTo { set; get; }
        [Browsable(false)]
        public int finished { set; get; }
        [Browsable(false)]
        public int totalGroup
        {
            get
            {
                int group = int.MaxValue;
                this.inputParams.ForEach(r =>
                    {
                        switch (r.type)
                        {
                            case 0:
                                int curgroup = (int)Math.Floor(r.aArray.Count * 1.0 / r.unit);
                                group = group > curgroup ? curgroup : group;
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                        }
                    });
                return group;
            }
        }
        [Browsable(false)]
        public StepSupport.StepStatus status
        {
            get
            {
                int tmpStatu = (int)Math.Floor((finished*1.0 / sendTo));
                switch (tmpStatu)
                {
                    case 0:
                        return StepSupport.StepStatus.Undo;
                    case -1:
                        return StepSupport.StepStatus.Error;
                    case 1:
                        return StepSupport.StepStatus.Done;
                    default:
                        return StepSupport.StepStatus.Doing;
                }
            }
        }
        public static Step CreateStep()
        {
            var aStep = new Step();
       
            return aStep;
        }
        
        [Browsable(false)]
        public int setStepNo
        {
            set
            {
                this._No = value;
            }
        }
        [Category("基本属性"), PropertyOrder(0)]
        [DisplayName("Step No.")]
        [Description("第几步，")]
        public int StepNo
        {
            get
            {
                return _No;
            }
        }
        [Category("基本属性"), PropertyOrder(1)]
        [DisplayName("执行程序")]
        [Description("使用的执行程序")]
        [ItemsSource(typeof(usedLanguage))]
        public int Runner
        {
            get 
            {
                return _Runner;
            }
            set 
            {
                _Runner = value;
            }
        }
        [Category("基本属性"), PropertyOrder(2)]
        [DisplayName("程序文件")]
        [Description("")]
        [EditorAttribute(typeof(FileChooserEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string FilePath
        {
            get;
            set;
        }

        [Category("基本属性"), PropertyOrder(2)]
        [DisplayName("操作系统")]
        [Description("程序所需要的操作系统支持")]
        [ItemsSource(typeof(supportedOS))]
        public int OS
        {
            get
            {
                return _os;
            }
            set
            {
                _os = value;
            }
        }
        [Category("类库"), PropertyOrder(4)]
        [DisplayName("程序所需类库")]
        [Description("程序执行时所需要的非常规类库")]
        public List<string> libs
        {
            set;
            get;
        }
        [Category("输入参数"), PropertyOrder(5)]
        [DisplayName("输入参数")]
        [Description("输入参数的顺序及类型")]
        public List<Param> inputParams
        {
            get;
            set;
        }

        [Category("输入参数"), PropertyOrder(6)]
        [DisplayName("输入参数格式")]
        [Description("输入参数顺序内容，例{0} {1} {2}，大括号内部为输入参数的索引值")]
        public string inputParamsFormat
        {
            get;
            set;
        }
        [Category("输出格式"), PropertyOrder(7)]
        [DisplayName("输出参数格式")]
        [Description("请按照C语言类型匹配")]
        public string outputParamsFormat
        {
            get;
            set;
        }
        public List<string> outputResult;
        [Category("输出格式"), PropertyOrder(8)]
        [DisplayName("是否需要保存文件")]
        [Description("")]
        public bool Need_OutputFile
        {
            set;
            get;
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
    }
}
