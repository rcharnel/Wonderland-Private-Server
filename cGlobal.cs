using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GupdtSrv;
using System.Reflection;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Config;

namespace Wonderland_Private_Server
{
    static class cGlobal
    {
        public static List<System.Threading.Thread> ThreadManager = new List<System.Threading.Thread>();
        public static Config.Settings SrvSettings = new Config.Settings();

        public static gitClient GClient = new gitClient();


        public static Network.WorldManager WLO_World;
        public static Utilities.Task.TaskManager ApplicationTasks;

        #region DataBase
        public static DBConnector.DBOAuth gDataBaseConnection = new DBConnector.DBOAuth();
        public static DataManagement.DataBase.CharacterDataBase gCharacterDataBase;
        public static DataManagement.DataBase.UserDataBase gUserDataBase;
        public static DataManagement.DataBase.GameDataBase gGameDataBase;
        #endregion

        #region DataFiles
        public static DataManagement.DataFiles.ItemManager gItemManager;
        public static DataManagement.DataFiles.SkillDataFile gSkillManager;
        public static DataManagement.DataFiles.EveManager gEveManager;
        public static DataManagement.DataFiles.NpcDat gNpcManager;        
        #endregion

        #region Systems
        public static Instance gInstanceSystem = new Instance();
        public static GuildSystem gGuildSystem = new GuildSystem();

        #endregion

        #region Settings
        #endregion

        public static ActionCodes.AC GetActionCode(int ID)
        {
            foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
                               where c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(ActionCodes.AC))
                               select c))
            {
                ActionCodes.AC m = null;
                string var = y.Namespace;
                try
                {
                    m = (Activator.CreateInstance(y) as ActionCodes.AC);
                    if (m.ID == ID) return m;
                }
                catch { Utilities.LogServices.Log(new Exception("failed to load AC " + (Activator.CreateInstance(y) as ActionCodes.AC).ID)); }
            }
            return null;
        }
        public static Maps.Map GetMap(ushort mapid)
        {
            return new Maps.Map(cGlobal.gEveManager.GetMapData(mapid));
            //foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
            //                   where c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(Maps.Map))
            //                   select c))
            //{
            //    Maps.Map m = null;

            //    try
            //    {
            //        m = (Activator.CreateInstance(y) as Maps.Map);
            //        if (m.MapID == mapid) return m;
            //    }
            //    catch { Utilities.LogServices.Log(new Exception("failed to load Map " + (Activator.CreateInstance(y) as Maps.Map).MapID)); }
            //}
            //return null;
        }
    }
}
