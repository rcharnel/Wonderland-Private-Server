using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;
using PhoenixGameServer.Game;

namespace PhoenixGameServer.Network.WAC
{
    public class AC23:WLOAC
    {
        public override int ID { get { return 23; } }

        public override void Process(Player p, Packet r)
        {
            switch (r.Unpack8())
            {
                // case 1: Recv1(ref r, p); break;
                case 2: Recv2(p, r); break;
                case 3: Recv3(p, r); break;
                default: base.Process(p, r); break;
            }
        }

        void Recv2(Player p, Packet r)
        {
                byte pos = r.Unpack8();
            if(p.CurMap is GameMap)
                (p.CurMap as GameMap).onItemPickup(p, pos);
        }
        void Recv3(Player p, Packet r)
        {
            byte pos = r.Unpack8();
            byte qnt = r.Unpack8();
            byte ukn = r.Unpack8();



            if (p.Inv[pos].ItemID > 0)
            {

                if (p.Inv[pos].Dropable && p.CurMap is GameMap)
                    (p.CurMap as GameMap).onItemDrop(p, pos, qnt);
                else//ask to destroy
                    p.SendPacket(Packet.ConvertfromFormat<Packet>("bbbbWb", 23, 212, 255, pos, p.Inv[pos].ItemID, qnt));
            }
        }
    }
}
