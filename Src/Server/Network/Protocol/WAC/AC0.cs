using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;
using PhoenixGameServer.Game;

namespace PhoenixGameServer.Network.WAC
{
    public class AC0:WLOAC 
    {

        public override void Process( Player client, Phoenix.Core.Networking.Packet r)
        {
            PacketBuilder s = new PacketBuilder();
            s.Begin();
            s.Add(new byte[] { 1, 9 });
            s.Add(new byte[] { 107, 000, 001, });
            s.Add("WloPhoenix");
            client.SendPacket(s.End());
            s.Begin();
            s.Add(new byte[] { 54, 29 });
            s.Add(new byte[] {037, 001, 145, 001, 002, 101, 000,
                002, 102, 000, 002, 103, 000, 002, 106, 000, 002, 202,
                000, 002, 201, 000, 002, 204, 000, 002, 203, 000, 002,
                045, 001, 002, 047, 001, 001, 105, 000, 002, 046, 001,
                001, 146, 001, 001, 104, 000, 002, 107, 000, 002, 148,
                001, 001, 147, 001, 001, 245, 001, 002, 246, 001, 001, 
                247, 001, 001, 234, 003, 001, 235, 003, 001, 078, 004, 
                001, 079, 004, 001, 035, 003, 001, 033, 003, 002, 034,
                003, 001, 233, 003, 002, 133, 003, 001, 135, 003, 001,
                134, 003, 001, 077, 004, 002});
            client.SendPacket(s.End());
        }
    }
}
