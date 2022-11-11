using Autofac;
using Protocol;
using Newtonsoft.Json;
using ProjectCommon.Unit;
using ProtoBuf;
using ServerCommon.Unit;
using SqlDataCommon;
using SuperSocketSlim.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace GameServerBase.ServerLogic.Logic
{
    /// <summary>
    /// 玩家状态类型
    /// </summary>
    public enum ePlyearState
    {
        /// <summary>
        /// 登陆中
        /// </summary>
        Logining,
        /// <summary>
        /// 重登录
        /// </summary>
        ReLogining,
        /// <summary>
        /// 在线
        /// </summary>
        Online,
        /// <summary>
        /// 离线
        /// </summary>
        Offline,
        /// <summary>
        /// 游戏中
        /// </summary>
        Playing,
        /// <summary>
        /// 切换游戏
        /// </summary>
        ChangingGame,
        /// <summary>
        /// 退出游戏
        /// </summary>
        Logouting,
    }


    public class LogicPlayer : AfContainer, IPlayer
    {
        private readonly LogicPlayerManager playerManager;
        public SqlPlayerInfoExt SqlPlayerInfo { get; set; } = new SqlPlayerInfoExt();
        public ePlyearState PlayerState { get; set; } = ePlyearState.Offline;

        // = GateServerId + ClientPeerId
        public uint SessionId { get; set; }
        public SuperSocketSlim.Logging.ILog Logger => playerManager.Logger;
        bool IsInit { get; set; } = false;
        public int PlayerId { get { return SqlPlayerInfo.PlayerID; } }


        public LogicPlayer(LogicPlayerManager manager) : base(manager.AutofacContainer)
        {
            this.playerManager = manager;
        }

        public void OnCheckTime()
        {
           // throw new NotImplementedException();
        }

        public void Logout()
        {
            if (PlayerState == ePlyearState.Logouting)
                return;

            PlayerState = ePlyearState.Logouting;
            playerManager.RemovePlayerByPlayerId(PlayerId);

            SqlPlayerInfo.LastLogoutTime = DateTime.Now;
            //TimeSpan totaltime = DateTime.Now - getCacheTime(eTimesFlag.LoginTime);
            //if (totaltime.TotalSeconds > 0)
            //{
            //    sqlPlayerInfo.OnlineTime += (int)totaltime.TotalSeconds;
            //}


            //todo invoke PlayerComponent onLogout
            //todo save player data
        }


        public bool OnLogined()
        {
            bool result = false;
            if (PlayerState == ePlyearState.Logining || PlayerState == ePlyearState.ReLogining)
            {
                result = true;

                //addCachesMapValue((int)ePlayerFlag.LoginCount);
                //if (getCachesMapValue((int)ePlayerFlag.TempOnlineTime) < sqlPlayerInfo.OnlineTime)
                //{
                //    setCachesMapValue((int)ePlayerFlag.TempOnlineTime, sqlPlayerInfo.OnlineTime);
                //}
            }

            PlayerState = ePlyearState.Online;
            IsInit = true;

            return result;
            //if (PlayerState != ePlyearState.ReLogining)
            //    setCacheTime(eTimesFlag.LoginTime, DateTime.Now);
        }

    }
}
