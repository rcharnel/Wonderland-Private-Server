using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GupdtSrv;
using DataFiles;
using System.Reflection;
using Wonderland_Private_Server.Config;
using System.Collections.Concurrent;
using Server;

namespace System
{
    static class cGlobal
    {

        
        public static bool Run;

        public static gitClient GitClient;

        

        public static Server.Config.Settings SrvSettings;

        public static DataBase.CharacterDataBase gCharacterDataBase;
        public static DataBase.UserDataBase gUserDataBase;
        public static DataBase.GameDataBase gGameDataBase;

        public static DataFiles.PhxItemDat ItemDatManager;

        public static LoginServer gLoginServer;
        public static WorldServer gWorld;

        //public static Server.WloWorldNode WLO_World;
        //public static Game.Maps.MapManager gMapManager;

        public static TaskManager ApplicationTasks;

        

        #region Systems
        //public static Instance gInstanceSystem = new Instance();
        //public static GuildSystem gGuildSystem = new GuildSystem();

        #endregion

        #region Settings
        #endregion


        
    }
}
