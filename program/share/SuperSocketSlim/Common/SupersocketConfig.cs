using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocketSlim.Common.Configuration
{
    ///https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
    public sealed class SupersocketConfig 
    {
        public List<ServerConfig> ServerList { get; set; }
    }

    public sealed class CommonConfig
    {
        public class ServerConfigSlim
        {
            public string IP { get; set; }
            public int Port { get; set; }
        }

        public List<NameVal> SQLConnectionList { get; set; }
        public ServerConfigSlim EurekaServer { get; set; }
    }

    public sealed class NameVal
    {
        public string Name { get; set; }
        public string Val { get; set; }
    }

    public sealed class ServerConfig
    {

        public string Name { get; set; }
        public string ServerType { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public ushort PortRangeMin { get; set; }
        public ushort PortRangeMax { get; set; }
        public int ListenBacklog { get; set; } = 5;
        public bool ClearIdleSession { get; set; }
        public int MaxConnectionNumber { get; set; }
        public int SendingQueueSize { get; set; }
        public int MaxRequestLength { get; set; }
        public int SendBufferSize { get; set; }
        public int ReceiveBufferSize { get; set; }
        public bool SyncSend { get; set; } = false;
        public int SendTimeOut { get; set; } = 500; //ms


        public int ClearIdleSessionInterval { get; set; } = 200; //second
        public int IdleSessionTimeOut { get; set; } = 10;
        public bool LogBasicSessionActivity { get; set; } = true;
    }


}
