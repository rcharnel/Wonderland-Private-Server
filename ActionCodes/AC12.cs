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
    public class AC12:AC
    {
        public override int ID { get { return 12; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {

            switch (p.B)
            {
                case 1: Recv1(ref r, p); break;
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player p, Packet r)
        {
            try
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
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}
