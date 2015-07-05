using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using System.Net.Sockets;
using System.Net;

namespace Network
{
    class LoginClient : ConcurrentDictionary<Client3, Player>
    {
        public IPAddress SrcIP { get; private set; }

        public Player AddSock(Client3 src)
        {
            if (Count == 0) SrcIP = src.IPAddr;

            src.onConnectionLost = new Action(() => RemSock(src));
            Player tmp = new Player(src,cGlobal.ItemDatManager);
            if (TryAdd(src, tmp))
                return tmp;
            else
                return null;
        }

        void RemSock(Client3 src)
        {
            Player tmp;
            TryRemove(src, out tmp);
            DebugSystem.Write("Client " + src.SockAddress() + " has disconnected");
        }
        public void TerminateAll()
        {

        }
    }
}
