using Autofac;
using GameServerBase;
using GameServerBase.ServerLogic.Logic;
using Protocol.Service.LogicService;
using ServerCommon;
using ServerCommon.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogic
{
    public class LogicServer : GameServer<LogicPeer, I_LogicService>
    {
        public LogicPlayerManager PlayerManager => GetManager<LogicPlayerManager>();
        public LogicServer()
        {
        }


        protected override void OnStarted()
        {
            base.OnStarted();

            //TimerManager.CreateTimerServerThread(ServerCheckTime, null, TryConnectWorldServers);
        }

        //void TryConnectWorldServers(object? timer)
        //{
        //    base.TryConnectBy<ServerWorld.WorldServer>(out bool allConnected);
        //    if (allConnected)
        //        (timer as MyTimer)!.Stop(); // null-forgiving operator
        //}

        protected override void Tick(double elapsed)
        {

        }

    }
}
