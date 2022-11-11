using CommonRpc.Net;
using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    public class ServiceHandlerList : HandlerList<MessageServiceHandler>
    {
        readonly IHost peerEntity;

        private Dictionary<RpcServiceAttribute, MessageServiceHandler> rpcAttr2HandlerDict;


        public ServiceHandlerList(IEnumerable<MessageServiceHandler> handlers, IHost peerEntity)
        {
            this.peerEntity = peerEntity;
            rpcAttr2HandlerDict = new Dictionary<RpcServiceAttribute, MessageServiceHandler>();

            foreach (var handler in handlers)
            {
                handler.Init();
                RegisterHandler(handler);

                var rpcAttr = handler.RpcServiceAttribute;

                foreach(var ele in rpcAttr2HandlerDict)
                {
                    if(ele.Key.Conflict(rpcAttr))
                    {
                        var exceptionStr = $"{ele.Value.GetType().Name}[{ele.Key.MinMsgId}-{ele.Key.MaxMsgId}] conflict" +
                            $" with {handler.GetType().Name}[{rpcAttr.MinMsgId}-{rpcAttr.MaxMsgId}]";
                        throw new ArgumentException(exceptionStr);
                    }
                }
                rpcAttr2HandlerDict.Add(rpcAttr, handler);
                peerEntity.MyLog.Debug($"{nameof(ServiceHandlerList)} id:{rpcAttr.MinMsgId}-{rpcAttr.MaxMsgId} type:{handler.GetType().Name}");
            }

            rpcAttr2HandlerDict = null;

        }

    }

}
