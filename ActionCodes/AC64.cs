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
                case 1: Recv1(ref r, p); break;// create new objet tent
                case 2: Recv2(ref r, p); break;
                case 3: Recv3(ref r, p); break; // stop build
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player r, RecvPacket p)
        {
            try
            {
                r.Tent.Floors[1].Create_NewObject_Tent(r, p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv2(ref Player r, RecvPacket p)
        {
            try { }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv3(ref Player r, RecvPacket p)
        {
            try { }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}
