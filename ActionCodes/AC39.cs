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
    public class AC39 : AC
    {
        public override int ID { get { return 39; } }
        public override void ProcessPkt(ref Player p, RecvPacket r)
        {

            switch (r.B)
            {
                // case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref p, r); break;// request NEW MEMBER TO GUILD
                case 3: Recv3(ref p, r); break; // acept resquest guild
                case 4: Recv4(ref p, r); break; // Guild EMAIL
                case 6: Recv6(ref p, r); break;// leave guild
                case 7: Recv7(ref p, r); break; // Demiss member
                case 8: Recv8(ref p, r); break; // TAB MESSAGE
                case 9: Recv9(ref p, r); break; // edit rule
                case 11: Recv11(ref p, r); break;//Remove HOLD THE POST OF VIC ORG
                case 14: Recv14(ref p, r); break;//HOLD THE POST OF VICE ORGLEADER
                case 16: Recv16(ref p, r); break;//PERMISSION
                case 18: Recv18(ref p, r); break; // change insigna guild
                default: Utilities.LogServices.Log("AC " + r.A + "," + r.B + " has not been coded"); break;
            }
        }
        void Recv2(ref Player p, RecvPacket r)
        {
            try
            {
                uint m = r.Unpack32(); // get request member id               

                if (p.CurrentMap.Players.ContainsKey(m))
                {
                  //  p.CurrentMap.Players[m].GuildID = p.CurGuild.GuildID;

                    SendPacket s = new SendPacket();
                    s.Pack(new byte[] { 39, 3 });
                    s.Pack(p.UserID);
                    cGlobal.WLO_World.BroadcastTo(s, directTo: m);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv3(ref Player p, RecvPacket r)
        {
            try
            {
                uint m = r.Unpack32(); // get request member id
                if (p.CurrentMap.Players.ContainsKey(m))
                {                    
                    p.CurrentMap.Players[m].CurGuild.AddNewMemberGuild(p,m);
                }
                
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv4(ref Player p, RecvPacket r)
        {
            try
            {
                //uint dst = r.Unpack32(3); // get member id
                //string text = r.UnpackNChar(7);
                //p.CurGuild.GuilMail(p.UserID,dst, text);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv6(ref Player p, RecvPacket r)
        {
            try
            {
                p.CurGuild.LeaveGuild(ref p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv7(ref Player p, RecvPacket r)
        {
            uint target = r.Unpack32();
            try
            {
                if (p.CurGuild.Leader.ID == p.UserID)
                {
                    p.CurGuild.Dismiss(target, p.UserID);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv8(ref Player p, RecvPacket r)
        {
            try
            {
                
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv9(ref Player p, RecvPacket r)
        {
            try
            {               
                    if (p.CurGuild.Leader.ID == p.UserID)
                    {

                        p.CurGuild.Edit_Rule(r.UnpackStringN());
                    }                
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv11(ref Player p, RecvPacket r)
        {
            try
            {
                uint target = r.Unpack32();

                if (p.CurGuild.Leader.ID == p.UserID)
                {
                    p.CurGuild.RemoveHoldThePostOfViceOrgleader(p,target);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv14(ref Player p, RecvPacket r)
        {
            try
            {
                uint target = r.Unpack32();               

                if (p.CurGuild.Leader.ID == p.UserID)
                {
                    p.CurGuild.HoldThePostOfViceOrgleader(target, r.Unpack8());
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv16(ref Player p, RecvPacket r)
        {
            try
            {
                uint target = r.Unpack32();             

                if (p.CurGuild.Leader.ID == p.UserID) // holy leader have this permission
                {
                    p.CurGuild.ChangePermissionMember(target,r);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv18(ref Player p, RecvPacket r)
        {
            try
            {
                if (p.CurGuild.Leader.ID == p.UserID) // or vices + permition.
                {
                    p.CurGuild.ChangInsigneGuild(ref p,r);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }

        }
        

    }
}
