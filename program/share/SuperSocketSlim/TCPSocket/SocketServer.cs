using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocketSlim.Logging;

namespace SuperSocketSlim
{
    public class SocketServer
    {
        protected object SyncRoot = new object();

        public IAppServer AppServer { get; private set; }

        protected TcpSocketListener Listener { get; private set; }

        public IPEndPoint EndPoint { get; private set; }

        private ActiveConnector _Connector;
        public ActiveConnector Connector
        {
            get
            {
                if (_Connector == null)
                    _Connector = ActiveConnector.New(ProcessNewClient);

                return _Connector;
            }
        }

        private readonly int m_SendTimeOut;
        private readonly int m_ReceiveBufferSize;
        private readonly int m_SendBufferSize;



        public SocketServer(IAppServer appServer)
        {
            var config = appServer.ServerConfig;

            m_SendTimeOut = config.SendTimeOut;
            m_ReceiveBufferSize = config.ReceiveBufferSize;
            m_SendBufferSize = config.SendBufferSize;


            AppServer = appServer;
        }

        public bool StartBase()
        {
            ILog log = AppServer.Logger;

            Listener = new TcpSocketListener();
            Listener.NewClientAcceptedAsyncHandler += new NewClientAcceptHandler(OnNewClientAcceptedAsync);

            EndPoint = Listener.Start(AppServer.ServerConfig);

            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Listener ({0}) was started", EndPoint);
            }

            return true;
        }


        public void StopBase()
        {            
            Listener.Stop();
        }



        //TODO 这里感觉怪怪的
        protected IAppSession CreateSession(Socket client, SocketSession session)
        {
            if (m_SendTimeOut > 0)
                client.SendTimeout = m_SendTimeOut;

            if (m_ReceiveBufferSize > 0)
                client.ReceiveBufferSize = m_ReceiveBufferSize;

            if (m_SendBufferSize > 0)
                client.SendBufferSize = m_SendBufferSize;

            client.NoDelay = true;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            return this.AppServer.CreateAppSession(session);
        }

        public bool Start()
        {
            try
            {
                int byteCount = AppServer.ServerConfig.ReceiveBufferSize;

                if (byteCount <= 0)
                    byteCount = 4096;

                if (!StartBase())
                    return false;

                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                return false;
            }
        }

        protected void OnNewClientAcceptedAsync(TcpSocketListener listener, Socket client)
        {
            ProcessNewClient(client, null);
        }

        private IAppSession ProcessNewClient(Socket client, ConnectParam connectParam)
        {
            SocketSession socketSession;

            socketSession = new SocketSession(client);
            socketSession.ConnectParam = connectParam;

            var appSession = CreateSession(client, socketSession);

            if (appSession == null)
            {
                AppServer.Logger.Debug($"{nameof(ProcessNewClient)} Create Session fail, close socket");
                client.Close();
                return null;
            }

            socketSession.OnClosedHandler += SessionClosed;

            if (RegisterSession(appSession))
            {
                //Console.WriteLine("{0} AsyncRun peerid:",DateTime.Now, session.SessionID);
                //AppServer.AsyncRun(() => socketSession.Start());
                socketSession.Start();
                connectParam?.CallbackOnStarted?.Invoke(appSession);
            }

            return appSession;

        }

        private bool RegisterSession(IAppSession appSession)
        {
            if (AppServer.RegisterSession(appSession))
                return true;

            appSession.SocketSession.Close(CloseReason.InternalError);
            return false;
        }

        void SessionClosed(SocketSession session, CloseReason reason)
        {
        }

        public void Stop()
        {
            lock (SyncRoot)
            {
                StopBase();
            }
        }



    }
}
