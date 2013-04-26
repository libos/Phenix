using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Phenix.UI;
namespace Phenix.Core
{
    public class Param
    {
 
        public Param()
        {
            type = 0;
            unit = 1;
            aArray = new List<string>();
            seperator = ";";

            Exceptions = new List<Exception>();

            aDatabase = new DataBase();
            aDatabase.tableField = new List<string>();
            aDatabase.type = 0;
            
            fileList = new FileList();
            fileList.subDirectory = false;
            
        }
        public static Param CreateParam()
        {
            var param = new Param();
            return param;
        }
        public List<Exception> Exceptions;

        [Category("基本属性"), PropertyOrder(0)]
        [DisplayName("参数来源")]
        [Description("输入参数来源")]
        [ItemsSource(typeof(paramType))]
        public int type
        {
            set;
            get;
        }
        [Category("基本属性"), PropertyOrder(1)]
        [DisplayName("参数单元")]
        [Description("调用时，每次输入多少个单元的数据")]
        public int unit
        {
            get;
            set;
        }

        [Category("基本属性"), PropertyOrder(2)]
        [DisplayName("单元间隔符")]
        [Description("某个参数数据不同单元作为一个参数时，不同单元间隔，默认值为“;”")]
        public string seperator
        {
            get;
            set;
        }

        [Category("数据源"), PropertyOrder(3)]
        [DisplayName("集合")]
        [Description("如果数据来源是文件，请填写此属性值")]
        public List<string> aArray { set; get; }

        [Category("数据源"), PropertyOrder(4)]
        [DisplayName("文件列表")]
        [Description("如果数据来源是文件，请填写此属性值")]
        [ExpandableObject()]
        public FileList fileList
        {
            get;
            set;
        }

        [Category("数据源"), PropertyOrder(5)]
        [DisplayName("数据库")]
        [Description("如果数据来源来自数据库，请填写")]
        [ExpandableObject()]
        [DefaultValue("SQLite")]
        public DataBase aDatabase
        {
            set;
            get;
        }

    } 
    [DefaultProperty("type")] 
    public class DataBase
    { 
        public DataBase()
        {
           
        }
        [Category("数据库"), PropertyOrder(0)]
        [DisplayName("数据库类型")]
        [Description("")]
        [ItemsSource(typeof(supportedDatabase))]
        public int type
        {
            set;
            get;
        }
        [Category("数据库"), PropertyOrder(2)]
        [DisplayName("SQLite")]
        [Description("")]
        [EditorAttribute(typeof(FileChooserEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string SQLite
        {
            set;
            get;
        }
 
        [Category("数据库"), PropertyOrder(4)]
        [DisplayName("Redis")]
        [Description("")]
        public string Redis
        {
            set;
            get;
        }

        [Category("数据库"), PropertyOrder(6)]
        [DisplayName("MySQL端口号")]
        [Description("")]
        public string MySQL
        {
            set;
            get;
        }
        
        [Category("详情"), PropertyOrder(7)]
        [DisplayName("数据库名")]
        [Description("所使用的数据库名")]
        public string databaseName { set; get; }
        
        [Category("数据库"), PropertyOrder(9)]
        [DisplayName("表名")]
        [Description("所使用的表名")]
        public string tableName { set; get; }

        [Category("数据库"), PropertyOrder(10)]
        [DisplayName("字段")]
        [Description("所使用的字段")]
        public List<string> tableField { set; get; }

    }
    [DefaultProperty("folderPath")]
    public class FileList
    {
        public FileList()
        {
        }

        [Category("文件夹"), PropertyOrder(3)]
        [DisplayName("数据集所在文件夹")]
        [Description("选择数据文件所在的根目录")]
        [EditorAttribute(typeof(FolderChooserEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string folderPath
        {
            get;
            set;
        }
        [Category("文件夹"), PropertyOrder(4)]
        [DisplayName("是否使用子文件夹")]
        [Description("")]
        public bool subDirectory { set; get; }

        [Category("文件"), PropertyOrder(4)]
        [DisplayName("文件夹内文件")]
        [Description("可以使用正则表达式")]
        public List<string> filePattern
        {
            get;
            set;
        }
    }
    public class paramType : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection type = new ItemCollection();
            for (int i = 0; i < StepSupport.DataType.Length; i++)
            {
                type.Add(i, StepSupport.DataType[i]);
            }

            return type;
        }
    }
}
