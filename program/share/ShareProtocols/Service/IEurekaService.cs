using CommonRpc.RpcBase;
using MessagePack;
using Protocol.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service
{
    [RpcService(500,1000)]
    public interface IEurekaService
    {
        RegisterServerRet RegisterServerAndSubscribe(ServerRegisterData serverInfo);

    }
}
