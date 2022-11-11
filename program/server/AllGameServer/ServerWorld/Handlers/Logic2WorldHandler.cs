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
using Protocol.Service.WorldService;

namespace GameServerBase.ServerWorld
{
    public class Logic2WorldHandler : GameServiceHandler, ILogic2World
    {

        public Logic2WorldHandler()
        {
            playerManager = Server.GetManager<WorldPlayerManager>();
        }

        private readonly WorldPlayerManager playerManager;

        //public WorldLoginRet PlayerLogin(WorldLoginData packet)
        //{
        //    Server.Logger.DebugFormat("LWOnPlayerLoginHandler begin PacketOnPlayerLogin {0}", JsonConvert.SerializeObject(packet));


        //    var resultCode = EnumResultType.Fail;
        //    if (ClientPeer.PeerServerId > 0 && playerManager.OnPlayerLogin(packet, ClientPeer.PeerServerId))
        //        resultCode = EnumResultType.Success;


        //    WorldLoginRet retMsg = new WorldLoginRet()
        //    {
        //        PlayerID = packet.Info.PlayerId,
        //        GlobalSessionID = packet.GlobalSessionID,
        //        Result = resultCode,
        //    };

        //    Server.Logger.DebugFormat("LWOnPlayerLoginHandler end PacketOnPlayerLoginResult {0}", JsonConvert.SerializeObject(retMsg));

        //    return retMsg;
        //}


    }
}
