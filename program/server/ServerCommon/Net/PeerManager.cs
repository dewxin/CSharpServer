using CommonRpc.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class PeerManager<TPeer> : ServerComponentBase 
        where TPeer : class, IPeer
    {
        //TODO 应该需要是单线程访问的
        protected ConcurrentDictionary<ushort, TPeer> peerID2PeerDict { get; set; } = new();

        public PeerManager(ServerBase serverBase): base(serverBase)
        {
        }

        public virtual bool AddPeer(TPeer peer)
        {
            if(peerID2PeerDict.ContainsKey(peer.PeerID))
            {
                peer.Disconnect();
                return false;
            }
            //todo.now 会block
            return peerID2PeerDict.TryAdd(peer.PeerID, peer);
        }

        public virtual void RemovePeer(TPeer peer)
        {
            TPeer val;
            peerID2PeerDict.TryRemove(peer.PeerID, out val);
        }


        public List<TPeer> GetAllSessions()
        {
            return peerID2PeerDict.Values.ToList();
        }


        public bool HasPeer(ushort peerId)
        {
            return peerID2PeerDict.ContainsKey(peerId);
        }

        public TPeer GetPeer(ushort peerId)
        {
            if (peerID2PeerDict.ContainsKey(peerId))
                return peerID2PeerDict[peerId];

            return null;
        }



        //Tick 发生在Supersocket的会话里
        public override void Tick(double elapsed)
        {
        }

    }
}
