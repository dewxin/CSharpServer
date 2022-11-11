using CommonRpc.RpcBase;
using Protocol.Param;
using Protocol.Service.WorldService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.WorldService
{
    [RpcService(31000, 32000)]
    public interface IGate2World: I_WorldService
    {
        void PlayerDisconnect(PacketPlayerDisconnect request);
    }
}
