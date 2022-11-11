using FluentNHibernate.Cfg.Db;
using ProjectCommon.MySql;
using SqlDataCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Sql
{
    public class SqlThreadData
    {
        public delegate bool SqlProcHandler(object packet, object peer);
        public delegate void SqlCallBackHandler(object packet, object peer, bool result);

        public SqlThreadData(object msg, object sp, SqlProcHandler proc, SqlCallBackHandler callback)
        {
            Packet = msg;
            Peer = sp;
            ProcHandler = proc;
            CallbackHandler = callback;
            IsSuccess = false;
        }

        public SqlProcHandler ProcHandler { get; private set; }
        public SqlCallBackHandler CallbackHandler { get; private set; }
        public object Packet { get; private set; }
        public object Peer { get; private set; }

        public bool IsSuccess { get; set; }
    }



    public class MySqlManager : ServerComponentBase
    {
        public MySqlBase GetSql { get; private set; }

        public MySqlManager(ServerBase server) : base(server)
        {
            GetSql = new MySqlBase();
            GetSql.OnError += server.Logger.Error;

            var mystr = server.CommonConfig.SQLConnectionList.Where(nameval => nameval.Name.Equals("MySql")).Single().Val;
            GetSql.InitMySql(mystr, typeof(SqlServerInfoMap).Assembly);
        }

        public override void Tick(double elapsed)
        {

        }

        //public override void Init()
        //{
        //}

        //public override void Release()
        //{
        //}
    }
}
