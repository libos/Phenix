using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace Phenix
{
    class QueueModule
    {
        RedisClient Redis = new RedisClient("localhost", 6380);

        public QueueModule()
        {
            Redis.Db = 0;
        }
        public void inQueue(string cmd)
        {
            Redis.LPush("msgQueue1",RedisExt.GetBytes(cmd));
        }
    }
}
