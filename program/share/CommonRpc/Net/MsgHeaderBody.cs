using CommonRpc.RpcBase;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommonRpc.Net
{
    [Flags]
    public enum HeaderBit:byte
    {
        None = 0,
        IsReply = 0b_1000_0000,
        IsForward = 0b_0100_0000,

        ForwardFromTarget = 0b_0011_1000,
        ForwardToTarget = 0b_0000_0111,
        ClearTargetMask = 0b_1100_0000,
    }

    //todo  PB序列化相关的类有点多 是不是可以合并几个
    public class MsgHeaderBody
    {

        public MsgHeaderBody(ushort msgId = 0, ushort invokeId = 0, bool isReply = false, ushort clientPeerId = 0)
        {
            MsgId = msgId;
            InvokeId = invokeId;
            if (isReply)
            {
                Header = (byte)(Header | (byte)HeaderBit.IsReply);
            }

            var forwardAttr = ProtocolTool.GetForwardAttrByMsgId(msgId);
            if (forwardAttr!=null)
            {
                Header = (byte)(Header | (byte)HeaderBit.IsForward);
                FromTarget = forwardAttr.FromTarget;
                ToTarget = forwardAttr.ToTarget;

                if(isReply)
                {
                    Header = (byte)(Header & (byte)HeaderBit.ClearTargetMask);
                    FromTarget = forwardAttr.ToTarget;
                    ToTarget = forwardAttr.FromTarget;
                }
            }

            ClientPeerId = clientPeerId;
        }

        private MsgHeaderBody(ArraySegment<byte> headerData)
        {
            ConvertBytesToHeader(headerData);
        }

        public static MsgHeaderBody Parse(ArraySegment<byte> headerData)
        {
            return new MsgHeaderBody(headerData);
        }


        /// 头长度 todo 可以用stream.Length代替这样就不用写死了
        /// 头部优化一下 比如用一个字节里面8个比特位表示有无InvokeId有无ReplyId
        public const int NetHeaderSize = sizeof(ushort) * 5;

        /// <see cref="HeaderBit"/>
        private byte Header { get; set; } = 0;
        public ushort InvokeId { get; private set; } = 0;
        public ushort MsgId { get; private set; } = 0;
        public ushort ClientPeerId { get; private set; } = 0;
        public ushort BodySize { get; private set; } = 0;

        public bool IsReply => (Header & (byte)HeaderBit.IsReply) == (byte)HeaderBit.IsReply;
        public bool IsForward => (Header & (byte)HeaderBit.IsForward) == (byte)HeaderBit.IsForward;

        public ForwardTarget FromTarget 
        {
            get
            {
                var fromBits = Header & (byte)HeaderBit.ForwardFromTarget;
                fromBits = fromBits >> 3;
                return (ForwardTarget)fromBits;
            }
            set
            {
                var val = (byte)value << 3;
                Header |= (byte)val;
            }
        }

        public ForwardTarget ToTarget
        {
            get
            {
                var toBits = Header & (byte)HeaderBit.ForwardToTarget;
                return (ForwardTarget)toBits;
            }
            set
            {
                Header |= (byte)value;
            }
        }

        private ArraySegment<byte> _Body;
        public ArraySegment<byte> Body 
        {
            get => _Body;
            set
            {
                _Body = value;
                checked
                {
                    BodySize = (ushort)Body.Count;
                }
            }
        }

        public void SetMsgBody<T>(T msg)
            where T:class
        {
            var bodyByte = SerializerHelper.Serialize(msg);
            Body = bodyByte;
        }

        public void SetMsgBody(byte[] msg)
        {
            Body = new ArraySegment<byte>(msg);
        }


        public byte[] ConvertHeaderToBytes()
        {
            byte[] b = new byte[NetHeaderSize];
            //todo 这里是不是没考虑网络字节序 大小端的问题
            BitConverter.GetBytes(Header).CopyTo(b, 0);
            BitConverter.GetBytes(InvokeId).CopyTo(b, 1);
            BitConverter.GetBytes(MsgId).CopyTo(b, 3);
            BitConverter.GetBytes(ClientPeerId).CopyTo(b, 5);
            BitConverter.GetBytes(BodySize).CopyTo(b, 7);

            return b;
        }

        public void ConvertBytesToHeader(ArraySegment<byte> headerData)
        {
            Header = headerData.Array[headerData.Offset];
            InvokeId = headerData.Array[headerData.Offset + 1];
            MsgId = BitConverter.ToUInt16(headerData.Array, headerData.Offset + 3);
            ClientPeerId = BitConverter.ToUInt16(headerData.Array, headerData.Offset+5);
            BodySize = BitConverter.ToUInt16(headerData.Array, headerData.Offset+7);
        }


        public List<ArraySegment<byte>> GetData()
        {
            List<ArraySegment<byte>> byteList = new List<ArraySegment<byte>>();
            byteList.Add(new ArraySegment<byte>(ConvertHeaderToBytes()));
            if(BodySize > 0)
                byteList.Add(Body);

            return byteList;
        }

    }
}