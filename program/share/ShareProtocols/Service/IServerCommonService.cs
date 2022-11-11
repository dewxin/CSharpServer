using CommonRpc.RpcBase;
using Protocol.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Service
{
    //为了避免隐藏bug, 不要使用[0-100]的msgId
    [RpcService(100,500)]
    public interface IServerCommonService
    {
        void ConnectServer(ServerConnectData data);

        void NotifyOnlineServerInfoList(ServerListInfo notifyServersData);

        void NotifyOnlineServerAdded(ServerInfo newServer);
        void NotifyOnlineServerRemoved(ServerInfo newServer);

        void ServerNotify();

    }
}
