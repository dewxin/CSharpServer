using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNet.Unit
{

    public class ClientInfo
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class PlayerInfo
    {

        public int PlayerId { get; set; }

        public string NickName { get; set; }
        public long Gold { get; set; }

     
    }
}
