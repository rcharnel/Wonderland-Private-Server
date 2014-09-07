using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GupdtSrv;

namespace Wonderland_Private_Server
{
    static class cGlobal
    {
        public static List<System.Threading.Thread> ThreadManager = new List<System.Threading.Thread>();


        public static gitClient GClient = new gitClient();
    }
}
