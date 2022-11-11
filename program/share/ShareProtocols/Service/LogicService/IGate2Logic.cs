using CommonRpc.RpcBase;
using Protocol.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.LogicService
{
    [RpcService(11000, 12000)]
    public interface IGate2Logic: I_LogicService
    {
        void PlayerDisconnect(PacketPlayerDisconnect request);

    }
}
