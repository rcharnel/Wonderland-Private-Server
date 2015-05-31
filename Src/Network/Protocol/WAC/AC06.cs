using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlo.Core;
using Game;

namespace Server.Network.WAC
{
    public class AC06 : WLOAC
    {
        public override int ID { get { return 06; } }

        public override void Process(Player p, SendPacket r)
        {
            switch (r.Unpack8())
            {
                case 1: Recv1(p, r); break;
                //case 2: Recv2(ref r, p); break;
                //case 3: Recv3(ref r, p); break;
                default: base.Process(p, r); break;
            }
        }
        void Recv1(Player p, SendPacket g)
        {
            byte direction = g.Unpack8();
            p.CurX = g.Unpack16();
            p.CurY = g.Unpack16();
            ushort unknown = g.Unpack16();
            SendPacket tmp = new SendPacket();
            tmp.Pack((byte)6);
            tmp.Pack((byte)1);
            tmp.Pack(p.CharID);
            tmp.Pack((byte)direction);
            tmp.Pack16(p.CurX);
            tmp.Pack16(p.CurY);
            tmp.SetHeader();
            p.CurMap.Broadcast(tmp, "Ex", p.CharID);
        }
    }
}
