using CommonRpc.Net;
using CommonRpc.Rpc;
using Protocol;
using Protocol.Param;
using Protocol.Service.LogicService;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogic.Handlers
{
    public class Client2LogicHandler : GameServiceHandler, IClient2Logic
    {
        public Client2LogicHandler()
        {
        }

        public GetPlayerInfoRet GetPlayerInfo()
        {
            return new GetPlayerInfoRet { playerInfo = new PlayerInfo {PlayerId =1, Gold=1, NickName="goodBoy"} };
        }
    }
}
