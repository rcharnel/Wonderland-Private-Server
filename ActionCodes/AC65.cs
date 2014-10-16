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
    public class AC65 : AC
    {
        public override int ID { get { return 65; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, p); break; // entra na tent
                case 2: Recv2(ref r, p); break; // close tent
                default: LogServices.Log(p.A + "," + p.B + " Has not been coded"); break;
            }
        }
        void Recv1(ref Player p, RecvPacket r)
        {
            try
            {
                p.Tent.(p.UserID, p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv2(ref Player p, RecvPacket r)
        {
            try
            {
                p.Tent.Close();
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}
