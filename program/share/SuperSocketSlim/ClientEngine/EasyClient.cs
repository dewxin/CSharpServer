using SuperSocketSlim.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocketSlim.ClientEngine
{
    public class EasyClient<TRequest>
        where TRequest : SessionRequestInfo 
    {
        private IReceiveFilter receiveFilter;

        private AppSession<TRequest> appSession;

        public event Action<TRequest> HandlePackage = delegate{};
        public event Action<object, EventArgs> OnConnected = delegate{};
        public event Action<SocketSession, CloseReason> OnClosed = delegate{};



        public void Initialize(IReceiveFilter receiveFilter)
        {
            this.receiveFilter = receiveFilter;

        }

        public void Send(List<ArraySegment<byte>> data)
        {
            appSession.TrySend(data);
        }

        public void Connect(EndPoint remoteEndPoint)
        {
            var connector = ActiveConnector.New(ProcessNewClient);
            var task = connector.ActiveConnect(remoteEndPoint);
            task.Wait();
        }

        private IAppSession ProcessNewClient(Socket client, object otherEndServerInfo)
        {
            SocketSession socketSession = new SocketSession(client);

            appSession = new AppSession<TRequest>(receiveFilter);

            appSession.Initialize(null, socketSession);

            OnConnected(client, null);

            appSession.OnExecuteCommand += HandlePackage;
            socketSession.OnClosedHandler += OnClosed;

            //Console.WriteLine("{0} AsyncRun peerid:",DateTime.Now, session.SessionID);
            //AppServer.AsyncRun(() => socketSession.Start());
            socketSession.Start();

            return appSession;

        }


        public void Close()
        {
            appSession.Close();
        }
    }
}
