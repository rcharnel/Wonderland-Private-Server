using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBase;
using Network.ActionCodes;

namespace Plugin
{
    public interface PluginHost
    {

        UserDataBase gUserDataBase { get;}
        CharacterDataBase gCharacterDataBase { get; }
        GameDataBase gGameDataBase { get; }
        WorldServerHost GameWorld { get; }
        MapHost gMapManager { get; }

        #region DataFiles
        //ItemManager ItemDataBase { get; }
        //NpcDat NpcDataBase { get; }
        //SkillDataFile SkillManager { get; }
        //EveManager EveDataManager { get; }
        #endregion
    }
}
