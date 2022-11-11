using ProjectCommon.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public abstract class ServerComponentBase : AfComponent
    {
        protected readonly ServerBase server;

        public ServerComponentBase(ServerBase server):base(server.ServerManagerContainer)
        {
            this.server = server;
        }

    }
}
