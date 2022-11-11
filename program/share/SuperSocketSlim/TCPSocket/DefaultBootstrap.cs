using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim;

namespace SuperSocketSlim
{
    /// <summary>
    /// SuperSocket default bootstrap
    /// </summary>
    public partial class DefaultBootstrap : IBootstrap
    {
        private List<IAppServer> m_AppServers = new List<IAppServer>();

        /// <summary>
        /// Indicates whether the bootstrap is initialized
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// Global configuration
        /// </summary>
        public SupersocketConfig SupersocketConfig { get; private set; }
        public CommonConfig  CommonConfig {get;private set; }

        /// <summary>
        /// Global log
        /// </summary>
        //private ILog m_GlobalLog;

        /// <summary>
        /// Gets the bootstrap logger.
        /// </summary>
        //ILog ILoggerProvider.Logger
        //{
        //    get { return m_GlobalLog; }
        //}

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IAppServer> AppServers
        {
            get { return m_AppServers; }
        }


        public static IBootstrap CreateBootstrap(params string[] configFileList)
        {
            SupersocketConfig supersocketConfig = null;
            CommonConfig commonConfig = null;
            foreach (var jsonConfigFile in configFileList)
            {
                // Build a config object, using env vars and JSON providers.
                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile(jsonConfigFile)
                    .AddEnvironmentVariables()
                    .Build();

                if (config == null)
                    throw new ConfigurationErrorsException("Invalid 'superSocket' or 'socketServer' configuration section.");

                // Get values from the config given their key and their target type.
                if(config.GetSection("Supersocket").Exists())
                    supersocketConfig = config.GetRequiredSection("Supersocket").Get<SupersocketConfig>();

                if(config.GetSection("Common").Exists())
                    commonConfig = config.GetRequiredSection("Common").Get<CommonConfig>();

            }

            if (supersocketConfig == null || commonConfig == null)
                throw new ConfigurationErrorsException($"{nameof(CreateBootstrap)} cannot parse config.");
            return new DefaultBootstrap(supersocketConfig, commonConfig);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="superConfig">The config.</param>
        public DefaultBootstrap(SupersocketConfig superConfig, CommonConfig commonConfig)
        {
            SupersocketConfig = superConfig;
            CommonConfig = commonConfig;

            //AppDomain.CurrentDomain.SetData("Bootstrap", this);
        }

        //void exceptionSource_ExceptionThrown(object sender, ErrorEventArgs e)
        //{
        //    m_GlobalLog.Error(string.Format("The server {0} threw an exception.", ((IAppServer)sender).Name), e.Exception);
        //}

        //void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    m_GlobalLog.Error("The process crashed for an unhandled exception!", (Exception)e.ExceptionObject);
        //}

        public bool Init()
        {
            if (m_Initialized)
                return false;

            //Initialize servers
            foreach (var serverConfig in SupersocketConfig.ServerList)
            {

                if (string.IsNullOrEmpty(serverConfig.Name))
                    throw new Exception("The name attribute of server node is required!");

                if (string.IsNullOrEmpty(serverConfig.ServerType))
                    throw new Exception("serverType attribute of the server node is required!");

                var server = CreateServerByType(serverConfig.ServerType);
                server.Setup(this, serverConfig, CommonConfig);
                m_AppServers.Add(server);

            }

            m_Initialized = true;
            return true;
        }

        protected virtual IAppServer CreateServerByType(string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName, true);
            return Activator.CreateInstance(serviceType) as IAppServer;
        }


        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        public StartResult Start()
        {
            if (!m_Initialized)
            {
                //if (m_GlobalLog.IsErrorEnabled)
                //    m_GlobalLog.Error("You cannot invoke method Start() before initializing!");

                return StartResult.Failed;
            }

            var result = StartResult.None;

            var succeeded = 0;

            foreach (var server in m_AppServers)
            {
                if (!server.Start())
                {
                    //if (m_GlobalLog.IsErrorEnabled)
                    //    m_GlobalLog.InfoFormat("The server instance {0} has failed to be started!", server.Name);
                }
                else
                {
                    succeeded++;

                    //if (m_GlobalLog.IsInfoEnabled)
                    //    m_GlobalLog.InfoFormat("The server instance {0} has been started!", server.Name);
                }
            }

            if (m_AppServers.Any())
            {
                if (m_AppServers.Count == succeeded)
                    result = StartResult.Success;
                else if (succeeded == 0)
                    result = StartResult.Failed;
                else
                    result = StartResult.PartialSuccess;
            }

            return result;
        }

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        public void Stop()
        {
            var servers = m_AppServers.ToArray();

            foreach (var server in servers)
            {
                server.Stop();

                //if (m_GlobalLog.IsInfoEnabled)
                //    m_GlobalLog.InfoFormat("The server instance {0} has been stopped!", server.Name);
            }

        }

    }
}
