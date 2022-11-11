// See https://aka.ms/new-console-template for more information

using ClientNet.Unit;
using CommonRpc;
using System;
using System.Threading;

namespace ClientNet
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        static void Run()
        {
            var client = NetClient.New();
            client.ConnectLocalServer();

            client.Logger.DebugDelegate += (str) => Console.WriteLine(str);
            client.Logger.ErrorDelegate += (str) => Console.WriteLine(str);
            //while (!client.GatePeer.PeerStub.Connected)
            //    Thread.Sleep(1);
            client.LoginServer("nagi001", "nagi001");

            while(true)
            {
                client.Tick(1);
                Thread.Sleep(0);
            }
        }
    }
}
