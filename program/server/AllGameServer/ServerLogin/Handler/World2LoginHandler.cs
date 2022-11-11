using CommonRpc.Rpc;
using Newtonsoft.Json;
using ProjectCommon.MySql;
using ProjectCommon.Unit;
using Protocol;
using Protocol.Param;
using Protocol.Service.LoginService;
using ServerCommon;
using ServerCommon.Net;
using SqlDataCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogin
{
    public class World2LoginHandler : GameServiceHandler, IWorld2Login
    {
        public MySqlBase mySql => Server.MySqlManager.GetSql;
        public ClientPeerManager ClientPeerManager => Server.ClientPeerManager;
        public World2LoginHandler()
        {
            this.Server = base.Server as LoginServer;
        }

        public new LoginServer Server { get; private set; }

        public ClientOnLoginPeer Peer => base.SessionHolder.Peer as ClientOnLoginPeer;

        public GetPlayerInfoRet GetPlayerInfo(string token)
        {
            var playerInfo = Server.TokenManager.GetPlayerInfo(token);
            if(playerInfo == null)
            {
                return new GetPlayerInfoRet();
            }

            return new GetPlayerInfoRet
            {
                playerInfo = new PlayerInfo()
                {
                    Gold = playerInfo.Gold,
                    HeadIcon = playerInfo.HeadIcon,
                    NickName = playerInfo.NickName,
                    PlayerId = playerInfo.PlayerID,
                }
            };
        }
    }
}
