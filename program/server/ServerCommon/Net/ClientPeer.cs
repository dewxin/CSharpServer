using CommonRpc.Net;
using Newtonsoft.Json;
using ProjectCommon.Unit;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Net
{
    public abstract class ClientPeer : PeerServerEnd 
    {

        readonly ClientPeerManager connectManager;


        public ClientPeer()
        {
            connectManager = Server.ClientPeerManager;
            PeerID = Server.PeerIdGenerator.GetIdUShort();
        }


        public override void OnPeerStarted()
        {
            connectManager.AddPeer(this);
        }

        public override void OnPeerClosed()
        {
            connectManager.RemovePeer(this);
        }

        public abstract PeerBase GetLogicPeer();

        public abstract PeerBase GetWorldPeer();

    }
}
