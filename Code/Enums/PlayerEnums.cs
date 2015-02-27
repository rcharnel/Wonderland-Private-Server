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
        InGame_InMap,
        InGame_Warping,
        InGame_Interacting,
        

    }
    public enum SendType
    {
        Normal,
        Multi,
        Queue,
    }
}
