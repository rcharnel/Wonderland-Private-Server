using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Utilities;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC06:AC
    {
        public override int ID { get { return 06; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
                switch (p.B)
                {
                    case 1: Recv1(ref r, p); break;
                    case 2: Recv2(ref r, p); break;
                    case 3: Recv3(ref r, p); break;
                    default: LogServices.Log(p.A + "," + p.B + " Has not been coded"); break;
                }
        }
        void Recv1(ref Player p, Packet g)
        {
            if (g.Data.Count > 5)
            {
                byte direction = g.Data[2];
                p.X = g.Unpack16(3);
                p.Y = g.Unpack16(5);
                //WORD unknown = p->Unpack16(7);
                //p.Info.e = 0;
                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 6, 1 });
                tmp.Pack32(p.ID);
                tmp.Pack8(direction);
                tmp.Pack16(p.X);
                tmp.Pack16(p.Y);
                p.CurrentMap.Broadcast(tmp, p.ID);
            }
        }
        void Recv2(ref Player p, Packet r)
        {
        }
        void Recv3(ref Player p, Packet r)
        {
        }
    }
}
