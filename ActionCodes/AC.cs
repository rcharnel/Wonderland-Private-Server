using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wlo.Core;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC
    {
        public virtual int ID { get { return 0; } }
        public virtual void ProcessPkt(ref Player c, RecvPacket p)
        {
            switch (p.B)
            {
                default: Utilities.LogServices.Log("Action Code " + p.A + "," + p.B + "has not been coded"); break;
            }
        }
    }
}
