using ServerCommon;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonRpc.RpcBase;
using CommonRpc.Rpc;
using Protocol.Service;
using CommonRpc.Net;
using GameServerBase.ServerLogic.Logic;
using Protocol.Param;
using Protocol.Service.LogicService;

namespace GameServerBase.ServerLogic.Handlers
{

    public class Gate2LogicHandler : GameServiceHandler, IGate2Logic
    {
        private readonly LogicPlayerManager playerManager;

        public Gate2LogicHandler()
        {
            playerManager = Server.GetManager<LogicPlayerManager>();
        }


        public void PlayerDisconnect(PacketPlayerDisconnect request)
        {
            var player = playerManager.GetPlayerBySessionId(request.GlobalSessionID);
            if (player != null)
            {
                player.Logout();
            }
        }
    }
}
