using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Game;
using Game.Code;
using Plugin;

namespace Server
{
    public class MapSystem
    {
        PluginManager pluginList;
        protected ConcurrentDictionary<ushort, GameMap> MapList = new ConcurrentDictionary<ushort, GameMap>();

        public MapSystem(PluginManager src)
        {
            pluginList = src;
            MapList.Clear();
            Thread tmp = new Thread(new ThreadStart(MapThrd));
            tmp.Name = "MapSystemThread";
            tmp.Init();
        }
        ~MapSystem()
        {
        }

        void MapThrd()
        {
            do
            {
                //foreach (var m in MapList)
                //    if (m.Value.IdleTimer.Elapsed > new TimeSpan(0, 1, 0))
                //    {
                //        Map p;
                //        m.Value.Kill = true;
                //        MapList.TryRemove(m.Key, out p);
                //        p = null;
                //    }

                Thread.Sleep(250);
            }
            while (true);
        }

        protected virtual GameMap GetMap(ushort ID)
        {
            if (pluginList != null)
                if (pluginList.MapList.Count(c => c.MapID == ID) == 1)
                    return pluginList.MapList.Single(c => c.MapID == ID).Copy<GameMap>();

            return null;
        }

        //public bool onTelePort(byte portalID, WarpData map,ref Player target)
        //{
        //    if(!MapList.ContainsKey(map.DstMap))
        //    {
        //        //Create Map

        //        Map tmp = new Map(cGlobal.gEveManager.GetMapData(map.DstMap));
        //        cGlobal.gGameDataBase.SetupMap(ref tmp);
        //        if (!MapList.TryAdd(map.DstMap, tmp)) return false;
        //    }
            
        //    MapList[map.DstMap].onWarp_In(portalID, ref target, map);
        //    DebugSystem.Write("Loaded Map (" + MapList[map.DstMap].MapID.ToString() + ") " + MapList[map.DstMap].Name);
        //    return true;
        //}

        //public virtual void onPlayerDisconnected(ref Player src)
        //{
        //    foreach (var m in MapList.Values)
        //    {
        //        if (m.Players.ContainsKey(src.ID))
        //            m.Players.TryRemove(src.ID, out src);
        //    }
        //}
    }
}
