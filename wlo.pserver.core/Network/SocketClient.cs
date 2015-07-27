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
            p.SetHeader();
            p.Encode();
            base.SendPacket(p);
        }
    }
}
