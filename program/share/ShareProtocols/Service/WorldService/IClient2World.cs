using CommonRpc.RpcBase;
using Protocol.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.WorldService
{
    [RpcService(30000,31000)]
    [RpcForward(ForwardTarget.Client, ForwardTarget.World)]
    public interface IClient2World: I_WorldService
    {
        GetPlayerInfoRet Login(WorldLoginData worldLoginData);
    }
}
