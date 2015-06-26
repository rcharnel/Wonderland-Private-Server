using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using Game;

namespace Server.Network.WAC
{
    public class AC65 :WLOAC
    {
        public override int ID { get { return 65; } }

        public override void Process(Player p, RecievePacket r)
        {
            switch (r.Unpack8())
            {
                case 1: Recv1(p, r); break;
                case 2: Recv2(p, r); break;
                default: base.Process(p, r); break;
            }
        }

        void Recv1(Player r, RecievePacket p)
        {
            //if(r.CurMap is GameMap)
            //(r.CurMap as GameMap).onEnterTent(p.Unpack32(), r);
        }
        void Recv2(Player r, RecievePacket p)
        {
            r.Inv.onItemCanceled(p.Unpack8());
        }
    }
}
