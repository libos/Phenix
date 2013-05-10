using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Data;
namespace Phenix.Core.Database
{
    public class SQLiteHelper : IDatabaseHelper
    {
        string filepath;
        SQLiteConnection conn; 
        public SQLiteHelper(string filepath)
        {
            this.filepath = filepath;
            conn = new SQLiteConnection();
            conn.ConnectionString = @"Data Source=" + filepath + ";Version=3;";//;Password=my_password
            conn.Open();
        }
        public object execute(string sql)
        {
            SQLiteCommand sc = new SQLiteCommand(sql, conn);

            SQLiteDataReader dr = sc.ExecuteReader();
            
            return dr;
        }
        public DataSet getDataSet(string sql)
        {
            SQLiteCommand sc = new SQLiteCommand(sql, conn);
            var adapter = new SQLiteDataAdapter(sc);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds;
        }
        public void saveProto(string tablename, string primaryKey, params string[] ValueList)
        {
            FileStream fsFile = new FileStream(@"tmp.rdb", FileMode.Append);
            StreamWriter sw = new StreamWriter(fsFile);
            for (int i = 0; i < ValueList.Length; i++)
            {
                string sql = @"SELECT ('*4\r\n' || '$' || LENGTH(redis_cmd) 
                            || '\r\n'  || redis_cmd || '\r\n' || '$' || LENGTH(redis_key) 
                            || '\r\n' || redis_key || '\r\n' ||  '$' || LENGTH(hkey) 
                            || '\r\n' ||  hkey || '\r\n' || '$' || LENGTH(hval)
                            || '\r\n'|| hval || '\r') FROM ( 
                                SELECT  'HSET' as redis_cmd,  
                                '" + tablename + i.ToString() + @"' AS redis_key,  
                                " + primaryKey + @" AS hkey,  
                                 " + ValueList[i] + @" AS hval  
                                FROM " + tablename + @"
                                ) AS t";

                SQLiteDataReader sdr = (SQLiteDataReader)execute(sql);
                while (sdr != null && sdr.Read())
                {
                    sw.Write(sdr[0]);
                    Console.Write(sdr[0]);
                }
                sdr.Close();
            }
            sw.Close();
            fsFile.Close();
        }

        public void sendProtoFile()
        {

        }
        public void readyReceive()
        {

        }
        public void receiveRDBFile()
        {

        }
        ~SQLiteHelper()
        {
            conn.Close();
        }
    }
}
