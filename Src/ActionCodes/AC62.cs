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
    public class AC062:AC
    {
        public override int ID { get { return 62; } }
        public override void ProcessPkt(ref Player p, RecvPacket r)
        {
                switch (r.B)
                {
                    case 1: Recv1(ref p, r); break;
                    case 3: Recv3(ref p, r); break;//rotate, move objects in tent.
                   
                    default: LogServices.Log(r.A + "," + r.B + " Has not been coded"); break;
                }
        }
        void Recv1(ref Player p, RecvPacket r)
        {
            try { }            
            
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv3(ref Player p, RecvPacket r)
        {
            try
            {
                //byte pos = r.Unpack8(2);
                //byte ax = r.Unpack8(4);
                //byte ay = r.Unpack8(8);
                //byte floor = r.Unpack8(12);
                //byte rotate = r.Unpack8(16);                
                //p.Tent.Floors[1].Rotate_Move_Object(p, pos,ax,ay,floor,rotate); }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}
