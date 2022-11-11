using CommonRpc.Net;
using CommonRpc.Rpc;
using CommonRpc.RpcBase;
using SuperSocketSlim.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    public class ProtobufRequestInfo : RequestInfo<byte[]>, IProtoMsg
    {
        /// 协议id
        public ushort MsgId { get; set; }
        public ushort InvokeId { get; set; }
        public bool IsReply { get; set; }
        public bool IsForward { get; set; }
        public ForwardTarget FromTarget { get; set; }
        public ForwardTarget ToTarget { get; set; }
        public ushort ClientPeerId { get; set; }
        public Object Protobuf { get; set; }

        public ProtobufRequestInfo(MsgHeaderBody msgHeaderBody,byte[] body)
        {
            this.MsgId = msgHeaderBody.MsgId;
            this.InvokeId = msgHeaderBody.InvokeId;
            this.IsReply = msgHeaderBody.IsReply;
            this.IsForward = msgHeaderBody.IsForward;
            this.FromTarget = msgHeaderBody.FromTarget;
            this.ToTarget = msgHeaderBody.ToTarget;
            this.ClientPeerId = msgHeaderBody.ClientPeerId;

            Body = body;
        }

        public void SetData(byte[] data)
        {
            Body = data;
        }

        public MsgHeaderBody ToMsgHeaderBody()
        {
            var ret = new MsgHeaderBody(msgId: MsgId, invokeId: InvokeId, isReply: IsReply, clientPeerId:ClientPeerId);
            ret.SetMsgBody(Body);
            return ret;
        }
    }
}
