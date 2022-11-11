using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocketSlim
{
    public class ConnectParam
    {
        public ushort PeerID { get; set; }
        public string PeerType { get; set; }

        public Action<IAppSession> CallbackOnStarted { get; set; }
    }

    public class ActiveConnector : IActiveConnector
    {
        public event Func<Socket, ConnectParam, IAppSession> ProcessNewClientEvent;

        public static ActiveConnector New(Func<Socket, ConnectParam, IAppSession> processNewClientFunc)
        {
            return new ActiveConnector(processNewClientFunc);
        }

        private ActiveConnector(Func<Socket, ConnectParam, IAppSession> funcProcessClient)
        {
            System.Diagnostics.Debug.Assert(funcProcessClient != null);
            ProcessNewClientEvent += funcProcessClient;
        }

        public Task ActiveConnect(EndPoint targetEndPoint)
        {
            return ActiveConnect(targetEndPoint, null);
        }

        public Task ActiveConnect(EndPoint targetEndPoint, ConnectParam connectParam)
        {
            var socket = new Socket(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var task = socket.ConnectAsync(targetEndPoint);
            
            task.ContinueWith((_) => { 

                var appsesion = ProcessNewClientEvent(socket, connectParam);

            });

            return task;
        }


    }
}
