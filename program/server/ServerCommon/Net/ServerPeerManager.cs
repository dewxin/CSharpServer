using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class ServerPeerManager : PeerManager<ServerPeer>
    {
        public ServerPeerManager(ServerBase server) : base(server)
        {
        }


        public List<ServerPeer> GetPeersByType<PeerType>() where PeerType : ServerPeer
        {
            var type = typeof(PeerType);
            List<ServerPeer> list = new();

            foreach (var it in peerID2PeerDict)
            {
                if (it.Value.GetType().Name.Equals(type.Name))
                {
                    list.Add(it.Value);
                }
            }

            return list;
        }

        public PeerType? GetFirstPeerByType<PeerType>() where PeerType : ServerPeer
        {
            var list = GetPeersByType<PeerType>();
            if (list.Count == 0)
                return null;
            return list[0] as PeerType;
        }

    }
}
