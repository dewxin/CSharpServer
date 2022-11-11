using Google.Protobuf;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;


namespace ProjectCommon.Redis
{
    //RedisBase是线程安全的封装类，支持多线程同时使用
    public class RedisBase
    {

        private ConnectionMultiplexer redis;

        public RedisBase(string connectStr)
        {
            Init(connectStr);
        }

        public void Init(string connectStr)
        {
            if(redis==null)
            {
                redis = ConnectionMultiplexer.Connect(connectStr);
            }
        }

        public IDatabase GetDb(int dbIndex = -1, object asyncState = null)
        {
            return redis.GetDatabase(dbIndex, asyncState);
        }

        public T GetObject<T>(int id, int dbIndex=-1, string entry="DataConfig", string serverName="dev") where T : pb::IMessage<T>, new()
        {
            //typeof(T).Name 问题不大
            string redisKey = string.Join(':', serverName, entry, typeof(T).Name, id);
            var db = GetDb(dbIndex);
            var json = db.StringGet(redisKey);

            var ret = JsonParser.Default.Parse<T>(json);
            return ret;
        }


    }
}
