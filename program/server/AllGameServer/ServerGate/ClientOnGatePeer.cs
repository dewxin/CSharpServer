using Protocol;
using ProjectCommon.Unit;
using ServerCommon;
using ServerCommon.Net;
using ServerCommon.Timer;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonRpc.Net;
using SqlDataCommon;
using Protocol.Param;
using Protocol.Service.LogicService;
using Protocol.Service.WorldService;

namespace GameServerBase.ServerGate
{
    public class ClientOnGatePeer : ClientPeer
    {

        //该玩家所在 逻辑服务器Id
        public ushort LogicServerId { get; set; }

        public SqlLoginInfo SqlLoginInfo { get; set; }
        public PlayerInfo PlayerInfo { get; set; }

        // 全局ID  = GateServerId + ClientPeerId
        public uint SessionId { get; private set; }
        protected override GateServer Server => base.Server as GateServer;


        public ClientOnGatePeer()
        {
            LogicServerId = 0;
        }

        public override void OnPeerStarted()
        {
            //todo 重构去掉 对base的调用
            base.OnPeerStarted();

            SessionId = SessionIdHelper.MakeSessionId(Server.ServerInfo.ID, PeerID);
        }

        public override void OnPeerClosed()
        {
            //todo 命名需要一种规范，规定这个函数需不需要在派生的override函数中 调用base函数
            base.OnPeerClosed();

            PacketPlayerDisconnect msg = new PacketPlayerDisconnect();
            msg.GlobalSessionID = SessionId;
            GetLogicPeer()?.GetServiceProxy<IGate2Logic>().PlayerDisconnect(msg);
            GetWorldPeer()?.GetServiceProxy<IGate2World>().PlayerDisconnect(msg);

            //DecreaseLoigcPeerPlayerCount();
        }

        //public void DecreaseLoigcPeerPlayerCount()
        //{
        //    var logic = GetLogicPeer();
        //    if (logic != null)
        //        logic.PlayerCount--;
        //}

        //public void IncreaseLogicPeerPlayerCount()
        //{
        //    var logic = GetLogicPeer();
        //    if (logic != null)
        //        logic.PlayerCount++;
        //}

        //public void ChangeLogic(ushort newId)
        //{
        //    DecreaseLoigcPeerPlayerCount();
        //    LogicServerId = newId;
        //    IncreaseLogicPeerPlayerCount();
        //}

        public override ServerLogic.LogicPeer GetLogicPeer()
        {
            if (LogicServerId == 0)
                AllocateLogicServer();
            return Server.ServerPeerManager.GetPeer(LogicServerId) as ServerLogic.LogicPeer;
        }

        public override ServerWorld.WorldPeer GetWorldPeer()
        {
            return Server.ServerPeerManager.GetFirstPeerByType<ServerWorld.WorldPeer>();
        }


        // 分配逻辑服务器
        public void AllocateLogicServer()
        {
            if (LogicServerId != 0 && GetLogicPeer() == null)
                LogicServerId = 0;

            if (LogicServerId == 0)
            {
                var list = Server.ServerPeerManager.GetPeersByType<ServerLogic.LogicPeer>().Select(e => (e as ServerLogic.LogicPeer));
                if (list == null || list.Count() <= 0)
                    return;

                ServerLogic.LogicPeer serverPeer = list.Min();

                if (serverPeer != null)
                {
                    LogicServerId = serverPeer.PeerID;
                }
            }
        }
    }
}
