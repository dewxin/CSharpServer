global using Protocol.Service.LogicService;
global using Protocol.Service.WorldService;
global using Protocol.Service.GateService;
global using Protocol.Service.LoginService;
global using Protocol.Service;
using Autofac;
using CommonRpc;
using CommonRpc.Net;
using CommonRpc.Rpc;
using GameServerBase.ServerEureka;
using Protocol;
using Protocol.Param;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SuperSocketSlim;
using Newtonsoft.Json;

namespace GameServerBase
{
    public abstract class GameServer<TPeer,IService> : GameServer 
        where TPeer : GamePeer 
        where IService: class
    {
        public GameServer()
        {
            PeerType = typeof(TPeer).Name;
            ServiceInterfaceType = typeof(IService);
        }

    }

    public abstract class GameServer : ServerBase 
    {
        public string PeerType { get; set; }
        public bool NeedRegisterOnEureka { get; set; } = true;

        private Dictionary<ushort, ServerInfo> serverID2PeerDict = new();

        protected Type ServiceInterfaceType { get; set; }

        public ServerWorld.WorldPeer? GetFirstWorldPeer()
        {
            return ServerPeerManager.GetFirstPeerByType<ServerWorld.WorldPeer>();
        }

        public ServerLogin.LoginPeer? GetFirstLoginPeer()
        {
            return ServerPeerManager.GetFirstPeerByType<ServerLogin.LoginPeer>();
        }


        protected override void RegisterContainterObjects_InvokeBaseFirst(ContainerBuilder builder)
        {
            builder.RegisterInstance(this).As<ServerBase>().As<IHost>().As<GameServer>().SingleInstance();


            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(IsPeerType);


            builder.RegisterType<ServiceHandlerList>().SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).As<MessageServiceHandler>().Where(MatchServerService).SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).As<ServerComponentBase>().SingleInstance();
        }

        public bool IsPeerType(Type type)
        {
            if(type.IsAssignableTo(typeof(PeerBase)))
            {
                PeerBase.RegisterPeerType(type.Name, type);
                return true;
            }
            return false;
        }

        public bool MatchServerService(Type type)
        {
            if (type.IsAssignableTo(ServiceInterfaceType))
                return true;
            if (type.IsAssignableTo(typeof(IServerCommonService)))
                return true;
            return false;
        }


        protected override void OnStarted()
        {
            base.OnStarted();

			if(NeedRegisterOnEureka)
                ConnectEurekaServer();
        }


        private void ConnectEurekaServer()
        {
            var address = IPAddress.Parse(CommonConfig.EurekaServer.IP);
            IPEndPoint ipEnd = new IPEndPoint(address, CommonConfig.EurekaServer.Port);
            ConnectServer(ipEnd, new ConnectParam { PeerType = nameof(EurekaPeer), CallbackOnStarted = RegisterServerOnEureka });
        }

        public void RegisterServerOnEureka(IAppSession session)
        {
            var sessionHolder = session as SessionHolder;
            var endPoint = SocketServer.EndPoint;
            sessionHolder.GetServiceProxy<IEurekaService>().RegisterServerAndSubscribe(new()
            {
                PeerType = PeerType,
                ServerIP = endPoint.Address.ToString(),
                ServerPort = endPoint.Port
            })
            .TCallback = (packet) =>
            {
                ServerInfo = packet.ServerInfo;
                OnReceiveServerInfoAddedNotice(packet.ServerInfoList);
                Logger.Debug($"{nameof(RegisterServerOnEureka)} callback  {JsonConvert.SerializeObject(packet)}");
            };
        }

        public void OnReceiveServerInfoRemovedNotice(List<ServerInfo> serverInfoList)
        {
            foreach(var serverInfo in serverInfoList)
            {
                serverID2PeerDict.Remove(serverInfo.ID);
            }
        }

        public void OnReceiveServerInfoAddedNotice(List<ServerInfo> serverInfoList)
        {
            foreach (var serverInfo in serverInfoList)
            {
                serverID2PeerDict.Add(serverInfo.ID, serverInfo);
            }

            foreach (var serverInfo in serverInfoList)
            {
				//如果是自己也不要连接
                if (ServerPeerManager.HasPeer(serverInfo.ID))
                    continue;

                var address = IPAddress.Parse(serverInfo.IP);
                IPEndPoint ipEnd = new IPEndPoint(address, serverInfo.Port);
                ConnectServer(ipEnd, new ConnectParam { PeerID= serverInfo.ID, PeerType = serverInfo.PeerType, CallbackOnStarted = NotifyServerInfoOnConnecting });
            }
        }

        public void NotifyServerInfoOnConnecting(IAppSession session)
        {
            var sessionHolder = session as SessionHolder;
            sessionHolder.GetServiceProxy<IServerCommonService>().ConnectServer(new() { PeerType = ServerInfo.PeerType, ServerId = ServerInfo.ID });

        }



    }
}
