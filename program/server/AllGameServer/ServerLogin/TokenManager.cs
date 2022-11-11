using ServerCommon;
using SqlDataCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerBase.ServerLogin
{
    public class TokenManager : ServerComponentBase
    {
        private Dictionary<string,SqlPlayerInfo> token2PlayerInfoDict = new ();

        public TokenManager(ServerBase server) : base(server)
        {
        }

        public override void Tick(double elapsed)
        {
        }

        //TODO removed when required by world server or timer expires
        public string GetToken(SqlPlayerInfo playerInfo)
        {
            string token = Guid.NewGuid().ToString();

            token2PlayerInfoDict[token] = playerInfo;

            return token;
        }

        public SqlPlayerInfo? GetPlayerInfo(string token)
        {
            if(token2PlayerInfoDict.TryGetValue(token, out var playerInfo))
            {
                return playerInfo;
            }
            return null;
        }
    }
}
