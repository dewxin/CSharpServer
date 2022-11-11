using CommonRpc.RpcBase;
using Protocol.Param;
using Protocol.Service.LoginService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.LoginService
{
    [RpcService(21000,22000)]
    public interface IWorld2Login:I_LoginService
    {
        GetPlayerInfoRet GetPlayerInfo(string token);

    }
}
