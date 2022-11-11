using SuperSocketSlim;
using SuperSocketSlim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class ServerBoostrap
    {
        IBootstrap bootstrap = null;

        public ServerBoostrap()
        {
            Console.CancelKeyPress += OnExit;
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        public void Start(params string[] configFile)
        {

            bootstrap =  DefaultBootstrap.CreateBootstrap(configFile);

            bootstrap.Init();

            var result = bootstrap.Start();

            Console.WriteLine("Start result: {0}!", result);

            if (result == StartResult.Failed)
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Press key 'q' to stop it!");

            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            Console.WriteLine();

            //Stop the appServer
            bootstrap.Stop();

            Console.WriteLine("The server was stopped!");
            //Console.ReadKey();
        }

        void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            if (bootstrap != null)
                bootstrap.Stop();
        }

        void OnExit(object sender, EventArgs e)
        {
            if (bootstrap != null)
                bootstrap.Stop();
        }
    }
}
