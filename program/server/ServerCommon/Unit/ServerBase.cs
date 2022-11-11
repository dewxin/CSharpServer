using Autofac;
using CommonRpc.Net;
using CommonRpc.Rpc;
using ProjectCommon.Unit;
using ServerCommon.Net;
using ServerCommon.Sql;
using ServerCommon.Timer;
using SqlDataCommon;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim;
using SuperSocketSlim.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommonRpc;
using Protocol.Service;
using Protocol.Param;

namespace ServerCommon
{
    public class MyLogger : ILog
    {
        ServerBase serverBase;
        public MyLogger(ServerBase server)
        {
            serverBase = server;
        }

        public void Debug(string message)
        {
            serverBase.Logger.Debug(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            serverBase.Logger.DebugFormat(format, args);
        }

        public void Error(string message)
        {
            serverBase.Logger.Error(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            serverBase.Logger.ErrorFormat(format, args);
        }
    }



    public abstract class ServerBase : AppServer<SessionHolder>, IHost
    {
        //interval
        public static int ServerCheckTime = 5000;

        protected abstract void RegisterContainterObjects_InvokeBaseFirst(ContainerBuilder builder);

        // 子模块管理
        public AfContainer ServerManagerContainer { get; private set; }
        public ClientPeerManager ClientPeerManager => GetManager<ClientPeerManager>();
        public ServerPeerManager ServerPeerManager => GetManager<ServerPeerManager>();
        public MySqlManager MySqlManager => GetManager<MySqlManager>();
        public TimerManager TimerManager => GetManager<TimerManager>();

        //线程池
        //public ServerThread ServerThread { get; }
        IEnumerable<ThreadBase> backgroundThreads { get; set; }

        public string ServerType { get; private set; }

        public ServerInfo ServerInfo { get; set; }
        public Random GRandom { get; private set; }
        public IdGenerator PeerIdGenerator { get; private set; } //todo 字段位置整理一下

        //todo 移除Autofac自动装配容器
        public IContainer AutofacContainer { get; private set; }

        private MyLogger InnerLogger { get; set; }
        public ILog MyLog => InnerLogger;

        public bool SupportForwardMsg { get; set; }

        public PeerBase CurrentPeer { get; set; }

        public bool IsClient => false;

        public ServerBase()
        {
            ServerType = GetType().Name;
            GRandom = new Random((int)DateTime.Now.Ticks);
            PeerIdGenerator = new IdGenerator();
            InnerLogger = new MyLogger(this);
        }


        protected override bool Setup(ServerConfig config)
        {
            Logger.Info(string.Format("{0} : {1} Setup ...", DateTime.Now.TimeOfDay, config.Name));

            Init();

            return true;
        }


        protected void Init()
        {

            //注册
            var builder = new ContainerBuilder();
            builder.RegisterType<AfContainer>().SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).As<ServerComponentBase>().SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).As<ThreadBase>().SingleInstance();

            //TODO 不要注入不是自己的服务，比如GateServer不要有LoginServer的Handler
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).As<MessageServiceHandler>().SingleInstance();


            RegisterContainterObjects_InvokeBaseFirst(builder);

            //生成
            var container = builder.Build();
            RavenSingleton.Set<IContainer>(container);
            AutofacContainer = container;
            ServerManagerContainer = container.Resolve<AfContainer>(new TypedParameter(typeof(IContainer), AutofacContainer));
            CheckTimeHelper.ErrorHandler += Logger.ErrorFormat;

            backgroundThreads = container.Resolve<IEnumerable<ThreadBase>>();


            var managerBase = container.Resolve<IEnumerable<ServerComponentBase>>();
            foreach (var it in managerBase)
            {
                Logger.DebugFormat("server manager add entity {0}", it.GetType().Name);
                ServerManagerContainer.AddComponent(it);
            }


            Logger.Info(string.Format("{0} : {1} inited ...", DateTime.Now.TimeOfDay, this.Name));
        }


        // todo.now.1 类结构再改改 一个类同时被多个线程使用，感觉风险有点大
        // todo.now.2 改成单进程多线程
        // Supersocket Thread
        protected override void OnStarted()
        {
            base.OnStarted();
            // 定时任务
            //TimerManager.CreateTimer(20, null, (o) => { ServerThread.Instance.Wakeup(); });

            //线程
            foreach (var backgroundThread in backgroundThreads)
            {
                backgroundThread.Setup();
                ThreadPool.QueueUserWorkItem(backgroundThread.Run);
            }

            // cmdServer.Start();

            Logger.Info(string.Format("{0} : {1} OnStarted ...", DateTime.Now.TimeOfDay, this.Name));
        }


        /// <summary>
        /// 在ServerThread执行
        /// </summary>
        public void DoWorkInServerThreadLoop(double elapsed)
        {
            Tick(elapsed);

#if DEBUG_LOG
            Logger.Debug($"{nameof(DoWorkInServerThreadLoop)} {elapsed}");
#endif

            var list = TryUpdateSessionCache();
            foreach (var peerStub in list)
            {
                peerStub.Tick(elapsed);
            }

            ServerManagerContainer.Tick(elapsed);

        }

        /// 在ServerThread执行
        protected virtual void Tick(double elapsed) 
        { 
        }

        public ServerPeer GetGatePeerBySessionId(uint sessionId)
        {
            return GetPeerByServerId(SessionIdHelper.GetGateServerId(sessionId));
        }


        //public ServerPeer GetGatePeerByServerId(ushort serverId)
        //{
        //    return ServerPeerManager.GetPeerByServerId(serverId);
        //}


        public ServerPeer GetPeerByServerId(ushort serverId)
        {
            return ServerPeerManager.GetPeer(serverId);
        }


        //todo  把supersocket的接口放一块
        // 3 进入PeerBase等类 继承的 OnSessionStarted
        // 2 连接成功后创建 session 底层调用这个函数会 用socketSession 初始化PeerBase


        protected override SessionHolder CreateAppSession(SocketSession socketSession)
        {
            ConnectParam connectParam = socketSession.ConnectParam as ConnectParam;
            if(connectParam == null)
            {
                var peerStub = new SessionHolder();
                return peerStub;
            }
            else
            {
                var peerStub = new SessionHolder();
                peerStub.UpdatePeer(connectParam.PeerID, connectParam.PeerType, ServerPeerManager);
                return peerStub;
            }

        }


        // 1 服务器主动链接
        public void ConnectServer(IPEndPoint endPoint, ConnectParam connectParam)
        {
            Logger.Info($"{ServerType} try to connect {endPoint}");
            var activeConnector = this as IActiveConnector;
            try
            {
                activeConnector.ActiveConnect(endPoint, connectParam);
            }
            catch (SocketException)
            {
                //todo 写点日志？
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        protected override void OnStopped()
        {
            //cmdServer.Stop();
            base.OnStopped();
            Logger.Info(string.Format("{0} : {1} OnStopped ...", DateTime.Now.TimeOfDay, this.Name));
        }



        public T GetManager<T>() where T : AfComponent
        {
            return ServerManagerContainer.GetComponent<T>();
        }

        public PeerBase GetPeerById(ushort peerId)
        {
            PeerBase peerBase = ClientPeerManager.GetPeer(peerId);
            if (peerBase == null)
                peerBase = ServerPeerManager.GetPeer(peerId);

            return peerBase;
        }
    }

}
