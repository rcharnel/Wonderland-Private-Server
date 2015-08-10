using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Maps
{
    public class MapManager
    {

        public MapManager()
        {
        }

        public GameMap GetMap(ushort ID)
        {
            GameMap tmp = new GameMap();
            tmp.MapID = ID;
            return tmp;
        }

    }
}
