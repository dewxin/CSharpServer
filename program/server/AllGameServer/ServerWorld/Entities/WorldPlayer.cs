using ProtoBuf;
using Protocol;
using Protocol.Param;
using ServerCommon.Unit;
using SuperSocketSlim.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerWorld.Entities
{

    public enum ePlayerState
    {
        None,
        Logined,
        Logouting,
        ChangeScene,
    }

    public class WorldPlayer :  IPlayer
    {
        public uint SessionId { get; set; }

        readonly WorldPlayerManager playerManager;
        public ushort LogicID { get; set; }
        public int PlayerId { get { return playerInfo.PlayerId; } }

        public PlayerInfo playerInfo;

        public ePlayerState State { get; set; }

        public WorldPlayerManager PlayerMgr { get { return playerManager; } }

        public SuperSocketSlim.Logging.ILog Logger => playerManager.Logger;

        public WorldPlayer(WorldPlayerManager mgr)
            //: base(mgr.AutofacContainer)
        {
            playerManager = mgr;
            State = ePlayerState.None;
        }


        //public override void Release()
        //{
        //    base.Release();

        //    playerManager.GetMySql().Delete<SqlLoginInfo>("PlayerID", PlayerID);
        //    //PacketPlayerDisconnect ppd = new PacketPlayerDisconnect();
        //    //ppd.GlobalSessionID = GlobalSessionID;
        //    //var gate = _playerMgr.BaseMgr.GetGatePeer(GlobalSessionID);
        //    //gate?.SendMsg(ppd);

        //    PacketOnPlayerLogoutResult msg = new PacketOnPlayerLogoutResult();
        //    msg.GlobalSessionID = GlobalSessionID;
        //    msg.PlayerID = PlayerID;
        //    var logic = playerManager.BaseMgr.GetServerPeer(LogicID);
        //    logic?.SendMsg(msg);

        //}


        //public bool OnChangeScene(short logicid)
        //{
        //    if (TempLogicID <= 0 || TempLogicID != logicid)
        //        return false;

        //    TempLogicID = 0;
        //    LogicID = logicid;
        //    State = ePlayerState.Logined;
        //    return true;
        //}

        public void OnLogin()
        {
            //CreateEntitys();
            State = ePlayerState.Logined;
        }

        //public void UpdateState(int gameid)
        //{
        //    string queryString = string.Format("update {0} tmp set tmp.LoginState = {1} where tmp.PlayerID = {2}", typeof(SqlLoginInfo).Name, gameid, PlayerID);
        //    _playerMgr.GetMySql().UpdateHQLAsync(queryString);
        //}

        //public bool CheckLogicID(short logicid)
        //{
        //    if (LogicID != logicid)
        //    {
        //        LogicID = logicid;
        //        return true;
        //    }
        //    return false;
        //}

        //void CreateEntitys()
        //{
        //    var entitys = playerManager.AutoContainer.Resolve<IEnumerable<WorldEntityBase>>(new TypedParameter(GetType(), this));
        //    foreach (var it in entitys)
        //    {
        //        CreateEntity(it);
        //    }
        //    Init();
        //}

        public void Logout()
        {
            State = ePlayerState.Logouting;
            playerManager.RemovePlayerByPlayerId(PlayerId);
        }

        //public void SendMsg2Client(IExtensible msg)
        //{
        //    playerManager.SendMsg2Client(SessionId, msg);
        //}

        //public void SendMsg2Gate(IExtensible msg)
        //{
        //    playerManager.GetGatePeerBySessionId(SessionId)?.SendPacket(msg);
        //}


        public void Tick(double elapsed)
        {
        }

        public void OnCheckTime()
        {
           // throw new NotImplementedException();
        }

        public void Release()
        {
            //throw new NotImplementedException();
        }


        //public FriendInfo ConvertProto()
        //{
        //    FriendInfo fi = new FriendInfo();
        //    fi.IsFriend = false;
        //    fi.HeadIcon = playerInfo.HeadIcon;
        //    fi.NickName = playerInfo.NickName;
        //    fi.PlayerID = playerInfo.PlayerID;
        //    fi.VIPLvl = playerInfo.VIPLvl;
        //    return fi;
        //}

    }
}
