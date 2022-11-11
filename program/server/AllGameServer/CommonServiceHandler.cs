using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using Newtonsoft.Json;
using Protocol;
using Protocol.Param;
using Protocol.Service;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase
{
    public class CommonServiceHandler : GameServiceHandler, IServerCommonService
    {


        public void ConnectServer(ServerConnectData packet)
        {

            var connectMgr = Server.ServerPeerManager;
            if (connectMgr.GetPeer(packet.ServerId) != null)
            {
                Server.Logger.Error($"{nameof(ConnectServer)} duplicate svrId: {packet.ServerId}, type: {packet.PeerType}");
                //TODO 会话终止后， 资源怎么回收
                SessionHolder.DisconnectAppError();
                return;
            }

            var newPeer = SessionHolder.UpdatePeer(packet.ServerId, packet.PeerType, connectMgr);
        }

        public void ServerNotify()
        {
            Server.Logger.Debug($"{nameof(ServerNotify)}");
        }

        public void NotifyOnlineServerInfoList(ServerListInfo packet)
        {
            Server.OnReceiveServerInfoAddedNotice(packet.ServerInfoList);

            Server.Logger.Debug($"{nameof(NotifyOnlineServerInfoList)} {JsonConvert.SerializeObject(packet)}");
        }

        public void NotifyOnlineServerAdded(ServerInfo newServer)
        {
            Server.OnReceiveServerInfoAddedNotice(new List<ServerInfo> { newServer});
        }

        public void NotifyOnlineServerRemoved(ServerInfo newServer)
        {
            Server.OnReceiveServerInfoRemovedNotice(new List<ServerInfo> { newServer });
        }
    }
}
