global using CommonRpc.RpcBase;
using Newtonsoft.Json;
using Protocol;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonRpc.Rpc;
using Protocol.Service;
using GameServerBase.ServerWorld.Entities;
using Protocol.Param;
using Protocol.Service.WorldService;
using Protocol.Service.LoginService;

namespace GameServerBase.ServerWorld
{
    public class Client2WorldHandler : GameServiceHandler, IClient2World
    {
        public Client2WorldHandler()
        {
        }

        public GetPlayerInfoRet Login(WorldLoginData worldLoginData)
        {
            //TODO add worldPlayer to manager
            var ret = new GetPlayerInfoRet();

            var requetRet =Server.GetFirstLoginPeer()!.GetServiceProxy<IWorld2Login>().GetPlayerInfo(worldLoginData.Token);
            requetRet.TCallback = (response) =>
            {
                ret.playerInfo = response.playerInfo;
            };

            (ret as IWaitPremise).Wait(requetRet);
            return ret;

        }

    }
}
