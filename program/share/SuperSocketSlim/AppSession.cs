using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim.Logging;
using SuperSocketSlim.Protocol;

namespace SuperSocketSlim
{
    public enum CloseReason : int
    {
        /// <summary>
        /// The socket is closed for unknown reason
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Close for server shutdown
        /// </summary>
        ServerShutdown = 1,

        /// <summary>
        /// The client close the socket
        /// </summary>
        ClientClosing = 2,

        /// <summary>
        /// The server side close the socket
        /// </summary>
        ServerClosing = 3,

        /// <summary>
        /// Application error
        /// </summary>
        ApplicationError = 4,

        /// <summary>
        /// The socket is closed for a socket error
        /// </summary>
        SocketError = 5,

        /// <summary>
        /// The socket is closed by server for timeout
        /// </summary>
        TimeOut = 6,

        /// <summary>
        /// Protocol error 
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// SuperSocket internal error
        /// </summary>
        InternalError = 8,
    }

    /// <summary>
    /// AppSession base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class AppSession<TRequestInfo> : IAppSession<TRequestInfo>
        where TRequestInfo : SessionRequestInfo
    {
        #region Properties

        public virtual IAppServer AppServer { get; private set; }

        public event Action<TRequestInfo> OnExecuteCommand;
        public Action<CloseReason> OnSessionClosed { get; set; } = delegate { };
        public Action OnSessionStarted { get; set; } = delegate { };


        public bool Connected
        {
            get;
            internal set;
        }

        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        public ILog Logger
        {
            get { return AppServer.Logger; }
        }

        /// <summary>
        /// If Persistent, the connection will not time out
        /// </summary>
        public bool Persistent { get; set; }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; private set; }

        public ushort SessionID => SocketSession.SessionID;

        public SocketSession SocketSession { get; private set; }

        public ServerConfig Config => AppServer?.ServerConfig;

        public object ConnectParam => SocketSession.ConnectParam; 

        IReceiveFilter m_ReceiveFilter;

        #endregion

        public AppSession(IReceiveFilter receiveFilter)
        {
            this.StartTime = DateTime.Now;
            this.LastActiveTime = this.StartTime;
            this.m_ReceiveFilter = receiveFilter;
        }

        public virtual void Initialize(IAppServer appServer, SocketSession socketSession)
        {
            AppServer = appServer;
            SocketSession = socketSession;

            m_ReceiveFilter.Initialize(AppServer, this);

            socketSession.Initialize(this);
        }


        protected virtual void HandleException(Exception e)
        {
            Logger.Error(this, e);
            this.Close(CloseReason.ApplicationError);
        }

        internal void InternalHandleExcetion(Exception e)
        {
            HandleException(e);
        }

        public virtual void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
        }

        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        #region Sending processing

        public virtual bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (!SocketSession.TrySend(segments))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        #endregion

        #region Receiving processing

        protected virtual int GetMaxRequestLength()
        {
            return AppServer.ServerConfig.MaxRequestLength;
        }

        void IAppSession.ProcessRequest(ArraySegment<byte> buffer)
        {
            m_ReceiveFilter.ParseAndTryExecuteCommand(buffer);
        }

        public bool ExecuteCommand(SessionRequestInfo sessionRequestInfo)
        {
            TRequestInfo requestInfo = sessionRequestInfo as TRequestInfo;
            OnExecuteCommand(requestInfo);
            LastActiveTime = DateTime.Now;

            return true;
        }



        #endregion
    }

}
