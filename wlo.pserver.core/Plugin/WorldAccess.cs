using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Maps;
using Network;

namespace Plugin
{

    public interface WorldServerHost
    {

        //Player GetPlayer(uint id);
        bool isOnline(uint id);
        void DisconnectPlayer(uint id);
        //bool TransferinGame(ushort map, ref Player src);
        //void Teleport(byte portalID, WarpData map, Player target);

        void Broadcast(SendPacket pkt);
        void Broadcast(SendPacket pkt, uint? directTo = null, bool exclude = false);
    }
}
