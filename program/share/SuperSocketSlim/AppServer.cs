using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim.Logging;

namespace SuperSocketSlim
{
    public abstract class AppServer<TAppSession> : IAppServer, IActiveConnector
        where TAppSession : class, IAppSession
    {
        public AppServer()
        {
            
        }


        /// <summary>
        /// Starts this AppServer instance.
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            if (!StartBase())
                return false;

            if (ServerConfig.ClearIdleSession)
                StartClearSessionTimer();

            return true;
        }

        //private ConcurrentDictionary<ushort, TAppSession> m_SessionDict = new ConcurrentDictionary<ushort, TAppSession>();

        private List<TAppSession> sessionList = new List<TAppSession>();
        private int sessionVersion = 0;

        protected List<TAppSession> sessionListCache = new List<TAppSession>();
        private int sessionCacheVersion = 0;


        //为了避免多线程竞争,服务端逻辑线程定期获取Cache
        public List<TAppSession> TryUpdateSessionCache()
        {
            if(sessionVersion == sessionCacheVersion)
                return sessionListCache;

#if DEBUG
            Logger.Debug($"{nameof(TryUpdateSessionCache)} updated ");
#endif
            if(Monitor.TryEnter(sessionList))
            {
                try
                {
                    sessionListCache.Clear();
                    sessionListCache.AddRange(sessionList);
                    sessionCacheVersion = sessionVersion;
                }
                finally
                {
                    Monitor.Exit(sessionList);
                }
            }

            return sessionListCache;
        }


        /// <summary>
        /// Registers the session into the session container.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="appSession">The app session.</param>
        /// <returns></returns>
        protected virtual bool RegisterSession(TAppSession appSession)
        {
            lock(sessionList)
            {
                sessionList.Add(appSession);
                sessionVersion++;

#if DEBUG
                Logger.Debug($"{nameof(RegisterSession)} {appSession.GetType()}");
#endif

                return true;
            }
        }

        #region Clear idle sessions

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval = ServerConfig.ClearIdleSessionInterval * 1000;//in milliseconds
            m_ClearIdleSessionTimer = new System.Threading.Timer(ClearIdleSession, null, interval, interval);
        }

        /// <summary>
        /// Clears the idle session.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ClearIdleSession(object state)
        {
            DateTime now = DateTime.Now;
            DateTime timeOut = now.AddSeconds(0 - ServerConfig.IdleSessionTimeOut);
            try
            {
                //todo 使用最小堆检测过时
                List<TAppSession> timeoutSessionList = new List<TAppSession>();
                lock (sessionList)
                {
                    foreach(var session in sessionList)
                    {
                        if (session.Persistent)
                            continue;
                        if (session.LastActiveTime > timeOut)
                            continue;
                        timeoutSessionList.Add(session);
                    }
                    foreach (var session in timeoutSessionList)
                    {
                        sessionList.Remove(session);
                        sessionVersion++;
                    }
                }

                foreach(var session in timeoutSessionList)
                {
                    if (Logger.IsInfoEnabled)
                    {
                        string info = $"Session be closed for timeout {session.RemoteEndPoint.Address}";
                        Logger.Info(info);
                    }

                    session.Close(CloseReason.TimeOut);
                }
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("Clear idle session error!", e);
            }
        }

        #endregion

        #region Search session utils

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
            SocketServer.Stop();

            OnStopped();

            if (Logger.IsInfoEnabled)
                Logger.Info(string.Format("The server instance {0} has been stopped!", Name));

            if (m_ClearIdleSessionTimer != null)
            {
                m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_ClearIdleSessionTimer.Dispose();
                m_ClearIdleSessionTimer = null;
            }

            foreach (var session in sessionList)
            {
                session.Close(CloseReason.ServerShutdown);
            }
        }

        #endregion

        public ServerConfig ServerConfig { get; private set; }
        public CommonConfig CommonConfig { get; private set; }
        public SupersocketConfig AllConfig { get; private set; }

        //Server instance name
        private string m_Name;
        public ILog Logger { get; private set; }
        protected IBootstrap Bootstrap { get; private set; }
        public DateTime StartedTime { get; private set; }
        public ILogFactory LogFactory { get; private set; }



        protected abstract bool Setup(ServerConfig config);


        private void SetupBasic(ServerConfig config, CommonConfig commonConfig)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (!string.IsNullOrEmpty(config.Name))
                m_Name = config.Name;
            else
                m_Name = this.GetType().Name;

            ServerConfig = config;
            CommonConfig = commonConfig;

        }


        public bool Setup(IBootstrap bootstrap, ServerConfig config, CommonConfig commonConfig)
        {
            if (bootstrap == null)
                throw new ArgumentNullException("bootstrap");

            Bootstrap = bootstrap;

            SetupBasic(config, commonConfig);

            log4net.GlobalContext.Properties["LogAppName"] = m_Name;
            if (!SetupLogFactory(new Log4NetLogFactory()))
                return false;

            Logger = CreateLogger(this.Name);

            if (!Setup(config))
                return false;

            if (!SetupSocketServer())
                return false;

            return true;
        }


        private bool SetupLogFactory(ILogFactory logFactory)
        {
            if (logFactory != null)
            {
                LogFactory = logFactory;

                return true;
            }

            //Log4NetLogFactory is default log factory
            if (LogFactory == null)
            {
                log4net.GlobalContext.Properties["LogAppName"] = m_Name;
                LogFactory = new Log4NetLogFactory();
            }

            return true;
        }


        /// <summary>
        /// Creates the logger for the AppServer.
        /// </summary>
        /// <param name="loggerName">Name of the logger.</param>
        /// <returns></returns>
        protected virtual ILog CreateLogger(string loggerName)
        {
            return LogFactory.GetLog(loggerName);
        }

        /// <summary>
        /// Setups the socket server.instance
        /// </summary>
        /// <returns></returns>
        private bool SetupSocketServer()
        {
            try
            {
                //TODO 通过配置确定创建的SocketServer
                SocketServer = new SocketServer(this);
                return SocketServer != null;
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(e);

                return false;
            }
        }


        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        public SocketServer SocketServer { get; set; }

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        public bool StartBase()
        {
            if (!SocketServer.Start())
                return false;

            StartedTime = DateTime.Now;

            try
            {
                OnStarted();
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error("One exception wa thrown in the method 'OnStartup()'.", e);
                }
            }
            finally
            {
                if (Logger.IsInfoEnabled)
                    Logger.Info(string.Format("The server instance {0} has been started!", Name));
            }

            return true;
        }

        /// <summary>
        /// Called when [started].
        /// </summary>
        protected virtual void OnStarted()
        {

        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
        protected virtual void OnStopped()
        {

        }



        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession IAppServer.CreateAppSession(SocketSession socketSession)
        {
            var appSession = CreateAppSession(socketSession);

            appSession.Initialize(this, socketSession);

            return appSession;
        }

        protected abstract TAppSession CreateAppSession(SocketSession socketSession);

        bool IAppServer.RegisterSession(IAppSession session)
        {
            var appSession = session as TAppSession;

            if (!RegisterSession(appSession))
                return false;

            if (ServerConfig.LogBasicSessionActivity && Logger.IsInfoEnabled)
                Logger.Info("A new session connected!");

            return true;
        }



        Task IActiveConnector.ActiveConnect(EndPoint targetEndPoint, ConnectParam state)
        {
            return SocketServer.Connector.ActiveConnect(targetEndPoint, state);
        }

        Task IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
        {
            return SocketServer.Connector.ActiveConnect(targetEndPoint, null);
        }
    }
}
