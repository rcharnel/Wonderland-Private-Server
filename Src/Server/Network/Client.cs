using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Network
{
     class Client : ConcurrentDictionary<int, Player>
    {
        public void TerminateAll()
        {

        }
    }
}
