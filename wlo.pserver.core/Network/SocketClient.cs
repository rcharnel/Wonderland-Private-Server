using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Network
{


    public class SocketClient : Client3
    {
        public SocketClient()
        {
        }

        public SocketClient(Socket sock = null)
            : base(sock)
        {
        }


        public override void SendPacket(RCLibrary.Core.Networking.IPacket p)
        {
            base.SendPacket(p);
        }
    }
}
