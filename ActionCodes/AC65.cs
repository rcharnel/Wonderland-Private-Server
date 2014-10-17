using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC65:AC
    {
        public override int ID { get { return 65; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref r, p); break;
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player r, RecvPacket p)
        {
            r.CurrentMap.onEnterTent(p.Unpack32(2), r);
        }
        void Recv2(ref Player r, RecvPacket p)
        {
            r.Inv.onItemCanceled(p.Unpack8(2));
        }
    }
}
