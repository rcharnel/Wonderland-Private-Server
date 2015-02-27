using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Wonderland_Private_Server.Maps;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Network
{
    public class MapSystem
    {
        ConcurrentDictionary<ushort, Maps.Map> MapList = new ConcurrentDictionary<ushort, Maps.Map>();

        public MapSystem()
        {
            MapList.Clear();
            Thread tmp = new Thread(new ThreadStart(MapThrd));
            tmp.Name = "MapSystemThread";
            tmp.Init();
        }


        void MapThrd()
        {
            do
            {
                foreach (var m in MapList)
                    if (m.Value.IdleTimer.Elapsed > new TimeSpan(0, 1, 0))
                    {
                        Map p;
                        m.Value.Kill = true;
                        MapList.TryRemove(m.Key, out p);
                        p = null;
                    }

                Thread.Sleep(250);
            }
            while (true);
        }

        public bool onTelePort(byte portalID, WarpData map,ref Player target)
        {
            if(!MapList.ContainsKey(map.DstMap))
            {
                //Create Map

                Map tmp = new Map(cGlobal.gEveManager.GetMapData(map.DstMap));
                cGlobal.gGameDataBase.SetupMap(ref tmp);
                if (!MapList.TryAdd(map.DstMap, tmp)) return false;
            }
            
            MapList[map.DstMap].onWarp_In(portalID, ref target, map);
            Utilities.LogServices.Log("Loaded Map (" + MapList[map.DstMap].MapID.ToString() + ") " + MapList[map.DstMap].Name);
            return true;
        }

        public virtual void onPlayerDisconnected(ref Player src)
        {
            foreach (var m in MapList.Values)
            {
                if (m.Players.ContainsKey(src.ID))
                    m.Players.TryRemove(src.ID, out src);
            }
        }
    }
}
