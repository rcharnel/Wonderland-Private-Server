using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace Network
{
     class LoginClient : ConcurrentDictionary<int, Player>
    {
        public void TerminateAll()
        {

        }
    }
}
