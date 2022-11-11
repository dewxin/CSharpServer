using CommonRpc.Net;
using CommonRpc.RpcBase;
using Protocol;
using Protocol.Service;
using Protocol.Service.GateService;
using SuperSocketSlim;
using SuperSocketSlim.ClientEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace ClientNet.Unit
{
    public class ClientSessionHolder : SessionHolderBase 
    {
        EasyClient<ProtobufRequestInfo> socketClient = new EasyClient<ProtobufRequestInfo>();

        public NetClient MainClient { get; private set; }

        public NetPeer GatePeer { get; set; }

        public ClientSessionHolder(NetPeer gatePeer, ServiceHandlerList serviceHandlerList)
        {
            MainClient = gatePeer.MainClient;
            GatePeer = gatePeer;

            Init(serviceHandlerList, MainClient);

            {
                socketClient.Initialize(new ProtobufReceiveFilter());
                socketClient.HandlePackage += this.HandlePackage;
                socketClient.OnConnected += gatePeer.OnConnected;
                socketClient.OnClosed += gatePeer.OnClosed;
            }
        }

        public void DisconnectSkError()
        {
            socketClient.Close();
        }


        public override void Tick(double elapsed)
        {
            if (_recvList.IsEmpty)
                return;

            MainClient.Logger.Debug($"recvList count is {_recvList.Count}");

            int i = 0;
            ProtobufRequestInfo procinfo;

            while (i < 100 && _recvList.TryDequeue(out procinfo))
            {

                GatePeer.SessionHolder.RpcHandler.HandlePacketInServerTick(procinfo);
            }

        }

        public override bool TrySend(List<ArraySegment<byte>> data)
        {
            try
            {
                socketClient.Send(data);
            }
            catch (Exception ex)
            {
                MainClient.Logger.Error($"{ex.ToString()}");
                return false;
            }

            return true;
        }

        public void Connect(EndPoint remoteEndPoint)
        {
            socketClient.Connect(remoteEndPoint); 
        }


        ConcurrentQueue<ProtobufRequestInfo> _recvList = new ConcurrentQueue<ProtobufRequestInfo>(); //todo 换成 currentQueue

        void HandlePackage(ProtobufRequestInfo package)
        {
            //接收协议
            _recvList.Enqueue(package);
        }

        public override void DisconnectAppError()
        {
            socketClient.Close();
        }

    }

    public partial class NetPeer : PeerBase
    {
        public NetClient MainClient { get; private set; }

        public ILog MyLog => MainClient.MyLog;

        public ServiceHandlerList MessageHandlerList { get; private set; }

        public new ClientSessionHolder PeerStub { get => base.SessionHolder as ClientSessionHolder; set => base.SessionHolder = value; }

        public NetPeer(ServiceHandlerList handlerList, NetClient MainClient)
            :base(MainClient)
        {
            MessageHandlerList = handlerList;
            this.MainClient = MainClient;

            PeerStub = new ClientSessionHolder(this, handlerList);

        }

        public void ConnectServer(string ip, int port)
        {
            IPEndPoint ipend = new IPEndPoint(IPAddress.Parse(ip), port);
            PeerStub.Connect(ipend);
        }

        public void Tick(double elapsed)
        {
            PeerStub.Tick(elapsed);
        }


        public void OnConnected(object sender, EventArgs e)
        {
            MyLog.Debug($"OnConnected:{MainClient.ClientInfo.Account}");
        }

        public void OnError(object sender,  EventArgs e)
        {
            MyLog.Error($"OnError:{MainClient.ClientInfo.Account} ex:{e}");
            Disconnect();
        }

        public void OnClosed(SocketSession socketSession, CloseReason closeReason)
        {
            MyLog.Debug($"OnClosed:{MainClient.ClientInfo.Account}");

        }


    }
}
