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
    public class AC32 : AC
    {
        public override int ID { get { return 32; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref r, p); break;
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player p, RecvPacket r)
        {
            try
            {
                p.Emote = r.Unpack8(2);
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 32, 1 });
                    s.Pack32(p.UserID);
                s.Pack8(r.Unpack8(2));
                p.CurrentMap.Broadcast(s, p.ID);

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv2(ref Player p, RecvPacket r)
        {
            try
            {
                p.Emote = r.Unpack8(2);
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 32, 2 });
                s.Pack32(p.UserID);
                s.Pack8(r.Unpack8(2));
                p.CurrentMap.Broadcast(s, p.ID);
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }

}