using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum MapObjType
    {
        None,
        WarpData,
        WarPortal,
        Mob,
        Npc,
        Item,

    }
    public enum MapType
    {
        RegularMap,
        Tent,
    }
    public enum TeleportType
    {
        Regular,
        Special,
        Tent,
        Tool,
        Login,
        Quest,
        CmD,
    }
}
