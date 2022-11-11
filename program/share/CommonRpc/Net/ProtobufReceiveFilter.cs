using CommonRpc.Net;
using SuperSocketSlim.Common;
using SuperSocketSlim.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.Net
{
    //协议解析类
    public class ProtobufReceiveFilter : FixedHeaderReceiveFilter<ProtobufRequestInfo>
    {

        public ProtobufReceiveFilter()
            : base(MsgHeaderBody.NetHeaderSize)
        {

        }

        private MsgHeaderBody msgHeaderBody;

        protected override int ParseHeaderReturnBodySize(ArraySegment<byte> headerData)
        {
            msgHeaderBody = MsgHeaderBody.Parse(headerData);

            return msgHeaderBody.BodySize;
        }

        protected override ProtobufRequestInfo ResolveRequestInfo(byte[] bodyBuffer)
        {
            //TODO 感觉应该在这里使用PB解析成Object,这样可以减少内存池的占用时间

            var msg = new ProtobufRequestInfo(msgHeaderBody, bodyBuffer);
            //msg.TickTime = SearchState.CurTick;
            return msg;
        }
    }

}
