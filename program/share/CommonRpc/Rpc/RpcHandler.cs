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

    public interface IProtoMsg
    {
        ushort InvokeId { get;  }
        ushort MsgId { get;  }
        bool IsReply { get;  }
        ushort ClientPeerId { get;  }
        bool IsForward { get; set; }
        ForwardTarget FromTarget { get; set; }
        ForwardTarget ToTarget { get; set; }

        byte[] Body { get;  }
        object Protobuf { get;  set; }

    }

    public class RpcHandler
    {

        // 主动invokeid  回调处理其他服务器的 Response
        protected Dictionary<ushort, RpcResult> invokeId2ResultDict = new Dictionary<ushort, RpcResult>();

        private ServiceHandlerList msgHandlerList;
        private SessionHolderBase sessionHolder;

        public ILog Logger => sessionHolder.HostBase.MyLog;
        public RpcInvoker RpcInvoker => sessionHolder.RpcInvoker;

        public RpcHandler(SessionHolderBase sessionHolder, ServiceHandlerList ServiceHandlerList)
        {
            this.sessionHolder = sessionHolder;
            this.msgHandlerList = ServiceHandlerList;
        }

        public void AddRpcResult(RpcResult rpcResult)
        {
            var id = (rpcResult as IRpcResult).InvokerInvokeId;
            invokeId2ResultDict[id]= rpcResult;
        }

        
        public void RemoveRpcResult(RpcResult rpcResult)
        {
            var id = (rpcResult as IRpcResult).InvokerInvokeId;
            invokeId2ResultDict.Remove(id);
            rpcResult.Dispose();
        }


        protected bool TryParseProtobuf(IProtoMsg requestInfo, out object param)
        {
            //正常请求
            var handler = GetHandler(requestInfo.MsgId);
            if (handler != null)
            {
#if DEBUG
                param = handler.ParseRequestPacket(requestInfo.Body);
                return true;
#else
                try
                {
                    param = handler.ParseRequestPacket(requestInfo.Body);
                    return true;
                }
                catch (Exception)
                {
                    //todo 处理log
                    //Logger.ErrorFormat("PeerBase ParseProtobuf failed");
                    param =null;
                    return false;
                }
#endif
            }

            param = null;
            return false;
        }


        public void HandlePacketInServerTick(IProtoMsg protoInfo)
        {
            // 对端发送的数据包 是response,这个response由之前注册的callback处理
#if DEBUG
            Logger.Debug($"{nameof(HandlePacketInServerTick)} {JsonConvert.SerializeObject(protoInfo)}");
#endif
            if(protoInfo.IsReply)
            {
                if (!invokeId2ResultDict.ContainsKey(protoInfo.InvokeId))
                {
                    Logger.Error($"{nameof(HandlePacketInServerTick)} replyId {protoInfo.InvokeId} not found");
                    throw new Exception("replyId cannot be found");
                }

                {
                    var rpcResult = invokeId2ResultDict[protoInfo.InvokeId];

                    var rpcResultEx = rpcResult as IRpcResult;

                    //todo 将protoInfo.Body中的值解析到rpcResult
                    //而不是重新生成一个rpcResult
                    if (rpcResultEx.HasCallback)
                        rpcResultEx.InvokeCallback(protoInfo.Body);
                    else
                        Logger.Error($"{nameof(HandlePacketInServerTick)} replyId {protoInfo.InvokeId}  callback is null");

                    RemoveRpcResult(rpcResult);
                }
            }
            //对方发送的数据包是 request， 这个request由handler处理
            else
            {
                HandleServiceRpc(protoInfo);
            }
        }


        private void HandleServiceRpc(IProtoMsg protoInfo)
        {
            var handler = msgHandlerList.FindHandler(protoInfo.MsgId);
            if (handler != null)
            {
                bool parseProtobufSucceed = TryParseProtobuf(protoInfo, out object param);
                protoInfo.Protobuf = param;

                if (!parseProtobufSucceed)
                {
                    //todo log
                    return;
                }


#if !DEBUG
                try
                {
#endif
                handler.SessionHolder = this.sessionHolder;

                RpcInvoker.CurrentInvokeId = protoInfo.InvokeId;

                var result = handler.OnHandleMessage(protoInfo.Protobuf);
                if(result != null)
                {
                    if ((result as IWaitPremise).HasToWait)
                        (result as IWaitPremise).RemainContextIfWait(this.sessionHolder, protoInfo);
                    else
                        RpcInvoker.SendResponse(result, protoInfo);
                }
#if !DEBUG
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }

#endif

            }
            else
            {
#if DEBUG
                throw new Exception("Cannot handle message");
#endif
                //todo 记录一下
            }
        }

        public MessageServiceHandler GetHandler(ushort msgId)
        {
            return msgHandlerList.FindHandler(msgId);
        }

    }
}
