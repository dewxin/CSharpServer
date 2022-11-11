using CommonRpc.Rpc;
using GameServerBase.ServerWorld.Entities;
using Protocol;
using Protocol.Param;
using Protocol.Service;
using Protocol.Service.WorldService;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerWorld
{
    public class Gate2WorldHandler : GameServiceHandler, IGate2World
    {

        public Gate2WorldHandler()
        {
            playerManager = Server.GetManager<WorldPlayerManager>();
        }

        private readonly WorldPlayerManager playerManager;

        public void PlayerDisconnect(PacketPlayerDisconnect request)
        {
            var player = playerManager.GetPlayerBySessionId(request.GlobalSessionID);
            player.Logout();
#if DEBUG_LOG
            Server.Logger.Debug($"Player disconnect {request.GlobalSessionID} ");
#endif
        }

    }
}
