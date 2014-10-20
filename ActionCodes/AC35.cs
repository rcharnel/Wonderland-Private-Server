using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Utilities;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC35:AC
    {
        public override int ID { get { return 35; } }
        public override void ProcessPkt(ref Player p, RecvPacket r)
        {
            switch (r.B)
            {
                case 2: Recv2(ref p, r); break;
                default: LogServices.Log(r.A+","+r.B+" Has not been coded"); break;
            }
        }

        void Recv2(ref Player p, RecvPacket e)
        {
            string pw = e.UnpackChar(14);
            byte slot = e.Unpack8(2);

            if (p.Cipher == pw)
            {
               cGlobal.gCharacterDataBase.DeleteCharacter(p.m_charids[slot]);
                p.m_charids[slot] = 0;
               cGlobal.gUserDataBase.Update_Player_ID(p.DataBaseID, 0, slot);

                if (p.m_charids[1] == 0 && p.m_charids[2] == 0)
                    p.Cipher = "";

                Send_24_5(p, 53);
                Send_24_5(p, 52);
                Send_24_5(p, 54);
                Send_24_5(p, 183);
                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 20, 8 });
                p.Send(tmp);
                tmp = new SendPacket();
                tmp.PackArray(new byte[] { 35, 2 });
                tmp.Pack8(1);
                tmp.Pack8(slot);
                p.Send(tmp);
            }
            else
            {
                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 35, 2 });
                tmp.Pack8(3);
                tmp.Pack8(slot);
                p.Send(tmp);
            }

        }

        void Send_24_5(Player player, byte value)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 24, 5 });
            p.Pack8(value);
            p.Pack16(0);
            player.Send(p);

        }
    }
}
