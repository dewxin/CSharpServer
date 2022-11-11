using Autofac;
using GameServerBase;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameServerBase.ServerWorld.Entities;
using Protocol.Service.WorldService;

namespace GameServerBase.ServerWorld
{
    internal class WorldServer : GameServer<WorldPeer, I_WorldService>
    {
        public WorldPlayerManager PlayerManager => GetManager<WorldPlayerManager>();
        public WorldServer()
        {
        }


        protected override void Tick(double elapsed)
        {
        }

    }
}
