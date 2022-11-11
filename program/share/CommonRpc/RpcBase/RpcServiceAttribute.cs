using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.RpcBase
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RpcServiceAttribute : Attribute
    {
        public ushort MinMsgId { get; set; }
        public ushort MaxMsgId { get; set; }

        //msgId 左闭右开
        public RpcServiceAttribute(ushort minMsgId, ushort maxMsgId)
        {
            MinMsgId = minMsgId;
            MaxMsgId = maxMsgId;

            if(MinMsgId > MaxMsgId)
            {
                throw new ArgumentException($"MinMsgId is greater than MaxMsgId");
            }
        }

        public bool Conflict(RpcServiceAttribute another)
        {
            // {me} {another}
            if (MaxMsgId <= another.MinMsgId)
                return false;

            // {another} {me}
            if (MinMsgId >= another.MaxMsgId)
                return false;

            return true;
        }

        public bool Include(ushort msgId)
        {
            return MinMsgId <= msgId && msgId < MaxMsgId;
        }
    }

}
