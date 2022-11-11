using Newtonsoft.Json;
using ProjectCommon.Unit;
using Protocol.Param;
using Protocol.Service;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerEureka
{
    public class ServerInfoManager
    {

        private EurekaServer Server { get; set; }
        private IdGenerator idGenerator = new IdGenerator();
        private Dictionary<ushort, ServerInfo> serverID2PeerDict = new();
        private Dictionary<ushort,ServerPeer> needNotifyPeerDict = new();

        public ServerInfoManager(EurekaServer eurekaServer)
        {
            Server = eurekaServer;
        }

        public ushort GetServerID()
        {
            return idGenerator.GetIdUShort();
        }

        public RegisterServerRet RegisterServerAndSubscribe(ServerRegisterData registerData)
        {

            var serverInfo = new ServerInfo();
            {
                serverInfo.ID = Server.CurrentPeer.PeerID;
                serverInfo.IP = registerData.ServerIP;
                serverInfo.Port = registerData.ServerPort;
                serverInfo.PeerType = registerData.PeerType;
                serverInfo.State = ServerState.Idle;
            }
            serverID2PeerDict.Add(serverInfo.ID, serverInfo);
            needNotifyPeerDict.Add(serverInfo.ID, Server.CurrentPeer as ServerPeer);

            NotifyServerAdded(serverInfo);

            return new RegisterServerRet() { ServerInfo = serverInfo, ServerInfoList= GetServerList()};
        }

        private void NotifyServerAdded(ServerInfo serverInfo)
        {
            foreach (var peer in needNotifyPeerDict.Values)
            {
                if (peer.PeerID == serverInfo.ID)
                    continue;

                peer.GetServiceProxy<IServerCommonService>().NotifyOnlineServerAdded(serverInfo);
            }
        }

        private void NotifyServerRemoved(ServerInfo serverInfo)
        {
            foreach (var peer in needNotifyPeerDict.Values)
            {
                peer.GetServiceProxy<IServerCommonService>().NotifyOnlineServerRemoved(serverInfo);
            }
        }


        public void ServerDisconnect(ushort peerId)
        {
            serverID2PeerDict.Remove(peerId);
            needNotifyPeerDict.Remove(peerId);

            NotifyServerRemoved(new ServerInfo { ID = peerId });
        }


        private List<ServerInfo> GetServerList()
        {
            return serverID2PeerDict.Values.ToList();
        }

    }
}
