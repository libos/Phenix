using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Phenix.Core
{
    public static class StepSupport
    {
        public static string[] Runner = { "CMD","Terminal","JAVA","Ruby","Python","Perl","Matlab"};
        public static string[] SysOS = {"Windows","Linux","OSX","Android","无限制" };
        public static string[] DataType = { "Array","FileList","Database"};
        public static string[] DatabaseSupported = { "SQLite", "Redis", "MySQL" };
        public  enum EnumLanguage
        {
            /// <summary>
            /// 直接运行
            /// </summary>
            NONE = 0x00,
            /// <summary>
            /// cmd
            /// </summary>
            CMD = 0x01,
            /// <summary>
            /// Java
            /// </summary>
            Java = 0x02,
            /// <summary>
            /// Ruby
            /// </summary>
            Ruby = 0x03,
            /// <summary>
            /// Python
            /// </summary>
            Python = 0x04,
            /// <summary>
            /// Perl
            /// </summary>
            Perl = 0x05,
            /// <summary>
            /// matlab -nodesktop -nosplash  -r fun() -logfile aa.out
            /// </summary>
            MatLab = 0x06
        }
        public  enum EnumOS
        {
            Windows = 0x00,
            Linux = 0x01,
            OSX = 0x02,
            Android = 0x03,
            无限制 = 0x04
        }
        public  enum EnumParams
        {
            Array = 0x00,
            FileList = 0x01,
            database = 0x02
        }
        public enum StepStatus
        {
            Undo = 0,
            Doing = 1,
            Done = 2,
            Error = 3
        }
        public enum DatabaseEnum
        {
            SQLite  =   0,
            Redis   =   1,
            MySQL   =   2
        }
    }

    public class usedLanguage : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection langs = new ItemCollection();
            for (int i= 0; i < StepSupport.Runner.Length; i++)
            {
                langs.Add(i,StepSupport.Runner[i]);
            }
        
            return langs;
        }
    }
    public class supportedOS : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection oss = new ItemCollection();
            for (int i= 0; i < StepSupport.SysOS.Length; i++)
            {
                oss.Add(i,StepSupport.SysOS[i]);
            }
        
            return oss;
        }
    }
    public class supportedDatabase : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection dbs = new ItemCollection();
            for (int i = 0; i < StepSupport.DatabaseSupported.Length; i++)
            {
                dbs.Add(i, StepSupport.DatabaseSupported[i]);
            }

            return dbs;
        }
    }
    
    
}
