using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixGameServer.Game;
using Phoenix.Core.Networking;

namespace PhoenixGameServer.Network.WAC
{
    public class AC14 : WLOAC
    {
        public override int ID { get { return 14; } }

        public override void Process(Player p, Packet r)
        {
            switch (r.Unpack8())
            {
                case 1: Recv1(p, r); break;
                case 2: Recv2(p, r); break;
                case 3: Recv3(p, r); break;
                case 4: Recv4(p, r); break;
                default: base.Process(p, r); break;
            }
        }

        public void Recv1(Player p, Packet r)//someone sent mail
        {
            switch (r.Unpack8())
            {
                case 1:
                    {
                        uint to = r.Unpack32();
                        string msg = r.UnpackStringN();
                        cGlobal.GameServer.DoAction(to, new Action<Player>(c => p.Mail.SendTo(c, msg)));
                    } break;
            }
        }
        public void Recv2(Player p, Packet r)//request to add friend
        {
            cGlobal.GameServer.DoAction(r.Unpack32(), new Action<Player>(c => c.SendPacket(Packet.ConvertfromFormat<Packet>("bbd", 14, 2, p.CharID))));
        }
        public void Recv3(Player p, Packet r)//they accept/didnt answer/refuse
        {
            uint to = r.Unpack32();
            byte act = r.Unpack8();

            switch (act)
            {
                case 1:
                    {
                        if (cGlobal.GameServer.DoAction(to, new Action<Player>(c => { c.MyFriends.AddFriend(p); p.MyFriends.AddFriend(c); })))
                        {
                            cGlobal.GameServer.DoAction(to, new Action<Player>(c =>
                            {
                                c.SendPacket(Packet.ConvertfromFormat<Packet>("bbdb", 14, 3, p.CharID, act));
                                p.SendPacket(Packet.ConvertfromFormat<Packet>("bbdb", 14, 3, c.CharID, act));
                            }));
                        }
                    } break;
            }

        }
        public void Recv4(Player p, Packet r)//request to delete
        {

            cGlobal.GameServer.DoAction(r.Unpack32(), new Action<Player>(c =>
            {
                c.MyFriends.DelFriend(p.CharID);
                p.MyFriends.DelFriend(c.CharID);
            }));
        }
    }
}
