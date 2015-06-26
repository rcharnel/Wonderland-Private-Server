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
        static readonly object mlock = new object();
        public static bool Run;

        public static gitClient GitClient;

        public static Dictionary<string, IDataManager> DatFile = new Dictionary<string, IDataManager>();

        public static Server.Config.Settings SrvSettings;



        //public static Server.WloWorldNode WLO_World;
        //public static Game.Maps.MapManager gMapManager;

        public static TaskManager ApplicationTasks;

        public static DataBase.CharacterDataBase gCharacterDataBase;
        public static DataBase.UserDataBase gUserDataBase;
        public static DataBase.GameDataBase gGameDataBase;

        #region Systems
        //public static Instance gInstanceSystem = new Instance();
        //public static GuildSystem gGuildSystem = new GuildSystem();

        #endregion

        #region Settings
        #endregion


        //static Dictionary<int, Server.Network.WAC.WLOAC> AcList = new Dictionary<int, Server.Network.WAC.WLOAC>(100);

        //public static Server.Network.WAC.WLOAC GetAction(int ID)
        //{

        //    try
        //    {
        //        return AcList[ID];
        //    }
        //    catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); }

        //    lock (mlock)
        //    {
        //        Server.Network.WAC.WLOAC resp = null;
        //        if (resp == null)
        //        {
        //            foreach (var y in (from c in Assembly.GetEntryAssembly().GetTypes()
        //                               where c.IsClass && !c.IsAbstract && c.IsPublic && c.IsSubclassOf(typeof(Server.Network.WAC.WLOAC))
        //                               select c))
        //            {
        //                Server.Network.WAC.WLOAC m = null;
        //                try
        //                {
        //                    m = (Activator.CreateInstance(y) as Server.Network.WAC.WLOAC);

        //                    AcList.Add(m.ID, m);
        //                    return m;
        //                }
        //                catch { DebugSystem.Write(new ExceptionData(ExceptionSeverity.Warning, "failed to load AC " + m.ID)); m = null; }
        //            }
        //        }
        //        return resp;
        //    }
        //}
    }
}
