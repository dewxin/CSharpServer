using Newtonsoft.Json;
using ProjectCommon.Unit;
using ProtoBuf;
using ServerCommon.Net;
using Protocol;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDataCommon;
using CommonRpc.Net;
using Protocol.Service;
using CommonRpc.RpcBase;
using System.Diagnostics;
using CommonRpc.Rpc;
using CommonRpc;
using Autofac;

namespace ServerCommon
{

    public abstract class PeerServerEnd : PeerBase
    {
        protected virtual ServerBase Server => PeerHost as ServerBase;

        public override SessionHolder SessionHolder => base.SessionHolder as SessionHolder;

        protected PeerServerEnd() 
            : base(RavenSingleton.Get<IContainer>().Resolve<ServerBase>())
        {
        }

        //TODO 移动到GameServer那一层？
        public bool TryForwardMsg(ProtobufRequestInfo requestInfo)
        {
            if (!Server!.SupportForwardMsg)
                return false;

            if (!requestInfo.IsForward)
                return false;

            //转发 服务端->客户端
            if (requestInfo.ToTarget == ForwardTarget.Client)
            {
                var clientPeer = Server.GetPeerById(requestInfo.ClientPeerId);
                clientPeer.SessionHolder.RpcInvoker.SendMsg(requestInfo.ToMsgHeaderBody());
                return true;
            }

            //转发客户端->服务端 请求包
            if (requestInfo.FromTarget == ForwardTarget.Client)
            {
                ClientPeer client = this as ClientPeer;
                Debug.Assert(client != null);

                //requestInfo.ClientPeerId = Peer.PeerID;

                RpcInvoker forwardPeerInvoker = null;
                if (requestInfo.ToTarget == ForwardTarget.Logic)
                {
                    forwardPeerInvoker = client.GetLogicPeer().SessionHolder.RpcInvoker;
                }
                else if (requestInfo.ToTarget == ForwardTarget.World)
                {
                    forwardPeerInvoker = client.GetWorldPeer().SessionHolder.RpcInvoker;
                }

                forwardPeerInvoker.SendMsg(requestInfo.ToMsgHeaderBody());
                return true;
            }


            throw new Exception("should not reach here");
        }
    }

    public class ServerPeer : PeerServerEnd
    {

        public ServerPeer()
        {
        }


        public override void OnPeerClosed()
        {
            Server.Logger.Debug($"{nameof(OnPeerClosed)} remove peer {PeerID}");
            Server.ServerPeerManager.RemovePeer(this);

            //if (_heartbeatTimer != null)
            //{
            //    _heartbeatTimer.Stop();
            //    _heartbeatTimer = null;
            //}
        }


        public virtual void OnServerPeerRegistered()
        {
            //LoadPeerServerInfo();
        }
    }
}
