using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wlo.Core;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC82 : AC
    {
        public override int ID { get { return 82; } }
        public override void ProcessPkt(ref Player p, RecvPacket r)
        {
            switch (r.B)
            {
                case 7: Recv7(ref p, r); break;// ADD MESSAGE
                case 8: Recv8(ref p, r); break;// TAB REQUEST GUILD  
                case 9: Recv9(ref p, r); break;// OPen message
                case 11: Recv11(ref p, r); break;// write MESSAGE
                default: Utilities.LogServices.Log("AC " + r.A + "," + r.B + " has not been coded"); break;
            }
        }
        void Recv7(ref Player p, RecvPacket r)
        {
            try
            {
                //p.CurGuild.AddMessage(p, r);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv8(ref Player p, RecvPacket r)
        {
            try
            {
                p.CurGuild.OpenTab(p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv9(ref Player p, RecvPacket r)
        {
            try
            {
                //p.CurGuild.OpenMessage(p,r);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv11(ref Player p, RecvPacket r)
        {
            try
            {
                p.CurGuild.OpenPainelWriteMessage(p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}