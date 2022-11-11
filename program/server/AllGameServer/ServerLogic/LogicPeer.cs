using Autofac;
using CommonRpc.Net;
using Protocol.Service;
using Protocol.Service.LogicService;
using ServerCommon;
using SqlDataCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogic
{
    public partial class LogicPeer : GamePeer, IComparable<LogicPeer>
    {
        public int PlayerCount { get; set; }

        public LogicPeer(ServerBase server)
        {
            PlayerCount = 0;
        }


        // 玩家多的排前面
        public int CompareTo(LogicPeer other)
        {
            if (other.PlayerCount > this.PlayerCount)
                return -1;
            else if (other.PlayerCount == this.PlayerCount)
                return 0;
            else
                return 1;
        }

    }
}
