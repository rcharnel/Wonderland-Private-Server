using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Utilities;
using Wlo.Core;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC08:AC
    {
        public override int ID { get { return 8; } }

        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv_1(ref r,p); break;
                default: LogServices.Log(p.A+","+p.B+" Has not been coded"); break;
            }
        }
        void Recv_1(ref Player r, RecvPacket p)
        {
            //int max = p.Unpack8(3);
            //int ptr = 4;
            //for (int a = 0; a < max; a++)
            //{
            //    r.AddStat(p.Unpack8(ptr), (byte)p.Unpack32(ptr + 1)); ptr += 5;
            //}
        }
    }
}
