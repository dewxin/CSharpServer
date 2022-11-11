using CommonRpc.Net;
using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    public interface IPeer
    {
        ushort PeerID { get; }
        void Disconnect();
    }

    //todo 感觉可以重构一下
    public interface ILog
    {
        //调用时会用$"ddd{var1.param}"形式，会生成很多小对象
        void Debug(string message);

        void DebugFormat(string format, params object[] args);

        void Error(string message);

        void ErrorFormat(string format, params object[] args);
    }

    public interface IHost
    {
        ILog MyLog { get;}

        bool IsClient { get; }

        PeerBase CurrentPeer { get; set; }
    }


    //TODO 能不能IDE 检测实现IPeer接口的类是不是 .Name.EndWith(string suffix)
    public abstract class PeerBase : IPeer
    {
        //服务端的PeerID 应该由Eureka分配
        //客户端的PeerID 应该由自己的GateServer分配
        public ushort PeerID { get; set; }


        //todo 换个写法 现在很容易出bug
        public virtual SessionHolderBase SessionHolder { get; set; }
        public ILog Logger => PeerHost.MyLog;

        public ushort CurrentInvokeId => SessionHolder.RpcInvoker.CurrentInvokeId;

        //todo Server 应该换成 PeerEntity 表示这个可能是Server 也可能是client
        public virtual IHost PeerHost { get; private set; }


        private static Dictionary<string, Type> peerTypeStr2TypeDict = new Dictionary<string, Type>();

        public PeerBase(IHost peerHost)
        {
            this.PeerHost = peerHost;
        }

        public T GetServiceProxy<T>() where T : class
        {
            return SessionHolder.GetServiceProxy<T>();
        }


        public static void RegisterPeerType(string peerTypeName, Type peerType)
        {
            peerTypeStr2TypeDict.Add(peerTypeName, peerType);
        }
        public static Type GetPeerTypeByName(string name)
        {
            return peerTypeStr2TypeDict[name];
        }

        //TODO 这里有问题
        public void Disconnect()
        {
            SessionHolder.DisconnectAppError();
        }


        protected MessageServiceHandler GetHandler(ushort msgId)
        {
            return SessionHolder.RpcHandler.GetHandler(msgId);
        }



        public virtual void OnPeerStarted()
        {
        }

        public virtual void OnPeerClosed()
        {
        }
    }
}
