using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using Game;

namespace Network.ActionCodes
{
    public class AC06:AC
    {
        public override int ID { get { return 06; } }
        public override void ProcessPkt(Player r, RecievePacket p)
        {
                switch (p.Unpack8())
                {
                    case 1: Recv1(r, p); break;
                    //case 2: Recv2(r, p); break;
                    //case 3: Recv3(r, p); break;
                }
        }
        void Recv1(Player p, RecievePacket g)
        {
            if (g.Count > 5)
            {
                byte direction = g.Unpack8();
                p.CurX = g.Unpack16();
                p.CurY = g.Unpack16();
                //WORD unknown = p->Unpack16(7);
                //p.Info.e = 0;
                p.CurMap.Broadcast(Tools.FromFormat("bbdbww", 6, 1, p.CharID, direction, p.CurX, p.CurY));
            }
        }
        void Recv2(Player p, RecievePacket r)
        {
        }
        void Recv3(Player p, RecievePacket r)
        {
        }
    }
}
