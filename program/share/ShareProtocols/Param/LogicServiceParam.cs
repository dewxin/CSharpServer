using CommonRpc.RpcBase;
using FlatSharp.Attributes;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Param
{
    #region Gate to Logic

    [MessagePackObject()]
    public class LogicLoginData
    {
        [Key(2)]
        public uint SessionId { get; set; }

        [Key(3)]
        public int PlayerId { get; set; }

    }

    [MessagePackObject()]
    public class LogicLoginRet: RpcResult<LogicLoginRet>
    {
        [Key(1)]
        public uint GlobalSessionID { get; set; }

        [Key(2)]
        public EnumResultType Result { get; set; } = EnumResultType.Success;

    }

    #endregion


    #region Client to Logic
    [MessagePackObject]
    [RpcData(RpcDataOption.UseCompress)]
    public class GetPlayerInfoRet : RpcResult<GetPlayerInfoRet>
    {
        [Key(1)]
        public PlayerInfo playerInfo { get; set; }
    }
    #endregion

}
