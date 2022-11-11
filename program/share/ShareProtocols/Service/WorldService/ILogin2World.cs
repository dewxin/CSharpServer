using CommonRpc.RpcBase;
using Protocol.Service.WorldService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service.WorldService
{
    [RpcService(33000, 34000)]
    public interface ILogin2World: I_WorldService
    {
    }
}
