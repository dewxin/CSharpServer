using CommonRpc.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    public abstract class HandlerList<THandler>
    where THandler : MessageServiceHandler
    {
        public HandlerList()
        {
        }

        protected List<THandler> serviceHandlerList = new List<THandler>();

        protected bool RegisterHandler(THandler handler)
        {
            serviceHandlerList.Add(handler);

            return true;

        }



        public THandler FindServiceHandler(ushort msgId)
        {
            foreach(var handler in serviceHandlerList)
            {
                if (handler.SetCurrentMsgIdWhenCanHandle(msgId))
                    return handler;
            }

            return null;
        }

        /// 查找handler
        public THandler FindHandler(ushort msgId)
        {
            var serviceHandler = FindServiceHandler(msgId);
            if (serviceHandler != null)
                return serviceHandler;

            return default(THandler);
        }

    }
}
