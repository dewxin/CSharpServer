using CommonRpc.Net;
using CommonRpc.RpcBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Rpc
{
    //todo 统计RPC函数调用的累计消耗时间

    public abstract class MessageServiceHandler
    {
        public RpcServiceAttribute _RpcServiceAttribute;
        public RpcServiceAttribute RpcServiceAttribute => _RpcServiceAttribute;
        public virtual SessionHolderBase SessionHolder { get; set; }

        public bool RpcFuncNotExist { get; set; } = false;

        protected Dictionary<ushort, MethodMeta> id2MethodMetaDict = new Dictionary<ushort, MethodMeta>();

        private ushort CurrentInvokeMsgId { get; set; }

        protected IHost host;
        public MessageServiceHandler(IHost host)
        {
            this.host = host;
        }

        public object ParseRequestPacket(byte[] buffers)
        {
            // 有些函数请求没有参数 void
            if(GetRequestType() == typeof(void))
                return null;

            return SerializerHelper.Deserialize(GetRequestType(), buffers);
        }

        protected Type GetRequestType()
        {
            return id2MethodMetaDict[CurrentInvokeMsgId].RequestType;
        }

        public bool SetCurrentMsgIdWhenCanHandle(ushort msgId)
        {
            var canHandle = id2MethodMetaDict.ContainsKey(msgId);

            if (canHandle)
                CurrentInvokeMsgId = msgId;
            return canHandle;
        }

        public RpcResult OnHandleMessage(object packet)
        {
            //TODO 顺便统计数据
            var method = id2MethodMetaDict[CurrentInvokeMsgId];
#if DEBUG
            host.MyLog.Debug($"{nameof(OnHandleMessage)}: {method.MethodName}  {JsonConvert.SerializeObject(packet)}");
#endif

#if !DEBUG
            try
            {
#endif
                return method.Invoke(this, packet) as RpcResult;
#if !DEBUG
            }
            catch (Exception ex)
            {

                host.MyLog.Debug($"{nameof(OnHandleMessage)}: catch exception {ex.Message}");
                return null;
            }
#endif
        }

        public void Init()
        {
            (id2MethodMetaDict, _RpcServiceAttribute) = RpcTool.GetRpcMethodDictAndServiceAttr(this.GetType());
        }

    }

}
