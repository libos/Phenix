using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace Phenix
{
    class DataMapping
    {
        RedisClient Redis = new RedisClient("localhost", 6380);
        public DataMapping()
        {
            Redis.Db = 1;
        }
        public DataMapping(int db)
        {
            if (db == 0)
            {
                throw new Exception("数据库编号必须>=1");
            }
            Redis.Db = db;
        }

    }
}
