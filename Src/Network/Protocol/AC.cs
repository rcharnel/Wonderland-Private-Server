using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Network;

namespace Server.Network
{
    public abstract class AC
    {
        public virtual int ID { get { return 0; } }
        public virtual void Process( Player src, RecievePacket r)
        {
            DebugSystem.Write(DebugItemType.Network_Heavy, "AC {0},{1} has not been coded", r[4], r[5]);
        }
    }
}
