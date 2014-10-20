using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Enums
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
        Regular,
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
    }
}
