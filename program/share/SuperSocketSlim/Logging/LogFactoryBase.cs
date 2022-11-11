﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SuperSocketSlim.Logging
{
    /// <summary>
    /// LogFactory Base class
    /// </summary>
    public abstract class LogFactoryBase : ILogFactory
    {
        /// <summary>
        /// Gets the config file file path.
        /// </summary>
        protected string ConfigFile { get; private set; }

        protected string RepositoryName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactoryBase"/> class.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        protected LogFactoryBase(string configFile)
        {
            
            if (Path.IsPathRooted(configFile))
            {
                ConfigFile = configFile;
                return;
            }

            if (Path.DirectorySeparatorChar != '\\')
            {
                configFile = Path.GetFileNameWithoutExtension(configFile) + ".unix" + Path.GetExtension(configFile);
            }

            
            if (log4net.GlobalContext.Properties["LogAppName"] == null)
            {
                log4net.GlobalContext.Properties["LogAppName"] = AppDomain.CurrentDomain.FriendlyName;                
            }

            RepositoryName = log4net.GlobalContext.Properties["LogAppName"].ToString();

            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                filePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                ConfigFile = configFile;
                return;
            }
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public abstract ILog GetLog(string name);
    }
}
