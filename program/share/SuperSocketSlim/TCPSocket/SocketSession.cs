using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim;
using SuperSocketSlim.Protocol;
using SuperSocketSlim.Logging;

namespace SuperSocketSlim
{
    public class SocketSession
    {
        public IAppSession AppSession { get; private set; }
        public ushort SessionID { get; private set; }
        public ServerConfig Config { get; set; }
        public Action<SocketSession, CloseReason> OnClosedHandler { get; set; }
        public Socket ClientSocket { get; set; }
        public virtual IPEndPoint LocalEndPoint { get; protected set; }
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }
        
        public ConnectParam ConnectParam { get; set; }
        public ILog Logger => AppSession.Logger;



        public SocketSession(Socket client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            ClientSocket = client;
            LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public virtual void Start()
        {
            StartReceive();

            OnSessionStarted();
        }


        public virtual void Close(CloseReason reason)
        {
            AppSession.AppServer.Logger.Info($"close socket Reason:{reason}");
            ClientSocket.Close();
            OnSessionClosed(reason);
        }


        public virtual void Initialize(IAppSession appSession)
        {
            AppSession = appSession;
            Config = appSession.Config;
        }


        protected virtual void OnSessionStarted()
        {
            AppSession.OnSessionStarted();
        }

        protected virtual void OnSessionClosed(CloseReason reason)
        {
            AppSession.OnSessionClosed(reason);
            OnClosedHandler?.Invoke(this, reason);
        }


        public virtual bool TrySend(ArraySegment<byte> segment)
        {
            //todo 改成异步的
            ClientSocket.Send(segment.Array, segment.Offset, segment.Count, SocketFlags.None);
            return true;
        }


        public bool TrySend(IList<ArraySegment<byte>> arraySegmentList)
        {
            foreach (var item in arraySegmentList)
            {
                //todo 改成异步的
                ClientSocket.Send(item.Array, item.Offset, item.Count, SocketFlags.None);
            }

            return true;
        }


        private void StartReceive()
        {
            //TODO 使用内存池提升效率
            byte[] receiveBuffer = new byte[1024];
            var receiveBufferArraySegment = new ArraySegment<byte>(receiveBuffer, 0, receiveBuffer.Length);
            //TODO 传输层协议改成UDP，这样一次能收完整个数据包
            var task = ClientSocket.ReceiveAsync(receiveBufferArraySegment, SocketFlags.None);

            task.ContinueWith(t => {

#if DEBUG
                //如果buffer不够大，应该是填满buffer然后函数返回了。可以测试一下
                if(task.IsFaulted)
                {
                    foreach(var ex in task.Exception.InnerExceptions)
                    {
                        if(ex is SocketException socketEx)
                        {
                            if(socketEx.SocketErrorCode == SocketError.ConnectionReset)
                            {
                                Logger.Debug($"{nameof(SocketSession)} {nameof(StartReceive)} task Faulted exception is {socketEx.SocketErrorCode.ToString()}");
                                //TODO 多线程问题
                                Close(CloseReason.SocketError);
                                return;
                            }
                        }
                    }

                }

                //TODO 这里会抛出异常
                ProcessReceivedDataAsync(receiveBufferArraySegment.Slice(0, task.Result));
                StartReceive();
#else
                try
                {
                    ProcessReceivedDataAsync(buffer, task.Result);
                }
                finally
                {
                    //发生socket异常是不是应该直接关闭socket
                    StartReceive();
                }

#endif
            });
        }

        public void ProcessReceivedDataAsync(ArraySegment<byte> buffer)
        {
            this.AppSession.ProcessRequest(buffer);
        }

    }
}
