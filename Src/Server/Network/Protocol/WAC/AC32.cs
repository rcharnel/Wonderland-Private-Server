using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixGameServer.Game;
using Phoenix.Core.Networking;
using DataFiles;

namespace PhoenixGameServer.Network.WAC
{
    public class AC32:WLOAC
    {
        public override int ID { get { return 32; } }

        public override void Process(Player p, Packet r)
        {
            switch (r.Unpack8())
            {
                case 1: Recv1(p, r); break;
                case 2: Recv2(p, r); break;
                default: base.Process(p, r); break;
            }
        }

        void Recv1(Player p, Packet r)
        {
            p.Emote = r.Unpack8();
            p.CurMap.Broadcast(Packet.ConvertfromFormat<Packet>("bbdb", 32, 1, p.CharID, p.Emote), "Ex", p.CharID);

        }
        void Recv2(Player p, Packet r)
        {
            p.Emote = r.Unpack8();
            p.CurMap.Broadcast(Packet.ConvertfromFormat<Packet>("bbdb", 32, 2, p.CharID, p.Emote), "Ex", p.CharID);
        }
    }
}
