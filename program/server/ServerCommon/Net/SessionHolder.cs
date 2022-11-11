using Autofac;
using CommonRpc;
using CommonRpc.Net;
using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using Newtonsoft.Json;
using ProjectCommon.Unit;
using ProtoBuf;
using Protocol;
using ServerCommon.Net;
using SuperSocketSlim;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class SessionHolder :SessionHolderBase, IAppSession
    {
        protected SafeQueue<ProtobufRequestInfo> recvQueue = new(256);


        AppSession<ProtobufRequestInfo> appSession = new AppSession<ProtobufRequestInfo>(new ProtobufReceiveFilter());

        public override PeerServerEnd Peer => base.Peer as PeerServerEnd;

        public SessionHolder()
        {
            var server = RavenSingleton.Get<IContainer>().Resolve<ServerBase>();
            var msgHandlerList = RavenSingleton.Get<IContainer>().Resolve<ServiceHandlerList>();
            Init(msgHandlerList, server);
            InitAppSession();
        }


        public void OnSessionClosedForward(CloseReason closeReason)
        {
            Peer?.OnPeerClosed();
        }

        //public void OnSessionStartedForward()
        //{
        //}

        public void InitAppSession()
        {
            appSession.OnExecuteCommand += ExecuteCommandVirtual;
            //appSession.OnSessionStarted += OnSessionStartedForward;
            appSession.OnSessionClosed += OnSessionClosedForward;
        }




        public ServerPeer CreatePeer(Type type)
        {
            return RavenSingleton.Get<IContainer>().Resolve(type) as ServerPeer;
        }

        /// <summary>
        /// 由于主动连接或者被动连接的服务器类型可能有多种
        /// 因此会在updatePeer函数里更新对应的Peer类型，
        /// </summary>
        public ServerPeer UpdatePeer(ushort peerId, string peerType, ServerPeerManager serverPeerManager)
        {
            var peer = CreatePeer(PeerBase.GetPeerTypeByName(peerType));

#if DEBUG_LOG
            peer.Logger.Debug($"{nameof(UpdatePeer)} serverId:{serverId} peerType:{peerType}");
#endif

            peer.PeerID = peerId;

            this.Peer = peer;
            peer.SessionHolder = this;

            //TODO里面的逻辑可能有问题
            serverPeerManager.AddPeer(peer);
            peer.OnPeerStarted();

            this.appSession.Persistent = true;

            return peer;
        }


        //接收消息入口 supersocket Thread
        public void ExecuteCommandVirtual(ProtobufRequestInfo requestInfo)
        {
            if(Peer is PeerServerEnd peerOnServerEnd)
            {
                if (peerOnServerEnd.TryForwardMsg(requestInfo))
                {
                    //转发数据包成功，直接返回
                    return;
                }
            }


            //todo 反序列化的代码可以放 supersocket线程
#if DEBUG_LOG
            Peer.Logger.Debug($"{nameof(ExecuteCommandVirtual)} {JsonConvert.SerializeObject(requestInfo)}");
#endif
            bool enqueueSucceed = recvQueue.Enqueue(requestInfo);
            if(!enqueueSucceed)
            {
                //todo 可能设计上有问题，客户端的请求超过了服务器CPU的负载, 或者是有客户端在攻击
            }

            ServerThread.Instance.Wakeup();
        }



        //todo elapsed参数放到GameTime里面

        public override void Tick(double elapsed)
        {
            if (recvQueue.IsEmpty)
                return;
#if DEBUG_LOG
            AppServer.Logger.Debug($"{nameof(Tick)}: {elapsed}");
#endif
            //var outTimer = CheckTimeHelper.StartTime();

            while(!recvQueue.IsEmpty)
            {
                var protoInfo = recvQueue.Dequeue();

                HostBase.CurrentPeer = Peer;
                RpcHandler.HandlePacketInServerTick(protoInfo);

            }

            //if (outTimer.IntervalGreaterThan(0.5))
            //    outTimer.Error($"Heartbeat-{i } proc:{ GetType().Name} ");
        }

        public override void DisconnectAppError()
        {
            appSession.Close(CloseReason.ApplicationError);
        }

        public override bool TrySend(List<ArraySegment<byte>> data)
        {
            return appSession.TrySend(data);
        }

        #region IAppSession

        public ushort SessionID => ((IAppSession)appSession).SessionID;

        public IPEndPoint RemoteEndPoint => ((IAppSession)appSession).RemoteEndPoint;

        public ServerConfig Config => ((IAppSession)appSession).Config;

        public IAppServer AppServer => ((IAppSession)appSession).AppServer;

        public SocketSession SocketSession => ((IAppSession)appSession).SocketSession;

        public IPEndPoint LocalEndPoint => ((IAppSession)appSession).LocalEndPoint;

        public DateTime LastActiveTime { get => ((IAppSession)appSession).LastActiveTime; set => ((IAppSession)appSession).LastActiveTime = value; }

        public DateTime StartTime => ((IAppSession)appSession).StartTime;

        public bool Connected => ((IAppSession)appSession).Connected;

        public bool Persistent { get => ((IAppSession)appSession).Persistent; set => ((IAppSession)appSession).Persistent = value; }

        public SuperSocketSlim.Logging.ILog Logger => ((IAppSession)appSession).Logger;

        Action<CloseReason> IAppSession.OnSessionClosed { get => ((IAppSession)appSession).OnSessionClosed; set => ((IAppSession)appSession).OnSessionClosed = value; }
        Action IAppSession.OnSessionStarted { get => ((IAppSession)appSession).OnSessionStarted; set => ((IAppSession)appSession).OnSessionStarted = value; }

        public void Close()
        {
            ((IAppSession)appSession).Close();
        }

        public void Close(CloseReason reason)
        {
            ((IAppSession)appSession).Close(reason);
        }

        public void ProcessRequest(ArraySegment<byte> buffer)
        {
            ((IAppSession)appSession).ProcessRequest(buffer);
        }

        public void Initialize(IAppServer server, SocketSession socketSession)
        {
            ((IAppSession)appSession).Initialize(server, socketSession);
        }

        public bool ExecuteCommand(SessionRequestInfo requestInfo)
        {
            return ((IAppSession)appSession).ExecuteCommand(requestInfo);
        }



        #endregion
    }


}
