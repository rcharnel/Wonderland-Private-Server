using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace Plugin
{
    public interface MapHost
    {
        GameMap GetMap(ushort ID);
    }
}
