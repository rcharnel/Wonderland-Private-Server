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
    public class AC85 : AC
    {
        public override int ID { get { return 85; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref r, p); break;
                case 3: Recv3(ref r, p); break;
                case 6: Recv6(ref r, p); break;
                case 10: Recv10(ref r, p); break;//chek members
                case 11: Recv11(ref r, p); break;//chek membersTabs
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player p, RecvPacket r)
        {
            try
            {
                cGlobal.gInstance.Send81_1(ref p);

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv2(ref Player p, RecvPacket r)
        {
            try
            {
                switch (r.Unpack8(2))
                {

                    case 1:
                        cGlobal.gInstance.Send81_1(ref p);
                        break;
                }

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv3(ref Player p, RecvPacket r)
        {
           // int cc = r.Unpack8(4);
            //string tt = "";
            string str = r.UnpackNChar(5);
            int tmp = r.Unpack16(2);
            try
            {
                //if (cc > 0)
                //{
                //    tt = r.Data.Skip(5).Take(cc).ToString();
                //}
                cGlobal.gInstance.CreaterInstance(ref p, tmp , str);

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv6(ref Player p, RecvPacket r)
        {            
            
            try
            {               
                cGlobal.gInstance.ExitInstancia(ref p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv10(ref Player p, RecvPacket r)
        {
            try
            {
                if(p.CurInstance != 0 )
                cGlobal.gInstance.CheckMembers(ref p,1);

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv11(ref Player p, RecvPacket r)
        {
            byte tmp = r.Unpack8(2);
            try
            {
                switch(tmp)
                {
                    case 1: // confirm my exit
                        p.CurInstance = 0;
                        break;
                    default://check tabs
                        cGlobal.gInstance.CheckMembers(ref p, tmp);
                        break;

                }
                    

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}