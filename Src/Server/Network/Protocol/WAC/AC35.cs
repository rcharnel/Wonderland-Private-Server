using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;
using PhoenixGameServer.Game;
using Phoenix.Core;
using PhoenixGameServer;

namespace PhoenixGameServer.Network.WAC
{
    public class AC35 : WLOAC
    {
        public override int ID { get { return 35; } }

        public override void Process(Player p, Packet r)
        {
            switch (r.Unpack8())
            {
                case 2: Recv2(p, r); break;
                default: base.Process(p, r); break;
            }
        }

        async void Recv2(Player p, Packet e)
        {
            byte slot = e.Unpack8();
            e.m_nUnpackIndex = 18;
            string pw = "";// e.UnpackChar(14);

            PacketBuilder tmp = new PacketBuilder();
            tmp.Begin(false);

            if (p.UserAcc.Cipher == (pw = e.UnpackString()))
            {
                cGlobal.gGameDataBase.DeleteCharacter((slot == 1) ? p.UserAcc.Character1ID : p.UserAcc.Character2ID);
                p.UserAcc.Character1ID = (slot == 1) ? 0 : p.UserAcc.Character1ID;
                p.UserAcc.Character2ID = (slot == 2) ? 0 : p.UserAcc.Character2ID;
                p.UserAcc.Cipher = (p.UserAcc.Character1ID == 0 && p.UserAcc.Character2ID == 0) ? "" : p.UserAcc.Cipher;
                cGlobal.gUserDataBase.UpdateUserField(p.UserID,
                    new DbParam("userchar2ID", p.UserAcc.Character2ID),
                    new DbParam("userchar1ID", p.UserAcc.Character1ID),
                    new DbParam("usercipher", p.UserAcc.Cipher));


                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbw", 24, 5, 53, 0));
                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbw", 24, 5, 52, 0));
                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbw", 24, 5, 54, 0));
                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbw", 24, 5, 183, 0));
                tmp.Add(Packet.ConvertfromFormat<Packet>("bb", 20, 8));
                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbb", 35, 2, 1, slot));
            }
            else
                tmp.Add(Packet.ConvertfromFormat<Packet>("bbbb", 35, 2, 3, slot));

            p.SendPacket( tmp.End());
        }
    }
}
