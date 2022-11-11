using Autofac;
using Newtonsoft.Json;
using ProjectCommon.Unit;
using Protocol.Service;
using ServerCommon;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerEureka
{
    public class EurekaServer : GameServer<EurekaPeer, IEurekaService>
    {
        public ServerInfoManager ServerInfoManager { get; set; }

        public EurekaServer()
        {
            NeedRegisterOnEureka = false;
            ServerInfoManager = new ServerInfoManager(this);
        }


        protected override SessionHolder CreateAppSession(SocketSession socketSession)
        {
            var peerStub = new SessionHolder();
            peerStub.UpdatePeer(ServerInfoManager.GetServerID(),nameof(OtherOnEurekaPeer),ServerPeerManager);
            return peerStub;
        }


    }
}
