using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;

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
                case 6: Recv6(ref p, r); break;// leave guild
                case 7: Recv7(ref p, r); break; // Demiss member
                case 8: Recv8(ref p, r); break; // TAB MESSAGE
                case 9: Recv9(ref p, r); break; // edit rule
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
                uint m = r.Unpack32(2); // get request member id
                cGlobal.WLO_World.GetPlayer(m).GuildID = p.GuildID;

                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {39,3 });
                s.Pack32(p.UserID);
                cGlobal.WLO_World.BroadcastTo(s, directTo: m);

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv3(ref Player p, RecvPacket r)
        {
            try
            {
                uint m = r.Unpack32(2); // get request member id
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 8 });
                s.Pack32(p.UserID);
                s.Pack8(1);
                cGlobal.WLO_World.BroadcastTo(s, directTo: m);
                cGlobal.gGuild.GlobalGuild[p.GuildID].AddNewMemberGuild(ref p);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv6(ref Player p, RecvPacket r)
        {
            try
            {
                if (cGlobal.gGuild.GlobalGuild.ContainsKey(p.GuildID))
                { cGlobal.gGuild.GlobalGuild[p.GuildID].LeaveGuild(p.UserID); }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv7(ref Player p, RecvPacket r)
        {
            uint target = r.Unpack32(2);
            try
            {
                if (cGlobal.gGuild.GlobalGuild[p.GuildID].LeaderID == p.UserID)
                    {
                        cGlobal.gGuild.GlobalGuild[p.GuildID].Dismiss(target, p.UserID);
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
                if (cGlobal.gGuild.GlobalGuild.ContainsKey(p.GuildID))
                {
                    if (cGlobal.gGuild.GlobalGuild[p.GuildID].LeaderID == p.UserID)
                    {

                        cGlobal.gGuild.GlobalGuild[p.GuildID].Edit_Rule(r.UnpackNChar(2));
                    }

                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv14(ref Player p, RecvPacket r)
        {
            try
            {
                uint target = r.Unpack32(2);               

                if (cGlobal.gGuild.GlobalGuild[p.GuildID].LeaderID == p.UserID)
                {
                    cGlobal.gGuild.GlobalGuild[p.GuildID].HoldThePostOfViceOrgleader(target,r.Unpack8(6));
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv16(ref Player p, RecvPacket r)
        {
            try
            {
                uint target = r.Unpack32(2);
                byte a = r.Unpack8(8);
                byte b = r.Unpack8(10);
                byte c = r.Unpack8(12);
                byte d = r.Unpack8(14);
                byte e = r.Unpack8(16);

                if (cGlobal.gGuild.GlobalGuild[p.GuildID].LeaderID == p.UserID)
                {
                    cGlobal.gGuild.GlobalGuild[p.GuildID].ChangePermissionMember(p.UserID, target, a, b, c, d, e);
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv18(ref Player p, RecvPacket r)
        {
            try
            {
                if (cGlobal.gGuild.GlobalGuild.ContainsKey(p.GuildID))
                {
                    if (cGlobal.gGuild.GlobalGuild[p.GuildID].LeaderID == p.UserID)
                    {

                        cGlobal.gGuild.GlobalGuild[p.GuildID].ChangInsigneGuild(r);
                    }

                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }

        }
        

    }
}
