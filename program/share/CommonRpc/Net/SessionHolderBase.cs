using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    public interface ISessionHolder
    {
        void DisconnectAppError();
        void Tick(double elapsed);
        bool TrySend(List<ArraySegment<byte>> data);
    }

    public abstract class SessionHolderBase : ISessionHolder
    {
        public abstract void DisconnectAppError();

        public abstract void Tick(double elapsed);

        public abstract bool TrySend(List<ArraySegment<byte>> data);

        public RpcHandler RpcHandler { get; private set; }
        public RpcInvoker RpcInvoker { get; private set; }

        public virtual IHost HostBase { get; protected set; }

        private Dictionary<Type, object> interface2ServiceProxyDict = new Dictionary<Type, object>(); 
        public virtual PeerBase Peer { get; protected set; }

        public SessionHolderBase()
        {

        }

        public void Init(ServiceHandlerList serviceHandlerList, IHost host)
        {
            this.HostBase = host;
            this.RpcHandler = new RpcHandler(this, serviceHandlerList);
            this.RpcInvoker = new RpcInvoker(this);
        }

        public T GetServiceProxy<T>()
        where T : class
        {
            if (!interface2ServiceProxyDict.ContainsKey(typeof(T)))
            {
                var serviceProxy = RpcClientProxy.Resolve<T>(RpcInvoker);
                interface2ServiceProxyDict.Add(typeof(T), serviceProxy);
            }

            return interface2ServiceProxyDict[typeof(T)] as T;
        }
    }
}
