using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;

namespace PhoenixGameServer.Network.WAC
{
    public class AC20 : WLOAC
    {
        public override int ID { get { return 20; } }

        public override void Process(Game.Player r, Phoenix.Core.Networking.Packet p)
        {
            switch (p.Unpack8())
            {
                case 1: Recv1(r, p); break;
                case 6: Recv6(r, p); break;
                //case 9: Recv9(r, p); break;
                //case 8: Recv8(r, p); break;
                default: base.Process(r, p); break;
            }
        }
        void Recv8(Game.Player src, Phoenix.Core.Networking.Packet r)
        {
            //if (!p.CurrentMap.Teleport(TeleportType.Regular, ref p, (byte)r.Unpack16(2)))
            //{
            //    SendPacket tmp = new SendPacket();
            //    tmp.PackArray(new byte[] { 20, 8 });
            //    p.Send(tmp);
            //}
        }
        void Recv1(Game.Player src, Phoenix.Core.Networking.Packet r)
        {
            //if (!p.CurrentMap.ProccessInteraction(r.Unpack8(2), ref p))
            //{
            src.SendPacket(Packet.ConvertfromFormat<Packet>("bb", 20, 8));
            //}
        }
        void Recv6(Game.Player src, Phoenix.Core.Networking.Packet r)
        {
            if (!src.ContinueInteraction())
            {
                src.SendPacket(Packet.ConvertfromFormat<Packet>("bb", 20, 8));

                if (src.Flags.HasFlag(Game.PlayerFlag.Warping))
                {
                    src.Flags.Remove(Game.PlayerFlag.Warping);
                    src.SendPacket(Packet.ConvertfromFormat<Packet>("bb", 5, 4));
                }
                //case PlayerState.InGame_Interacting:
                //    {
                //        p.object_interactingwith = null;
                //    } break;
            }
            src.Flags.Add(Game.PlayerFlag.InMap);
        }
        void Recv9(Game.Player src, Phoenix.Core.Networking.Packet r)
        {

        }
    }
}
