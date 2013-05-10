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
        public void getRDBFile(int db)
        {
            int old_db = Redis.Db;
            using (var trans = Redis.CreateTransaction())
            {
                trans.QueueCommand(r => r.Db = db);
                trans.QueueCommand(r => r.SaveAsync());
                trans.QueueCommand(r => r.Db = old_db);
                trans.Commit();
            }
        }
        public void updateTaskList(string aTaskJson, string TaskId)
        {
            int db = Redis.Db;

            removeFromList(Constants.selfTaskList, TaskId);
            removeFromList(Constants.allTaskList, TaskId);
            using (var trans = Redis.CreateTransaction())
            {
                trans.QueueCommand(r => r.Db = 0);
                trans.QueueCommand(r => Redis.LPush(Constants.selfTaskList, RedisExt.GetBytes(aTaskJson)));
                trans.QueueCommand(r => Redis.LPush(Constants.allTaskList, RedisExt.GetBytes(aTaskJson)));
                trans.QueueCommand(r => r.Db = db);
                trans.Commit();
            }
        }
        public void removeFromList(string ListName,string TaskId)
        {
            List<string> selfTaskList = RedisExt.ToStringList(Redis.LRange(ListName, 0, -1));
            selfTaskList.ForEach(s =>
            {
                if (s.Contains(TaskId))
                {
                    Redis.LRem(ListName, 0, RedisExt.GetBytes(s));
                }
            });
        }
        public void push2selfTaskList(string aTaskJson)
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
        public byte[][] hgetall(int db, string hashId)
        {
            int old_db = Redis.Db;
            byte[][] tmp = Redis.HGetAll(hashId);
            Redis.Db = old_db;
            return tmp;
        }
        public string hget(int db, string hashId,string key)
        {
            int old_db = Redis.Db;
            byte[] keybytes = RedisExt.GetBytes(key);
            string tmp = RedisExt.GetString(Redis.HGet(hashId, keybytes));
            Redis.Db = old_db;
            return tmp;
        }
        public List<string> hvals(int db, string hashId)
        {
            List<string> values = new List<string>();
            int old_db = Redis.Db;
            byte[][] vals = Redis.HVals(hashId);

            Redis.Db = old_db;
            return RedisExt.ToStringList(vals);
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
        public List<string> getAllTasksList()
        {
            return Redis.ExecLuaAsList("return redis.call('LRANGE', '" + Constants.allTaskList + "', 0, -1)");
        }

    }
}
