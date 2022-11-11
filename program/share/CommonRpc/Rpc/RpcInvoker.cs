using CommonRpc.Net;
using CommonRpc.RpcBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Rpc
{
    public class RpcInvoker
    {
        //todo 时间跑长了看看这些字段有没有bug，是不是会 清空
        public RpcIdGenerator InvokeIdGenerator { get; private set; } = new RpcIdGenerator();
        public ushort CurrentInvokeId { get; set; }

        private SessionHolderBase sessionHolder;

        public RpcHandler RpcHandler => sessionHolder.RpcHandler;

        public RpcInvoker(SessionHolderBase sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        public ILog Logger => sessionHolder.HostBase.MyLog;

        public ushort ClientPeerID
        {
            get
            {
                if (sessionHolder.HostBase == null)
                    return 0;
                if (sessionHolder.HostBase.IsClient)
                    return sessionHolder.Peer.PeerID;
                return 0;
            }
        }


        #region SendRquest
        //发送消息入口 serverThread
        public TResponse SendRequestParamObjRetObj<TRequest, TResponse>(ushort msgId, TRequest request)
            where TRequest : class
            where TResponse : RpcResult<TResponse>,new()
        {

#if DEBUG
            Logger.Debug($"{nameof(SendRequestParamObjRetObj)}: {request.GetType().Name} {JsonConvert.SerializeObject(request)}");
#endif

            var rpcResult = GetRpcResult<TResponse>();

            //var packet = ProtobufHelper.ToMsg(msgId, request, (rpcResult as IRpcResult).InvokerInvokeId, 0);
            var invokeId = (rpcResult as IRpcResult).InvokerInvokeId;
            var msgHeaderBody = new MsgHeaderBody(msgId:msgId, invokeId:invokeId, clientPeerId: ClientPeerID);
            msgHeaderBody.SetMsgBody(request);

            SendMsg(msgHeaderBody);

            return rpcResult;
        }

        //发送消息入口 serverThread
        public TResponse SendRequestParamVoidRetObj<TResponse>(ushort msgId)
            where TResponse : RpcResult<TResponse>, new()
        {
            var rpcResult = GetRpcResult<TResponse>();

            //var packet = ProtobufHelper.ToMsg(msgId, request, (rpcResult as IRpcResult).InvokerInvokeId, 0);
            var invokeId = (rpcResult as IRpcResult).InvokerInvokeId;
            var msgHeaderBody = new MsgHeaderBody(msgId: msgId, invokeId: invokeId, clientPeerId: ClientPeerID);

            SendMsg(msgHeaderBody);

            return rpcResult;
        }

        public void SendRequestParamObjRetVoid<TRequest>(ushort msgId, TRequest request)
            where TRequest:class
        {
            //Logger.DebugFormat("PeerBase SendMsg: {0} {1}", request.GetType().Name, JsonConvert.SerializeObject(request));

            var msgHeaderBody = new MsgHeaderBody(msgId:msgId, clientPeerId:ClientPeerID);
            msgHeaderBody.SetMsgBody(request);
            SendMsg(msgHeaderBody);
        }


        public void SendRequestParamVoidRetVoid(ushort msgId)
        {
            var packet = new MsgHeaderBody(msgId:msgId, clientPeerId:ClientPeerID);
            SendMsg(packet);
        }

        #endregion


        private byte GetInvokeId()
        {
            //todo  int 转 ushort存在风险 (理论上有，实际上应该不会发生）
            var rawId = InvokeIdGenerator.GenerateId();
            checked
            {
                byte id = (byte)rawId;
                return id;
            }
        }


        private TResponse GetRpcResult<TResponse>()
            where TResponse : RpcResult<TResponse>, new()
        {

            var id = GetInvokeId();

            var rpcResult = new TResponse();
            var rpcResultEx = rpcResult as IRpcResult;
            rpcResultEx.InvokerInvokeId = id;

            // 设置为null的回调会在 tick里面会 release 对应的invokeId

            Func<byte[], Object> func = (buffers) => SerializerHelper.Deserialize(typeof(TResponse), buffers);
            rpcResultEx.Deserializer = func;

			rpcResultEx.RpcHandler = sessionHolder.RpcHandler;
            return rpcResult;
        }


        public bool SendResponse<T>(T msg) where T : RpcResult
        {
            var msgEx = msg as IWaitPremise;
            return SendResponse<T>(msg, msgEx.PrevMsg);
        }


        public bool SendResponse<T>(T msg, IProtoMsg prevMsg) where T : class
        {
            if (msg == null)
                return false;

            var msgHeaderBody = new MsgHeaderBody(msgId:prevMsg.MsgId, isReply: true, invokeId: prevMsg.InvokeId, clientPeerId:prevMsg.ClientPeerId);
            msgHeaderBody.SetMsgBody(msg);

            return SendMsg(msgHeaderBody);
        }


        public bool SendMsg(MsgHeaderBody msg)
        {

            //TODO 统计玩家流量，标记拉黑异常玩家
            try
            {
                return sessionHolder.TrySend(msg.GetData());
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                //todo 看看底层判断这里是不是有问题
                sessionHolder.DisconnectAppError();
                return false;
            }

        }
    }
}
