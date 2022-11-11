using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim.Logging;
using SuperSocketSlim.Protocol;

namespace SuperSocketSlim
{
    /// <summary>
    /// The interface for AppServer
    /// </summary>
    public interface IAppServer : ILoggerProvider
    {
        string Name { get; }

        ServerConfig ServerConfig { get; }

        bool Start();

        void Stop();

        bool Setup(IBootstrap bootstrap, ServerConfig config, CommonConfig commonConfig);

        DateTime StartedTime { get; }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession CreateAppSession(SocketSession socketSession);
        bool RegisterSession(IAppSession session);

        ILogFactory LogFactory { get; }

        SocketServer SocketServer { get; }

    }
}
