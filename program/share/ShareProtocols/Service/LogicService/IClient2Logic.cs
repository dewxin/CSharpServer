using CommonRpc.RpcBase;
using Protocol.Param;
using Protocol.Service.LogicService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.LogicService
{
    [RpcService(10000,11000)]
    [RpcForward(ForwardTarget.Client, ForwardTarget.Logic)]
    public interface IClient2Logic: I_LogicService
    {
        GetPlayerInfoRet GetPlayerInfo();
    }
}
