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
    public class AC20 : AC
    {
        public override int ID { get { return 20; } }
        public override void ProcessPkt(Player r, RecievePacket p)
        {
            switch (p.Unpack8())
            {
                case 1: Recv1(r, p); break;
                case 6: Recv6(r, p); break;
                case 9: Recv9(r, p); break;
                case 8: Recv8(r, p); break;
            }
        }
        void Recv8(Player p, RecievePacket r)
        {
            if (!p.CurMap.Teleport(TeleportType.Regular, p, (byte)r.Unpack16()))
                p.Send(Tools.FromFormat("bb", 20, 8));
        }
        void Recv1(Player p, RecievePacket r)
        {
            p.Send(Tools.FromFormat("bb", 20, 8));
            //if (!p.CurrentMap.ProccessInteraction(r.Unpack8(), ref p))
            //{
            //    SendPacket tmp = new SendPacket();
            //    tmp.Pack(new byte[] { 20, 8 });
            //    p.Send(tmp);
            //}
        }
        void Recv6(Player p, RecievePacket r)
        {
            if (!p.ContinueInteraction())
            {
                p.Send(Tools.FromFormat("bb", 20, 8));

                if (p.Flags.HasFlag(PlayerFlag.Warping))
                {
                    p.Flags.Add(PlayerFlag.InMap);
                    p.Send(Tools.FromFormat("bb", 5, 4));
                }

                //switch (p.State)
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
        void Recv9(Player p, RecievePacket r)
        {

        }
    }
}