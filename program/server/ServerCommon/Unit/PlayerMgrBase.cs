using ProtoBuf;
using ServerCommon.Sql;
using Protocol;
using SuperSocketSlim.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Unit
{
    public interface IPlayer
    {
        ILog Logger { get; }
        int PlayerId { get; }
        //gate server id + client peer id
        uint SessionId { get; }
        void Tick(double elapsed);
        //void Release();
        void OnCheckTime();
    }


    public class PlayerMgrBase<TPlayer> : ServerComponentBase
        where TPlayer : class, IPlayer
    {
        //serverId + clientPeerId
        private Dictionary<uint, TPlayer> sessionId2PlayerDict { get; set; } = new Dictionary<uint, TPlayer>();
        private Dictionary<int, TPlayer> playerId2PlayerDict { get; set; } = new Dictionary<int, TPlayer>();

        public MySqlManager MySqlManager => server.MySqlManager;

        public PlayerMgrBase(ServerBase server) : base(server)
        {
        }

        public ILog Logger
        {
            get { return server.Logger; }
        }

        public override void Tick(double elapsed)
        {
            foreach (var player in GetAllPlayers())
            {
                player.Tick(elapsed);
            }
        }

        public bool AddPlayer(TPlayer player)
        {
            if (playerId2PlayerDict.ContainsKey(player.PlayerId))
                return false;

            playerId2PlayerDict.Add(player.PlayerId, player);
            sessionId2PlayerDict.Add(player.SessionId, player);
            return true;
        }

        public ICollection<TPlayer> GetAllPlayers()
        {
            return playerId2PlayerDict.Values;
        }

        public bool HasPlayerByPlayerId(int playerId)
        {
            return playerId2PlayerDict.ContainsKey(playerId);
        }

        //public override void Init()
        //{
        //}

        //public override void Release()
        //{
        //    foreach (var player in GetAllPlayers())
        //    {
        //        player.Release();
        //    }

        //    sessionId2PlayerDict.Clear();
        //    playerId2PlayerDict.Clear();

        //}


        public TPlayer GetPlayerBySessionId(uint sessionId)
        {
            if(sessionId2PlayerDict.ContainsKey(sessionId))
                return sessionId2PlayerDict[sessionId];
            return null;
        }

        public TPlayer GetPlayerByPlayerId(int playerId)
        {
            if (playerId2PlayerDict.ContainsKey(playerId))
                return playerId2PlayerDict[playerId];
            return null;
        }

        public void RemovePlayerByPlayerId(int playerId)
        {
            if(playerId2PlayerDict.ContainsKey(playerId))
            {
                var player = playerId2PlayerDict[playerId];
                RemovePlayer(player);
            }
        }

        public void RemovePlayer(TPlayer player)
        {
            playerId2PlayerDict.Remove(player.PlayerId);
            sessionId2PlayerDict.Remove(player.SessionId);
        }

        //todo 上移到GameServerBase
        public ServerPeer GetGatePeerBySessionId(uint sessionId)
        {
            return server.GetGatePeerBySessionId(sessionId);
        }

        public ServerPeer GetPeerByServerId(ushort serverId)
        {
            return server.GetPeerByServerId(serverId);
        }

    }
}
