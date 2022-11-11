using CommonRpc.Rpc;
using Protocol.Param;
using Protocol.Service;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerEureka.Handlers
{
    public class Server2EurekaHandler : GameServiceHandler, IEurekaService
    {
        private EurekaServer Server;
        private ServerInfoManager ServerInfoManager => Server.ServerInfoManager;

        public Server2EurekaHandler(ServerBase server)
        {
            this.Server = server as EurekaServer;
        }

        public RegisterServerRet RegisterServerAndSubscribe(ServerRegisterData registerData)
        {
            var serverInfo = ServerInfoManager.RegisterServerAndSubscribe(registerData);
            return serverInfo;
        }
    }
}
