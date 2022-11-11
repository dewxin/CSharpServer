using CommonRpc.Net;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerEureka
{
    public class OtherOnEurekaPeer : GamePeer
    {
        protected override EurekaServer Server => base.Server as EurekaServer;
        public OtherOnEurekaPeer()
        {
        }


        public override void OnPeerClosed()
        {
            base.OnPeerClosed();

            Server.ServerInfoManager.ServerDisconnect(PeerID);
            //if (_heartbeatTimer != null)
            //{
            //    _heartbeatTimer.Stop();
            //    _heartbeatTimer = null;
            //}
        }

    }
}
