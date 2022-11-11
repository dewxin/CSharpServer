using CommonRpc.Net;
using CommonRpc.Rpc;
using FlatSharp.Attributes;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.RpcBase
{

    public interface ICallback<out T>
    {
        Action<T> TCallback { set; }
    }

    //处理方 在返回结果给调用方前，可能需要先等待自身调用的接口返回。
    public interface IWaitPremise
    {
        void Wait(RpcResult premise);

        RpcResult Premise { get; set; }
        RpcResult Continue { get; set; }

        //TODO 存SessionHolder
        SessionHolderBase InvokerSessionHolder { get; set; }
        bool HasToWait { get; set; }
        void RemainContextIfWait(SessionHolderBase sessionHolder, IProtoMsg prevMsg);

        IProtoMsg PrevMsg { get; set; }
    }

    public interface IRpcResult
    {
        ushort InvokerInvokeId { get; set; }

        Func<byte[], Object> Deserializer { get; set; }
        RpcHandler RpcHandler { get; set; }

        bool HasCallback { get; }
        void InvokeCallback(byte[] data);
    }


    public class RpcResult<T> : RpcResult, ICallback<T>
        where T : RpcResult<T>
    {
        private Action<T> tCallback = null;
        [IgnoreMember]
        public Action<T> TCallback
        {
            get => tCallback;
            set
            {
                var thisEx = this as IRpcResult;
                if(value == null)
                {
                    //todo这里代码是不是有问题？？
                    if(tCallback!=null)
                        thisEx.RpcHandler.RemoveRpcResult(this);
                }
                else
                {
                    thisEx.RpcHandler.AddRpcResult(this);
                }
				tCallback = value;
            }
        }

        protected override bool HasCallback => TCallback != null;

        protected override void InvokeCallback(byte[] data)
        {
            if (InnerResult == null)
                ParseResult(data);
            TCallback?.Invoke(InnerResult as T);

            var continueRpc = (this as IWaitPremise).Continue;
            if (continueRpc != null)
            {
                (continueRpc as IWaitPremise).InvokerSessionHolder.RpcInvoker.SendResponse(continueRpc);
            }
        }
    }


    public abstract class RpcResult : IRpcResult, IWaitPremise, IDisposable
    {
        //todo 如果开启会有bug
        //~RpcResult()
        //{
        //    Dispose();
        //}

        public void Dispose()
        {

#if !DEBUG
            try
            {
#endif
                //todo 下一行应该是有哪个null指针 异常了
                var idGenerator = this_IRpcResult.RpcHandler.RpcInvoker.InvokeIdGenerator;
                idGenerator.ReleaseId(this_IRpcResult.InvokerInvokeId);

#if !DEBUG
            }
            catch (Exception ex)
            {
                //todo log
            }
#endif

        }

        #region IRpcResult
        private IRpcResult this_IRpcResult => this as IRpcResult;
        //调用方
        ushort IRpcResult.InvokerInvokeId { get; set; }
        Func<byte[], object> IRpcResult.Deserializer { get; set; }
        RpcHandler IRpcResult.RpcHandler { get; set; }
        bool IRpcResult.HasCallback => HasCallback;
        void IRpcResult.InvokeCallback(byte[] data) => InvokeCallback(data);

        #endregion

        protected abstract bool HasCallback { get; }
        protected abstract void InvokeCallback(byte[] data);

        protected object InnerResult { get; set; }
        protected object ParseResult(byte[] data)
        {
            InnerResult = this_IRpcResult.Deserializer(data);
            return InnerResult;
        }

        #region IWaitPremise 数据包处理方 等待前置Rpc完成 再发Response

        RpcResult IWaitPremise.Premise { get; set; }
        RpcResult IWaitPremise.Continue { get; set; }



        IProtoMsg IWaitPremise.PrevMsg { get; set; }
        SessionHolderBase IWaitPremise.InvokerSessionHolder { get;set;}
        bool IWaitPremise.HasToWait { get; set; } = false;

        private IWaitPremise This_IWaitPremise => this as IWaitPremise;


        void IWaitPremise.Wait(RpcResult premise)
        {
            This_IWaitPremise.HasToWait = true;
            This_IWaitPremise.Premise = premise;
            (This_IWaitPremise.Premise as IWaitPremise).Continue = this;
        }

        void IWaitPremise.RemainContextIfWait(SessionHolderBase peer, IProtoMsg prevMsg)
        {
            if (!This_IWaitPremise.HasToWait)
                return;

            This_IWaitPremise.InvokerSessionHolder = peer;
            This_IWaitPremise.PrevMsg = prevMsg;
        }

        #endregion

    }
}
