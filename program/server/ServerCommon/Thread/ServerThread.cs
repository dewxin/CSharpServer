using ProjectCommon.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class ServerThread : ThreadBase 
    {
        public ServerBase Server { get; private set; }

        public static ServerThread Instance { get; private set; }

        public ServerThread(ServerBase server)
        {
            this.Server = server;
            Instance= this;
        }

        protected override void DoWorkInThreadLoop(double elapsed)
        {
            Server.DoWorkInServerThreadLoop(elapsed);
        }

        protected override void OnError(string errorInfo, bool needNotify = false)
        {
            Server.Logger.ErrorFormat("ServerThread: {0}" , errorInfo);

            if (needNotify)
            {
                //var web = _server.FindEntity<HttpPostMgr>((short)eServerEntityType.HttpPostMgr);
                //if (web != null)
                //{
                //    web.NotifyWarning();
                //}
            }
        }
    }
}
