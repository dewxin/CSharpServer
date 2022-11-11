using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocketSlim.Protocol
{
    public class SessionRequestInfo
    {
        public ushort SessionID { get; set; }
    }


    public class RequestInfo<TRequestBody>: SessionRequestInfo
        where TRequestBody : class
    {
        public TRequestBody Body { get; set; }

    }

    public class RequestInfo<TRequestHeader, TRequestBody> : RequestInfo<TRequestBody>
        where TRequestBody: class
    {
        public TRequestHeader Header { get; set; }

        public RequestInfo(TRequestHeader header, TRequestBody body)
        {
            Header = header;
            Body = body;
        }

    }
}
