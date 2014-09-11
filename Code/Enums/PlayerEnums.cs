using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Enums
{
    public enum PlayerState
    {
        Connected_LoginWindow,
        Connected_CharacterSelection,
        Connected_CharacterCreation,
        Connected_Loggingin,
        InGame_InMap_Standing,
        InGame_Warping,
        InGame_Interacting,
        InGame_Battling_Alive,
        InGame_Battling_Dead,
        InGame_Battling_Sealed,

    }
    public enum SendType
    {
        Normal,
        Multi,
        Queue,
    }
}
