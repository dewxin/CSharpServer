using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Net
{
    public class ClientPeerManager : PeerManager<ClientPeer>
    {
        public ClientPeerManager(ServerBase serverBase) 
            : base(serverBase)
        {
        }


        public override void RemovePeer(ClientPeer peer)
        {
            if (peer != null)
            {
                base.RemovePeer(peer);
                server.PeerIdGenerator.ReleaseId(peer.PeerID);
            }
        }

    }
}
