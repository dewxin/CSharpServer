using System;
using System.Collections.Generic;
using System.Text;
using SuperSocketSlim.Common;

namespace SuperSocketSlim.Protocol
{
    /// <summary>
    /// FixedHeaderReceiveFilter,
    /// it is the Receive filter base for the protocol which define fixed length header and the header contains the request body length,
    /// you can implement your own Receive filter for this kind protocol easily by inheriting this class 
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class FixedHeaderReceiveFilter<TRequestInfo> : IReceiveFilter
        where TRequestInfo : SessionRequestInfo
    {
        private readonly int headerSize;


        // msgBodySize < 0表示没有找到头部
        private int msgBodySize =-1;

        private LinkedList<ArraySegment<byte>> dataBufferList = new LinkedList<ArraySegment<byte>>();

        private IAppSession appSession;

        protected FixedHeaderReceiveFilter(int headerSize)
        {
            this.headerSize = headerSize;
        }

        public void Initialize(IAppServer appServer, IAppSession session)
        {
            appSession = session;
        }


        public void ParseAndTryExecuteCommand(ArraySegment<byte> buffer)
        {
            dataBufferList.AddLast(buffer);

            while(DataNeedProcess())
            {
                if(NeedParseHeader())
                {
                    TryParseHeader();
                }
                //Parse Body
                else
                {
                    var requestInfo = TryParseBody();
                    appSession.ExecuteCommand(requestInfo);
                }

            }

        }

        public bool NeedParseHeader()
        {
            return msgBodySize == -1;
        }

        private bool DataNeedProcess()
        {
            //如果没有找到头部，并且数据量是大于头部固定大小的。
            if (msgBodySize < 0 && StoredDataSize() >= headerSize)
                return true;
            //如果找到bodysize，并且
            if(msgBodySize >= 0 && StoredDataSize() >= msgBodySize)
                return true;

            return false;
        }

        private int StoredDataSize()
        {
            int size = 0;
            foreach(ArraySegment<byte> buffer in dataBufferList)
            {
                size += buffer.Count;
            }
            return size;
        }

        private bool TryParseHeader()
        {
            var headerArray =CopyStoredBufferIntoArray(headerSize);

            var newBuffer = new ArraySegment<byte>(headerArray);

            msgBodySize = ParseHeaderReturnBodySize(newBuffer);

            return true;
        }


        private TRequestInfo TryParseBody()
        {
            var msgBodyArray = CopyStoredBufferIntoArray(msgBodySize);

            msgBodySize = -1;
            return ResolveRequestInfo(msgBodyArray);
        }


        private byte[] CopyStoredBufferIntoArray(int arraySize)
        {
            if (arraySize == 0)
                return null;

            byte[] byteArray = new byte[arraySize];

            int offset = 0;
            while (true)
            {
                var arraySeg = dataBufferList.First.Value;
                dataBufferList.RemoveFirst();
                if (arraySeg.Count + offset > arraySize)
                {
                    int partialHeaderSize = arraySize - offset;
                    arraySeg.Slice(0, partialHeaderSize).CopyTo(byteArray, offset);
                    var leftSeg = arraySeg.Slice(partialHeaderSize, arraySeg.Count - partialHeaderSize);
                    dataBufferList.AddFirst(leftSeg);
                    break;
                }
                arraySeg.CopyTo(byteArray, offset);
                offset += arraySeg.Count;

                if (offset == arraySize)
                    break;
            }

            return byteArray;
        }




        protected abstract int ParseHeaderReturnBodySize(ArraySegment<byte> headerData);

        protected abstract TRequestInfo ResolveRequestInfo(byte[] bodyBuffer);


    }
}
