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
    public class AC33:AC
    {
        public override int ID { get { return 33; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv_1(ref r, p); break;
                default: LogServices.Log(p.A + "," + p.B + " Has not been coded"); break;
            }
        }
        void Recv_1(ref Player p, RecvPacket r)
        {
            p.Settings.Set(r[1], r[2]);
        }
    }
}
