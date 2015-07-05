using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;
using Wlo.Core;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC20 : AC
    {
        public override int ID { get { return 20; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, ref p); break;
                case 6: Recv6(ref r, ref p); break;
                case 9: Recv9(ref r, ref p); break;
                case 8: Recv8(ref r, ref p); break;
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv8(ref Player p, ref RecvPacket r)
        {
            if (!p.CurrentMap.Teleport(TeleportType.Regular, ref p, (byte)r.Unpack16()))
            {
                SendPacket tmp = new SendPacket();
                tmp.Pack(new byte[] { 20, 8 });
                p.Send(tmp);
            }
        }
        void Recv1(ref Player p, ref RecvPacket r)
        {
            if (!p.CurrentMap.ProccessInteraction(r.Unpack8(), ref p))
            {
                SendPacket tmp = new SendPacket();
                tmp.Pack(new byte[] { 20, 8 });
                p.Send(tmp);
            }
        }
        void Recv6(ref Player p, ref RecvPacket r)
        {
            if (!p.ContinueInteraction())
            {
                SendPacket tmp = new SendPacket();
                tmp.Pack(new byte[] { 20, 8 });
                p.Send(tmp);

                switch (p.State)
                {
                    case PlayerState.InGame_Warping:
                        {
                            tmp = new SendPacket();
                            tmp.Pack(new byte[] { 5, 4 });
                            p.Send(tmp);
                        } break;
                    case PlayerState.InGame_Interacting:
                        {
                            p.object_interactingwith = null;
                        } break;
                }

                p.State = PlayerState.InGame_InMap;
            }
        }
        void Recv9(ref Player p, ref RecvPacket r)
        {

        }
    }
}