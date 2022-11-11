using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocketSlim;
using SuperSocketSlim.Logging;
using System.Net;

namespace SuperSocketSlim
{
    /// <summary>
    /// The bootstrap start result
    /// </summary>
    public enum StartResult
    {
        /// <summary>
        /// No appserver has been set in the bootstrap, so nothing was started
        /// </summary>
        None,
        /// <summary>
        /// All appserver instances were started successfully
        /// </summary>
        Success,
        /// <summary>
        /// Some appserver instances were started successfully, but some of them failed
        /// </summary>
        PartialSuccess,
        /// <summary>
        /// All appserver instances failed to start
        /// </summary>
        Failed
    }

    /// <summary>
    /// SuperSocket bootstrap
    /// </summary>
    public interface IBootstrap
    {
        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        IEnumerable<IAppServer> AppServers { get; }

        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        StartResult Start();


        bool Init();

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        void Stop();

    }

}
