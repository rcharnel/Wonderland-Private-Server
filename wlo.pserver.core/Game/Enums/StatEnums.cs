using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum RebornJob : byte
    {
        none,
        Killer = 1,
        Warrior,
        Knight,
        Wit,
        Priest,
        Seer,
        NissRB,

    }
    public enum Affinity
    {
        Normal = 0,
        Earth = 1,
        Water = 2,
        Fire = 3,
        Wind = 4,
        Undefined = 7,
    }
}
