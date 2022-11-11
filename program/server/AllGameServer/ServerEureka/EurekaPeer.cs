using Autofac;
using CommonRpc.Net;
using Newtonsoft.Json;
using Protocol.Service;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerEureka
{
    public class EurekaPeer : GamePeer
    {
        public EurekaPeer()
        {
        }



#if DEBUG_LOG
            Server.Logger.Debug($"{nameof(RegisterServerOnEureka)} called");
#endif
    }

}

