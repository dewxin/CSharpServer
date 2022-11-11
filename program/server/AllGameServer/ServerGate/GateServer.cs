using Autofac;
using CommonRpc;
using GameServerBase;
using Protocol;
using Protocol.Service;
using Protocol.Service.GateService;
using ServerCommon;
using ServerCommon.Timer;
using SqlDataCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerGate
{
    public class GateServer : GameServer<GatePeer, I_GateService>
    {
        public GateServer()
        {
            SupportForwardMsg = true;
        }


        protected override void OnStarted()
        {
            //todo eliminate calling base function
            base.OnStarted();

            TimerManager.CreateTimerServerThread(ServerCheckTime, null, TryConnectWorldAndLogicServer);
        }

        void TryConnectWorldAndLogicServer(object? timer)
        {

            //if (MySqlManager == null)
            //    return;

            ////todo 服务治理 移除magic number
            ////todo 会有boxing的开销
            //var list = MySqlManager.GetSql.FindList<SqlServerInfo>($"{nameof(SqlServerInfo.ServerState)}", (short)1);//todo magic number


            //bool serverAllConnected = true;
            //foreach (var svrInfo in list)
            //{
            //    bool svrTypeValid = svrInfo.ServerType == typeof(ServerWorld.WorldServer).Name || svrInfo.ServerType == typeof(ServerLogic.LogicServer).Name;
            //    if (svrTypeValid && !ServerPeerManager.HasPeerByServerId(svrInfo.ServerID))
            //    {
            //        Logger.InfoFormat("connect server sId:{0} sType:{1} name:{2}", svrInfo.ServerID, svrInfo.ServerType, svrInfo.ServerName);
            //        IPEndPoint ipend = new IPEndPoint(IPAddress.Parse(svrInfo.ServerIP), svrInfo.ServerPort);
            //        ConnectServer(ipend);
            //        serverAllConnected = false;
            //    }
            //}
            //if (serverAllConnected)
            //    (timer as MyTimer).Stop();
        }



    }
}
