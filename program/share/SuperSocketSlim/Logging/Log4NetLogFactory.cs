using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;

namespace SuperSocketSlim.Logging
{
    /// <summary>
    /// Log4NetLogFactory
    /// </summary>
    public class Log4NetLogFactory : LogFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        public Log4NetLogFactory()
            : this("log4net.config")
        {
           
        }


        public ILoggerRepository Repository { get; private set; }

        //public static ILoggerRepository Repository => LogManager.GetRepository(typeof(Log4NetLogFactory).Assembly);

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        /// <param name="log4netConfig">The log4net config.</param>
        public Log4NetLogFactory(string log4netConfig)
            : base(log4netConfig)
        {
            bool exist = LoggerManager.RepositorySelector.ExistsRepository(RepositoryName);
            if(!exist)
                Repository = LogManager.CreateRepository(RepositoryName);
            Repository = LogManager.GetRepository(RepositoryName);

            log4net.Config.XmlConfigurator.Configure(Repository, new FileInfo(ConfigFile));
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override ILog GetLog(string name)
        {
            return new Log4NetLog(LogManager.GetLogger(RepositoryName, name));
        }
    }
}
