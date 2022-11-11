using CommonRpc.Rpc;
using Protocol.Param;
using Protocol.Service.GateService;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerGate.Handlers
{
    public class Client2GateHandler : GameServiceHandler, IClient2Gate
    {

        public Client2GateHandler()
        {
        }


        public PeerIDResult GetPeerId()
        {
            return new() { PeerID = Server.CurrentPeer.PeerID };
        }
    }
}
