using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class SessionIdHelper
    {
        /// <summary>
        /// 获取gateid
        /// </summary>
        /// <param name="sessionid"></param>
        /// <returns></returns>
        public static ushort GetGateServerId(uint sessionid)
        {
            return (ushort)(sessionid >> 16);
        }

        /// <summary>
        /// 获取clientid
        /// </summary>
        /// <param name="sessionid"></param>
        /// <returns></returns>
        public static ushort GetClientPeerId(uint sessionid)
        {
            return (ushort)(sessionid & 0x0000ffff);
        }

        //全局ID  = GateServerId + ClientPeerId
        public static uint MakeSessionId(ushort gateServerId, ushort clientPeerId)
        {
            uint ret = (uint)(gateServerId << 16);
            return ret + clientPeerId;
        }
    }
}
