using Autofac;
using GameServerBase;
using Protocol;
using Protocol.Service.LoginService;
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

namespace GameServerBase.ServerLogin
{
    public class LoginServer : GameServer<LoginPeer, I_LoginService>
    {
        public LoginServer()
        {
        }

        public TokenManager TokenManager => this.GetManager<TokenManager>();


        protected override void OnStarted()
        {
            base.OnStarted();

            TimerManager.CreateTimerServerThread(ServerCheckTime, null, TryConnectWorldServers);
        }

        void TryConnectWorldServers(object? timer)
        {
            //base.TryConnectBy<ServerWorld.WorldServer>(out bool allConnected);
            //if (allConnected)
            //    (timer as MyTimer)!.Stop(); // null-forgiving operator
        }

        protected override void Tick(double elapsed)
        {
        }

        //protected override PeerStub CreatePassiveOpenPeer()
        //{
        //    var peer = AutofacContainer.Resolve<ClientPeerOnLogin>();
        //    return new PeerStub(peer, AutofacContainer);
        //}

    }
}
