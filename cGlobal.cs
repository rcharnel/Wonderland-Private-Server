using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GupdtSrv;
using System.Reflection;

namespace Wonderland_Private_Server
{
    static class cGlobal
    {
        public static List<System.Threading.Thread> ThreadManager = new List<System.Threading.Thread>();


        public static gitClient GClient = new gitClient();


        public static Network.WorldManager WLO_World;


        public static DataManagement.DataBase.CharacterDataBase gCharacterDataBase;
        public static DataManagement.DataBase.UserDataBase gUserDataBase;
        public static DataManagement.DataBase.GameDataBase gGameDataBase;


        public static DataManagement.DataFiles.ItemManager gItemManager;
        public static DataManagement.DataFiles.SkillDataFile gSkillManager;
        public static DataManagement.DataFiles.EveManager gEveManager;


        public static ActionCodes.AC GetActionCode(int ID)
        {
            foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
                               where c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(ActionCodes.AC))
                               select c))
            {
                ActionCodes.AC m = null;

                try
                {
                    m = (Activator.CreateInstance(y) as ActionCodes.AC);
                    if (m.ID == ID) return m;
                }
                catch { Utilities.LogServices.Log(new Exception("failed to load AC " + (Activator.CreateInstance(y) as ActionCodes.AC).ID)); }
            }
            return null;
        }
    }
}
