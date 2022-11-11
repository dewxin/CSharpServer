using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocketSlim.Protocol
{
    /// <summary>
    /// Receive filter interface
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IReceiveFilter
    {

        void ParseAndTryExecuteCommand(ArraySegment<byte> buffer);

        void Initialize(IAppServer appServer, IAppSession session);

    }
}
