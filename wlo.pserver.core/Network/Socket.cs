using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Net;
using System.Net.Sockets;



namespace Wonderland_Private_Server.Network
{
    public delegate void PacketRecv(RecvPacket r);
    public interface cSocket
    {
        IPAddress ClientIP { get; }
        int ClientPort { get; }
        bool isAlive { get; }
        TimeSpan Elapsed { get; }
        void Send(SendPacket pkt,bool queue = false);
        void Disconnect();
    }
}
