using CommonRpc.Net;
using CommonRpc.Rpc;
using Protocol;
using Protocol.Param;
using Protocol.Service.GateService;
using Protocol.Service.LoginService;
using Protocol.Service.WorldService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ClientNet.Unit
{
    public class MyLogger : ILog
    {
        public Action<string> DebugDelegate = delegate{};
        public Action<string> ErrorDelegate = delegate{};

        public void Debug(string message)
        {
            DebugDelegate(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public void Error(string message)
        {
            ErrorDelegate(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }
    }

    public class NetClient : IHost
    {
        public Random GRand = new Random((int)DateTime.Now.Ticks);
        public MyLogger Logger = new MyLogger ();
        public ILog MyLog => Logger;

        public NetPeer GatePeer { get; private set; }
        public NetPeer LoginPeer { get; private set; }
        public PlayerInfo PlayerInfo { get; private set; } = new PlayerInfo();
        public ClientInfo ClientInfo { get; private set; } = new ClientInfo();

        public ushort CurrentClientPeerId { get; set; }

        public bool IsClient => true;

        public PeerBase CurrentPeer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public NetClient()
        {
        }

        public static NetClient New()
        {
            var netClient = new NetClient();
            netClient.CreateGatePeer();
            return netClient;
        }


        private void CreateGatePeer()
        {
            var handlerList = new List<MessageServiceHandler>();
            ServiceHandlerList ServiceHandlerList = new ServiceHandlerList(handlerList, this);

            LoginPeer = new NetPeer(ServiceHandlerList, this);
            GatePeer = new NetPeer(ServiceHandlerList, this);
        }

        public void Tick(double elapsed)
        {
            LoginPeer.Tick(elapsed);
            GatePeer.Tick(elapsed);
        }


        #region net

        public void Exit()
        {
            MyLog.Debug($"{ClientInfo.Account} exit.");
            GatePeer.Disconnect();
        }

        #endregion

        #region server interface


        public void ConnectLocalServer()
        {
            LoginPeer.ConnectServer("127.0.0.1", 10000);
        }


        public void LoginServer(string account, string password)
        {
            ClientInfo.Account = account;
            ClientInfo.Password = password;

            AccountData msg = new AccountData();
            msg.Account = account;
            msg.Password = password;

            LoginPeer.GetServiceProxy<IClient2Login>().PlayerLogin(msg).TCallback = (response) =>
            {
                MyLog.Debug($"login result is {response.Result}");
                if(response.Result == LoginResultEnum.Succeed)
                {
                    GatePeer.ConnectServer(response.GateServerIP, response.GateServerPort);
                    GatePeer.GetServiceProxy<IClient2Gate>().GetPeerId().TCallback = (result) =>
                    {
                        //TODO 感觉这个全局ID放到IHost里面更合适一点
                        GatePeer.PeerID = result.PeerID;

                        GatePeer.GetServiceProxy<IClient2World>().Login(new WorldLoginData() { Token = response.Token }).TCallback =
                        (playerInfoRet) =>
                        {
                            var playerInfo = playerInfoRet.playerInfo;
                            MyLog.Debug($"nickName {playerInfo.NickName} playerId {playerInfo.PlayerId}");
                        };

                    };


                }


                //LoginPeer.Disconnect(); TODO 取消注释
            };

            MyLog.Debug("after assign callback");
        }

        #endregion

    }

}
