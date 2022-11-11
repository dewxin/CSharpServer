using CommonRpc.RpcBase;
using Protocol.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.GateService
{
    [RpcService(5000, 10000)]
    public interface IClient2Gate: I_GateService
    {
        PeerIDResult GetPeerId();
    }
}
