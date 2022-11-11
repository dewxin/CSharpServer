using CommonRpc.RpcBase;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Param
{
    [MessagePackObject]
    public partial class PeerIDResult : RpcResult<PeerIDResult>
    {
        [Key(1)]
        public ushort PeerID{ get; set; }
    }
}
