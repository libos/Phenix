using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;
using System.Data;
namespace Phenix.Core.Database
{
    public class MySQLHelper : IDatabaseHelper
    {
        MySqlConnection conn;
        public MySQLHelper(string database)
        {
            conn = new MySqlConnection("server=localhost;user=root;database=" + database 
                + ";port=3306;");
            conn.Open();
        }
        public MySQLHelper(string server, string port, string username, string password,string database)
        {
            conn = new MySqlConnection("server=" + server + ";user=" + username + ";database="+database + ";port=" + port + ";password=" + password + ";");
            conn.Open();
        }
        //jknlff8-pro-17m7755
        public object execute(string sql)
        {
            MySqlCommand msc = new MySqlCommand(sql, conn);
            try
            {
                return msc.ExecuteReader();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public DataSet getDataSet(string sql)
        {
            MySqlCommand msc = new MySqlCommand(sql, conn);
            var adapter = new MySqlDataAdapter(msc);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="primaryKey"></param>
        /// <param name="ValueList"></param>
        public void saveProto(string tablename,string primaryKey,params string[] ValueList)
        {
            FileStream fsFile = new FileStream(@"tmp.rdb", FileMode.Append);
            StreamWriter sw = new StreamWriter(fsFile);
            for (int i = 0; i < ValueList.Length; i++)
            {
                string sql = @"SELECT CONCAT('*4\r\n',  '$', LENGTH(redis_cmd), '\r\n',  
                                redis_cmd, '\r\n',  '$', LENGTH(redis_key), '\r\n', 
                                redis_key, '\r\n',  '$', LENGTH(hkey), '\r\n',  
                                hkey, '\r\n',  '$', LENGTH(hval), '\r\n', 
                                hval, '\r\n')
                                FROM (
                                SELECT  'HSET' as redis_cmd,  
                                '" + tablename + i.ToString() + @"' AS redis_key,  
                                " + primaryKey + @" AS hkey,  
                                 " + ValueList[i] + @" AS hval  
                                FROM " + tablename + @"
                                ) AS t";
                MySqlDataReader mdr = (MySqlDataReader)execute(sql);
                while (mdr!=null && mdr.Read())
                {
                    sw.Write(mdr[0]);
                    Console.Write(mdr[0]);
                }
                mdr.Close();
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
        ~MySQLHelper()
        {
            conn.Close();
        }
    }
}
