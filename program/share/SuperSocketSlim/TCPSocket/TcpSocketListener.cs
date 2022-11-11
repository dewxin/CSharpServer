using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocketSlim.Common.Configuration;
using SuperSocketSlim;
using SuperSocketSlim.Logging;
using System.Net.NetworkInformation;

namespace SuperSocketSlim
{

    public delegate void NewClientAcceptHandler(TcpSocketListener listener, Socket client);

    public class TcpSocketListener 
    {
        public event NewClientAcceptHandler NewClientAcceptedAsyncHandler;


        private Socket listenSocket;

        public virtual IPEndPoint Start(ServerConfig config)
        {
            int port;
            if (config.Port > 0)
                port = config.Port;
            else if (config.PortRangeMax > config.PortRangeMin && config.PortRangeMin > 0)
                port = GetUnusedPort(config.PortRangeMin, config.PortRangeMax);
            else
                port = GetUnusedPort(2000, 65500);

            IPEndPoint endPoint = new IPEndPoint(ParseIPAddress(config.IP), port);

            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(endPoint);
            listenSocket.Listen(config.ListenBacklog);

            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            SocketAcceptAsync(listenSocket);

            return endPoint;

        }

        public int GetUnusedPort(int rangeMin, int rangeMax)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            while(true)
            {
                var port = random.Next(rangeMin, rangeMax);

                if (!PortIsUsed(port))
                    return port;
            }
        }

        private bool PortIsUsed(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation tcpInfo in tcpConnInfoArray)
            {
                if (tcpInfo.LocalEndPoint.Port == port)
                    return true;
            }

            return false;
        }


        private IPAddress ParseIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || "Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.Any;
            else if ("IPv6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.IPv6Any;
            else
                return IPAddress.Parse(ip);
        }

        private void SocketAcceptAsync(Socket listenSocket)
        {
            var task = listenSocket.AcceptAsync();

            task.ContinueWith(t =>
            {
                try
                {
                    NewClientAcceptedAsyncHandler?.Invoke(this, t.Result);
                }
                finally
                {
                    SocketAcceptAsync(listenSocket);
                }

            });

        }

        public virtual void Stop()
        {
            if (listenSocket == null)
                return;

            lock (this)
            {
                if (listenSocket == null)
                    return;

                try
                {
                    listenSocket.Close();
                }
                finally
                {
                    listenSocket = null;
                }
            }

        }
    }
}
