using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Phenix.Core.Database
{
    public class RedisHelper
    {

        public RedisHelper()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="no">第几组</param>
        /// <param name="unit"></param>
        /// <param name="db"></param>
        /// <param name="hashKey"></param>
        /// <param name="hashIds"></param>
        /// <returns></returns>
        public string getOneParam(int group, int unit, int db, params string[] hashIds)
        {
            string tmp = string.Empty;
            QueueModule qm = new QueueModule();
            foreach (string hashId in hashIds)          //hashId不一样，key都是主键
            {
                List<string> values = qm.hvals(db, hashId);
                string one = string.Empty;
                for (int i = 0; i < unit; i++)
                {
                    if (one == string.Empty)
                    {
                        one = values[group*unit + i];
                    }
                    else
                    {
                        one += ";" + values[group*unit + i];
                    }
                }
                if (tmp == string.Empty)
                {
                    tmp = one;
                }
                else
                {
                    tmp += " " + one;
                }
            }
            return tmp;
        }
        public List<string> getParamSet(int group,int unit,int db,params string[] hashIds)
        {
            List<string> tmp = new List<string>();
            for (int i = 0; i < group; i++)
			{
                string param = getOneParam(group, unit, db, hashIds);
                tmp.Add(param);
			}
            return tmp;
        }
        public bool mergeRDB(string source, string newfile, int database)
        {
            FileStream fsource = new FileStream(source, FileMode.OpenOrCreate);
            FileStream fnew = new FileStream(newfile, FileMode.OpenOrCreate);
            for (int i = 0; i < 9; i++)
            {
               fnew.ReadByte() ;
            }

            fnew.ReadByte();
            byte data = Convert.ToByte(Convert.ToInt16(database.ToString("X")));
            fnew.WriteByte(data);
            fnew.Seek(0, SeekOrigin.Begin);
            fsource.Seek(-9, SeekOrigin.End);


            int numBytesRead = 0; 
            int numBytesToRead = (int)fnew.Length;
            
            byte[] bytes = new byte[numBytesToRead];


            while (numBytesToRead > 0)
            {
                int n = fnew.Read(bytes, numBytesRead , numBytesToRead);
 
                if (n == 0)
                    break;
                numBytesRead += n;
                numBytesToRead -= n;
            }
           
            fsource.Write(bytes, 9 , bytes.Length - 9 );
         
            fsource.Close();
            fnew.Close();
            return true;
        }

    }
}
