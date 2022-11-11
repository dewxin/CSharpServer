using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SuperSocketSlim
{
    public class ActiveConnectResult
    {
        public bool Result { get; set; }
        public IAppSession Session { get; set; }
    }

    public interface IActiveConnector
    {
        Task ActiveConnect(EndPoint targetEndPoint);

        Task ActiveConnect(EndPoint targetEndPoint, ConnectParam otherEndServerInfo);
    }
}
