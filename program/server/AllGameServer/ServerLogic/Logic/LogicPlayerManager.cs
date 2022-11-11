using Autofac;
using Protocol;
using ProjectCommon.MySql;
using ServerCommon;
using ServerCommon.Sql;
using ServerCommon.Unit;
using SqlDataCommon;
using SuperSocketSlim.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogic.Logic
{
    public class LogicPlayerManager : PlayerMgrBase<LogicPlayer>
    {
        public MySqlBase mySql => server.MySqlManager.GetSql;

        public IContainer AutofacContainer { get { return server.AutofacContainer; } }
        public LogicPlayerManager(ServerBase server) : base(server)
        {
        }


   
    }
}
