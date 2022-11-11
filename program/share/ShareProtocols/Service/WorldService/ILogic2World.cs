using CommonRpc.RpcBase;
using Protocol.Service.WorldService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.WorldService
{
    [RpcService(32000, 33000)]
    public interface ILogic2World : I_WorldService
    {
        //WorldLoginRet PlayerLogin(WorldLoginData worldLoginData);

    }
}
