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
    public class Client2LoginHandler : GameServiceHandler, IClient2Login
    {
        public MySqlBase mySql => Server.MySqlManager.GetSql;
        public ClientPeerManager ClientPeerManager => Server.ClientPeerManager;
        public Client2LoginHandler()
        {
            Server = base.Server as LoginServer;
        }

        public new LoginServer Server { get; private set; }


        public LoginResult PlayerLogin(AccountData loginData)
        {
            var playerInfo = SelectPlayerInfoByAccount(loginData.Account);
            if (playerInfo == null)
            {
                return new LoginResult { Result = LoginResultEnum.AccountNotExist };
            }

            var gateInfo = GetGateServerInfo();
            if (gateInfo == null)
            {
                return new LoginResult { Result = LoginResultEnum.ServerNotAvail };
            }

            var token = Server.TokenManager.GetToken(playerInfo);

            return new LoginResult
            {
                Result = LoginResultEnum.Succeed,
                GateServerIP = gateInfo.ServerIP,
                GateServerPort = gateInfo.ServerPort,
                Token = token
            };
        }



        public RegisterResult RegisterAccount(AccountData accountData)
        {
            var playerInfo = SelectPlayerInfoByAccount(accountData.Account);

            if (playerInfo!=null)
            {
                return new RegisterResult { Result = RegisterResultEnum.AccountAlreadyRegistered };
            }

            playerInfo = new SqlPlayerInfo()
            {
                Account = accountData.Account,
                Password = accountData.Password,
            };

            var ret = mySql.Insert(playerInfo);
            if (ret == -1)
            {
                return new RegisterResult { Result = RegisterResultEnum.FailDueToDbError };
            }

            return new RegisterResult { Result = RegisterResultEnum.Succeed };

        }

        #region DB

        public SqlPlayerInfo SelectPlayerInfoByAccount(string account)
        {
            string findhql = ($"from {nameof(SqlPlayerInfo)} where {nameof(SqlPlayerInfo.Account)} = '{account}' and {nameof(SqlPlayerInfo.IsInvalid)} = false;");
            var playerInfo = mySql.FindOneHQL<SqlPlayerInfo>(findhql);
            return playerInfo;
        }

        public SqlServerInfo? GetGateServerInfo()
        {
            var list = Server.MySqlManager.GetSql.FindList<SqlServerInfo>($"{nameof(SqlServerInfo.ServerState)}", (short)1);//todo magic number

            foreach (var serverInfo in list)
            {
                bool isGate = serverInfo.ServerType == typeof(ServerGate.GateServer).Name;
                if(isGate)
                    return serverInfo;
            }
            return null;
        }

        #endregion
    }
}
