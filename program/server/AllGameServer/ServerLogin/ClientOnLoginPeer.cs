using Protocol;
using ProjectCommon.Unit;
using ServerCommon;
using ServerCommon.Net;
using ServerCommon.Timer;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonRpc.Net;
using SqlDataCommon;

namespace GameServerBase.ServerLogin
{
    public class ClientOnLoginPeer : ClientPeer
    {

        protected override LoginServer Server => base.Server as LoginServer;


        public ClientOnLoginPeer()
        {
        }

        //public override void OnSessionStarted()
        //{
        //    base.OnSessionStarted();
        //}

        //public override void OnSessionClosed()
        //{
        //    base.OnSessionClosed();
        //}

        public override PeerBase GetLogicPeer()
        {
            throw new NotImplementedException();
        }

        public override PeerBase GetWorldPeer()
        {
            throw new NotImplementedException();
        }
    }
}
