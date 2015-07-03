using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace Wonderland_Private_Server.Src.Server
{
    public class LoginServer
    {
        List<LoginClient> ClientList;

        public LoginServer()
        {
            ClientList = new List<LoginClient>();
        }
    }
}
