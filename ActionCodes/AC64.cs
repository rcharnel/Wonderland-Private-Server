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
    public class AC64 : AC
    {
        public override int ID { get { return 64; } }
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
            r.Tent.Floors[1].Create_NewObject_Tent(r, p);
        }
        void Recv2(ref Player r, RecvPacket p)
        {
            
        }
    }
}
