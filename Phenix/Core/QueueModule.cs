using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace Phenix.Core
{
    public class QueueModule
    {
        RedisClient Redis = new RedisClient("localhost", 6380);//, Constants.Passwd);
        
        public QueueModule()
        {
            Redis.Db = 0;
        }
        public void inQueue(string cmd)
        {
            int db = Redis.Db;
           
            using (var trans = Redis.CreateTransaction())
            {
                trans.QueueCommand(r => r.Db = 0);
                trans.QueueCommand(r =>  Redis.LPush("msgQueue1",RedisExt.GetBytes(cmd)));
                trans.QueueCommand(r => r.Db = db);
                trans.Commit();
            }
        }

        public  void push2selfTaskList(string aTaskJson)
        {
            int db = Redis.Db;

            using (var trans = Redis.CreateTransaction())
            {
                trans.QueueCommand(r => r.Db = 0);
                trans.QueueCommand(r => Redis.LPush(Constants.selfTaskList, RedisExt.GetBytes(aTaskJson)));
                trans.QueueCommand(r => Redis.LPush(Constants.allTaskList, RedisExt.GetBytes(aTaskJson)));
                trans.QueueCommand(r => r.Db = db);
                trans.Commit();
            }

        }
        public  void push2TaskList(string aTaskJson)
        {
            int db = Redis.Db;

            using (var trans = Redis.CreateTransaction())
            {
                trans.QueueCommand(r => r.Db = 0);
                trans.QueueCommand(r => Redis.LPush(Constants.allTaskList, RedisExt.GetBytes(aTaskJson)));
                trans.QueueCommand(r => r.Db = db);
                trans.Commit();
            }
           
        }
    }
}
