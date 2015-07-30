using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using RCLibrary.Core.Networking;

namespace Network.ActionCodes
{
    public class AC35 : AC
    {
        public override int ID { get { return 35; } }
        public override void ProcessPkt(Player p, RecievePacket r)
        {
            switch (r.Unpack8())
            {
                case 2: Recv2(ref p, r); break;
            }
        }

        void Recv2(ref Player p, RecievePacket e)
        {
            byte slot = e.Unpack8();
            string uknw = e.UnpackString();
            string pw = e.UnpackString();

            if (p.UserAcc.Cipher == pw)
            {
                cGlobal.gCharacterDataBase.DeleteCharacter((slot == 1) ? p.UserAcc.Character1ID : p.UserAcc.Character2ID);

                cGlobal.gUserDataBase.Update_Player_ID(p.UserAcc.DataBaseID, 0, slot);

                if (cGlobal.gCharacterDataBase.GetCharacterData(p.UserAcc.Character1ID) == null && cGlobal.gCharacterDataBase.GetCharacterData(p.UserAcc.Character2ID) == null)
                    p.UserAcc.Cipher = "";
                p.Send( SendPacket.FromFormat("bbbb", 24, 5, 53, 0));
                p.Send( SendPacket.FromFormat("bbbb", 24, 5, 52, 0));
                p.Send( SendPacket.FromFormat("bbbb", 24, 5, 54, 0));
                p.Send( SendPacket.FromFormat("bbbb", 24, 5, 183, 0));
                p.Send( SendPacket.FromFormat("bb", 20, 8));
                p.Send( SendPacket.FromFormat("bbbb", 35, 2, 1, slot));
            }
            else
                p.Send( SendPacket.FromFormat("bbbb", 35, 2, 3, slot));

        }

        void Send_24_5(Player player, byte value)
        {
            player.Send( SendPacket.FromFormat("bbbb", 24, 5, value, 0));

        }
    }
}
