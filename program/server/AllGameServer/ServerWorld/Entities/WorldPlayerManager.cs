using ServerCommon;
using ServerCommon.Sql;
using ServerCommon.Unit;
using SqlDataCommon;
using SuperSocketSlim.Logging;

namespace GameServerBase.ServerWorld.Entities
{
    public class WorldPlayerManager : PlayerMgrBase<WorldPlayer>
    {
        public WorldPlayerManager(ServerBase server) : base(server)
        {
        }

    }
}