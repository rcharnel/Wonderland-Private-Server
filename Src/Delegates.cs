using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Interface;

namespace Wonderland_Private_Server
{
    public delegate void onWorldEvent(Player src, WorldEventType args);


    public delegate void BattleEvent(Fighter src,BattleAction cmd);
}
