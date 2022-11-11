using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocketSlim.Common;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim.Logging;
using SuperSocketSlim.Protocol;

namespace SuperSocketSlim
{
    public interface IAppSession
    {
        /// <summary>
        /// Gets the session ID.
        /// </summary>
        ushort SessionID { get; }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        ServerConfig Config { get; }

        /// <summary>
        /// Gets the app server.
        /// </summary>
        IAppServer AppServer { get; }
        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        SocketSession SocketSession { get; }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets or sets the last active time of the session.
        /// </summary>
        /// <value>
        /// The last active time.
        /// </value>
        DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Closes this session.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        void Close(CloseReason reason);

        bool Connected { get; }

        /// <summary>
        /// If Persistent, the connection will not time out
        /// </summary>
        bool Persistent { get; set; }

        ILog Logger { get; }

        /// <returns>return offset delta of next receiving buffer</returns>
        void ProcessRequest(ArraySegment<byte> buffer);

        void Initialize(IAppServer server, SocketSession socketSession);

        Action<CloseReason> OnSessionClosed { get; set; }
        Action OnSessionStarted { get; set; }

        bool ExecuteCommand(SessionRequestInfo requestInfo);
    }

    /// <summary>
    /// The basic interface for appSession
    /// </summary>
    public interface IAppSession<TRequestInfo> :IAppSession
        where TRequestInfo : class
    {
        event Action<TRequestInfo> OnExecuteCommand;
    }

}
