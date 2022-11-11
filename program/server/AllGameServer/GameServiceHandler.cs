using Autofac;
using CommonRpc;
using CommonRpc.Net;
using CommonRpc.Rpc;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase
{
    public class GameServiceHandler : MessageServiceHandler
    {
        public override SessionHolder SessionHolder => base.SessionHolder as SessionHolder;
        public GameServer Server { get; private set; }

        public GameServiceHandler() : base(RavenSingleton.Get<IContainer>().Resolve<ServerBase>())
        {
            Server = (base.host as GameServer) !;
        }
    }
}
