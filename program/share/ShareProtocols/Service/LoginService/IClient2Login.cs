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
    //TODO 应该按Service的目录 给每个Service自动生成对应的RpcServiceId
    [RpcService(20000,21000)]
    public interface IClient2Login:I_LoginService
    {
        RegisterResult RegisterAccount(AccountData accountData);

        LoginResult PlayerLogin(AccountData logicLoginData);

    }
}
