using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using Network.ActionCodes;
using Game;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC12:AC
    {
        public override int ID { get { return 12; } }

        public override void ProcessPkt(Player r, RecievePacket p)
        {

            switch (p.B)
            {
                case 1: Recv1(r, p); break;
            }
        }
        void Recv1(Player p, RecievePacket r)
        {
            try
            {
                if (!p.ContinueInteraction())
                {
                    p.Send(Tools.FromFormat("bb", 20, 8));

                    if (p.Flags.HasFlag(PlayerFlag.Warping))
                    {
                        p.Flags.Add(PlayerFlag.InMap);
                        p.Send(Tools.FromFormat("bb", 5, 4));
                    }
                    
                    //switch (p.Flags.)
                    //{
                    //    case PlayerState.InGame_Warping:
                    //        {
                    //            tmp = new SendPacket();
                    //            tmp.Pack(new byte[] { 5, 4 });
                    //            p.Send(tmp);
                    //        } break;
                    //    case PlayerState.InGame_Interacting:
                    //        {
                    //            p.object_interactingwith = null;
                    //        } break;
                    //}
                    
                }
            }
            catch (Exception t) {DebugSystem.Write(new ExceptionData(t)); }
        }
    }
}
