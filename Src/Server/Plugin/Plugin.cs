using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin;
using System.Reflection;
using System.Runtime;
using System.IO;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataBase;
using Network.ActionCodes;
using Game;

namespace Plugin
{
    public class PluginManager : PluginHost
    {
        readonly object mylock = new object();
        readonly HashAlgorithm SHA1 = new SHA1Managed();
        Dictionary<string, string> waitlist = new Dictionary<string, string>();

        public BindingList<AC> AcList = new BindingList<AC>();
        public BindingList<GameMap> MapList = new BindingList<GameMap>();

        FileSystemWatcher folderwatch = new FileSystemWatcher();

        #region Contract
        public GameDataBase gGameDataBase { get { return cGlobal.gGameDataBase; } }
        public CharacterDataBase gCharacterDataBase  { get { return cGlobal.gCharacterDataBase; } }
        public UserDataBase gUserDataBase  { get { return cGlobal.gUserDataBase; } }
        public WorldServerHost GameWorld { get { return cGlobal.gWorld; } }
        //public ItemManager ItemDataBase { get { return cGlobal.WloItemManager; } }
        //public NpcDat NpcDataBase { get { return cGlobal.WloNpcManager; } }
        //public SkillDataFile SkillManager { get { return cGlobal.WloSkillManager; } }
        //public EveManager EveDataManager { get { return cGlobal.EveDataFile; } }

        #endregion

        #region Methods
        public void Intialize()
        {
            //Setups to watch a folder for any changes
            var appDataPath = Environment.CurrentDirectory;
            var dbPath = Path.Combine(appDataPath, "Maps");

            if (!Directory.Exists(dbPath)) Directory.CreateDirectory(dbPath);


            folderwatch.Path = dbPath;
            folderwatch.Filter = "*.dll";
            folderwatch.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
            folderwatch.IncludeSubdirectories = true;
            folderwatch.EnableRaisingEvents = true;
            folderwatch.Deleted += folderwatch_Deleted;
            folderwatch.Created += folderwatch_Created;
            folderwatch.Changed += folderwatch_Changed;

            DebugSystem.Write("[PluginSystem][Info] - Scanning for Maps");
            ScanFolder();
        }

        void folderwatch_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                //kk we should be ok to update our files
                var file = new FileInfo(e.FullPath);

                //Generate obj
                var asm = Assembly.Load(File.ReadAllBytes(e.FullPath));
                Type[] types = asm.GetTypes();

                if (types.Count(c => typeof(GameMap).IsAssignableFrom(c)) > 0)
                {
                    for (int a = 0; a < MapList.Count; a++)
                    {
                        if (MapList[a].FileID == file.Name && file.LastWriteTime > MapList[a].Lastwrite)
                        {
                            MapList[a] = (GameMap)Activator.CreateInstance(asm.GetType(types.Single(c => typeof(GameMap).IsAssignableFrom(c)).ToString()), new object[] { this, new FileInfo(e.FullPath) });
                           DebugSystem.Write("[PluginSystem][Info] - Map " + MapList[a].MapID + " has been updated");
                            break;
                        }
                    }
                }
            }
            catch (Exception d) { }
        }

        void folderwatch_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                var file = new FileInfo(e.FullPath);

                //Generate obj
                var asm = Assembly.Load(File.ReadAllBytes(e.FullPath));

                Type[] types = asm.GetTypes();
                if (types.Count(c => typeof(GameMap).IsAssignableFrom(c)) > 0)
                {
                    Type type = types.Single(c => typeof(GameMap).IsAssignableFrom(c) && c.IsPublic);
                    var gh = (GameMap)Activator.CreateInstance(asm.GetType(type.ToString()), new object[] { this, new FileInfo(e.FullPath) });

                    if (MapList.Count(c => c.FileID == file.Name) == 0)
                    {
                        MapList.Add(gh);
                        DebugSystem.Write("[PluginSystem][Info] - Map " + gh.MapID + " has been loaded into the server");
                    }
                }
            }
            catch { }
        }

        void folderwatch_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                FileInfo o = new FileInfo(e.FullPath);
                if (MapList.Count(c => c.FileID == o.Name) > 0)
                {
                    lock (mylock)
                    {
                        GameMap r = MapList.Single(c => c.FileID == o.Name);
                        MapList.Remove(r);
                        DebugSystem.Write("[PluginSystem][Info] - Map " + r.MapID + " has been unloaded");
                    }
                }
            //    else if (AcList.Count(c => c.FileID == o.Name) > 0)
            //    {
            //        lock (mylock)
            //        {
            //            AcList.Remove(AcList.Single(c => c.FileID == o.Name));
            //            DLogger.SystemLog(o.Name.Split('.')[0] + " has been unloaded");
            //        }
            //    }
            }
            catch { }
        }

        public void ScanFolder()
        {
            foreach (FileInfo e in new DirectoryInfo(folderwatch.Path).EnumerateFiles("*.dll", SearchOption.AllDirectories))
            {
                //Generate obj

                try
                {
                    var file = new FileInfo(e.FullName);

                    //Generate obj
                    var asm = Assembly.Load(File.ReadAllBytes(e.FullName));

                    Type[] types = asm.GetTypes();
                    if (types.Count(c => typeof(GameMap).IsAssignableFrom(c)) > 0)
                    {
                        Type type = types.Single(c => typeof(GameMap).IsAssignableFrom(c) && c.IsPublic);
                        var gh = (GameMap)Activator.CreateInstance(asm.GetType(type.ToString()), new object[] { this, e });

                        if (MapList.Count(c => c.FileID == file.Name) == 0)
                        {
                            MapList.Add(gh);
                            DebugSystem.Write("[PluginSystem][Info] - Map " + gh.MapID + " has been loaded into the server");
                        }
                    }
                }
                catch { }
            }

            DebugSystem.Write("[PluginSystem][Info] - Found " + MapList.Count + " Maps");
        }

        //public AC GetAC(int id)
        //{
        //    for (int a = 0; a < AcList.Count;a++ )
        //        if (AcList[a].ID == id)
        //        {
        //            try
        //            {
        //                return AcList[a].Copy<AC>();
        //            }
        //            catch (Exception f) { return null; }
        //        }
        //    return null;
        //}

        //public bool isNewVersion_ofAC(AcPlugin oldac, out AcPlugin newac)
        //{
        //    var e = AcList.SingleOrDefault(c => c.ID == oldac.ID);

        //    if (e != null)
        //    {
        //        try
        //        {
        //            newac = e.Copy<AcPlugin>();
        //            return (e.Lastwrite != ((PluginObj)oldac).Lastwrite);
        //        }
        //        catch { }
        //    }
        //    newac = null;
        //    return false;
        //}
        #endregion

        public void Kill()
        {
            folderwatch.EnableRaisingEvents = false;
        }
    }
}
