using CommonRpc.Net;
using GameServerBase.ServerEureka;
using Protocol.Service;
using ServerCommon;
using SuperSocketSlim.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase
{
    public class GamePeer : ServerPeer
    {
        protected override GameServer Server => base.Server as GameServer;
        public GamePeer()
        {
        }

        public override void OnPeerStarted()
        {
            base.OnPeerStarted();

        }





    }
}
