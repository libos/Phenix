using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ServiceStack.Text;
namespace Phenix.Core
{
    public class Task 
    {

        //
        private string _name;
        private int _needNum;

        //each step should have 
        
        public static Task generateTasks()
        {
            string json = "{\"task_unique_no\":\"Hb88EOaFCnNflZ2vQb6Rpw==\",\"created_at\":\"\\/Date(1366375884348)\\/\",\"Name\":\"任务n我饿了歌曲\",\"priority\":0,\"NeedNum\":0,\"Count\":4,\"List\":[{\"StepNo\":0,\"Runner\":0,\"OS\":0,\"libs\":[],\"inputParams\":[],\"inputParamsFormat\":null,\"outputParamsFormat\":null,\"Need_OutputFile\":false},{\"StepNo\":1,\"Runner\":0,\"OS\":0,\"libs\":[],\"inputParams\":[{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}}],\"inputParamsFormat\":null,\"outputParamsFormat\":null,\"Need_OutputFile\":false},{\"StepNo\":2,\"Runner\":0,\"OS\":0,\"libs\":[],\"inputParams\":[{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}}],\"inputParamsFormat\":null,\"outputParamsFormat\":null,\"Need_OutputFile\":false},{\"StepNo\":3,\"Runner\":0,\"OS\":0,\"libs\":[],\"inputParams\":[{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}}],\"inputParamsFormat\":null,\"outputParamsFormat\":null,\"Need_OutputFile\":false}],\"Last\":{\"StepNo\":3,\"Runner\":0,\"OS\":0,\"libs\":[],\"inputParams\":[{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}},{\"type\":0,\"unit\":1,\"seperator\":\";\",\"aArray\":[],\"fileList\":{\"folderPath\":null,\"subDirectory\":false,\"filePattern\":null},\"aDatabase\":{\"type\":0,\"SQLite\":null,\"Redis\":null,\"MySQL\":null,\"databaseName\":null,\"tableName\":null,\"tableField\":[]}}],\"inputParamsFormat\":null,\"outputParamsFormat\":null,\"Need_OutputFile\":false},\"status\":0}";
            return  JsonSerializer.DeserializeFromString<Task>(json);
        }
        public Task()
        {
        }
        public string task_unique_no { set; get; }
        public DateTime created_at { set; get; }
        
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
      /*  public Step Item(int index)
        {

            return (Step)List[index];
        }
        */
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
        public int status
        {
            set;
            get;
        }
        public void save()
        {
            QueueModule qm = new QueueModule();
            qm.push2selfTaskList(this.Task2Json());
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
