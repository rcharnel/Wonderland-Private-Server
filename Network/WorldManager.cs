using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Reflection;
using Wonderland_Private_Server.Maps;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Network
{
    /// <summary>
    /// Handles the Recv and Packet Proccesing of all clients
    /// </summary>
    public class WorldManager
    {
        Thread Mainthrd,Mapthrd,Eventthrd;
        bool killFlag;
        readonly ManualResetEvent mylock;
        //readonly Semaphore ProcessLock,SendLock;
        //public event onWorldEvent WorldEvent;
        //event wOnlineCheck OnlineCheck;
        //event wDCPlayerEvent DCPlayer;

        /// <summary>
        /// List of Ip Adresses and thier respective clients
        /// </summary>
        ConcurrentDictionary<System.Net.IPAddress, Client> ConnectedPlayers = new ConcurrentDictionary<System.Net.IPAddress, Client>();
        /// <summary>
        /// Maps loaded into the world
        /// </summary>
        ConcurrentDictionary<ushort, Map> MapList = new ConcurrentDictionary<ushort, Map>();
               
        
        System.Diagnostics.Stopwatch exptimer = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// Clients that are in the Process of Logging into the Server
        /// </summary>
        public int Clients_Connected { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count); return a; } }
        /// <summary>
        /// Clients playing in the Server
        /// </summary>
        public int Clients_inGame { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count(n => n.inGame)); return a; } }

        public WorldManager()
        {
            mylock = new ManualResetEvent(false);
            //ProcessLock = new Semaphore(2, 5);
            //SendLock = new Semaphore(2, 3);
        }

        

        public void QueueaLoginClient(Socket client)
        {
            var p = new Player(ref client);

            if (ConnectedPlayers.ContainsKey(p.ClientIP))
            {
                if (!ConnectedPlayers[p.ClientIP].ContainsKey(p.ClientPort))
                    ConnectedPlayers[p.ClientIP].TryAdd(p.ClientPort, p);
                else
                    p.Disconnect();
            }
            else
            {
                Client tmp = new Client();
                tmp.TryAdd(p.ClientPort, p);
                ConnectedPlayers.TryAdd(p.ClientIP, tmp);
               
            }
            mylock.Set();
        }
        public void QueueAssistToolClient(Socket client)
        {
            //var p = new AssistTool(ref client);
            //if (ConnectedPlayers.ContainsKey(p.ClientIP))
            //{
            //    if (ConnectedPlayers[p.ClientIP].Tool == null)
            //        ConnectedPlayers[p.ClientIP].Tool = p;
            //    else
            //        p.Disconnect();
            //}
            //else
            //{
            //    Client tmp = new Client();
            //    tmp.Tool = p;
            //    ConnectedPlayers.TryAdd(p.ClientIP, tmp);     
            //    mylock.Set();          
            //}
            
        }

        public void Initialize()
        {
            killFlag = false;
            Mainthrd = new Thread(new ThreadStart(Mainwrk));
            Mainthrd.Name = "World Manager Main Thread";
            Mainthrd.Start();
            Mapthrd = new Thread(new ThreadStart(Mapwrk));
            Mapthrd.Name = "World Manager Main Thread";
            Mapthrd.Start();
            Eventthrd = new Thread(new ThreadStart(Eventwrk));
            Eventthrd.Name = "World Manager Main Thread";
            Eventthrd.Start();
        }

        public void Kill()
        {
            
            killFlag = true;
            mylock.Set();
            while (Mainthrd != null && Mainthrd.IsAlive) { Thread.Sleep(1); }
            Mainthrd = null;
            while (Mapthrd != null && Mapthrd.IsAlive) { Thread.Sleep(1); }
            Mapthrd = null;
            while (Eventthrd != null && Eventthrd.IsAlive) { Thread.Sleep(1); }
            Eventthrd = null;
        }

        void Mainwrk()// processes connected clients
        {
            exptimer.Start();
            do
            {
                #region Connected Clients
                foreach (var g in ConnectedPlayers.Values.Where(c => c.Count > 0))
                {
                    try
                    {
                        //if (g.Tool != null && !g.Tool.isAlive())
                        //{
                        //    Utilities.LogServices.Log(String.Format("{0} AssistTool Disconnectd",g.Tool.ClientIP));
                        //    g.Tool = null;
                        //}
                        if (g != null && g.Count > 0)
                        {
                            foreach (var c in g.Values.Where(c => c != null))
                            {
                                Player j = null;
                                Client t = null;

                                if (!c.inGame && c.Elapsed > new TimeSpan(0, (c.inGame) ? 35 : 5, 0))
                                {
                                    ConnectedPlayers[c.ClientIP].TryRemove(c.ClientPort, out j);
                                    //use disconnect Player Function switch if player is inGame
                                    if (ConnectedPlayers[j.ClientIP].Count == 0)
                                        ConnectedPlayers.TryRemove(j.ClientIP, out t);

                                    Utilities.LogServices.Log(String.Format("Client {0} timed out at Login.", j.ClientIP + ":" + j.ClientPort));
                                    SendPacket sp = new SendPacket(true);// PSENDPACKET PackSend = new SENDPACKET;
                                    sp.PackArray(new byte[] { 1, 7 });//PackSend->Header(63,2);
                                    j.Send(sp);
                                    j.BlockSave = true;
                                    j.Disconnect();
                                    continue;
                                }
                                else if (!c.isAlive)
                                {
                                    ConnectedPlayers[c.ClientIP].TryRemove(c.ClientPort, out j);
                                    //use disconnect Player Function
                                    if (ConnectedPlayers[j.ClientIP].Count == 0)
                                        ConnectedPlayers.TryRemove(j.ClientIP, out t);
                                    PlayerDisconnected(j);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Utilities.LogServices.Log(ex); }
                }
                #endregion


                Thread.Sleep(120);

                
            if(exptimer.Elapsed >= new TimeSpan(0,0,30))
                {
                    foreach (var m in MapList.Values.ToList())
                        try
                        {
                            foreach (var p in m.Players.Values.ToList())
                                p.CurExp += (int)((p.Level * new Random().Next(1, 700)) * 2.2);
                        }
                        catch { }
                    exptimer.Restart();
                }
            }
            while (!killFlag);

            foreach (var c in ConnectedPlayers.Values)
                foreach(var p in c.Values.ToList())
            {
                p.Disconnect();
                PlayerDisconnected(p);
            }

            MapList.Clear();
        }
        void Mapwrk()// processes connected clients
        {
            do
            {
                foreach (var map in MapList.Values.ToList())
                    map.UpdateMap();
                Thread.Sleep(1);
            }
            while (!killFlag);
        }
        void Eventwrk()// processes connected clients
        {
            do
            {
                Thread.Sleep(120);
            }
            while (!killFlag);
        }


        public Player GetPlayer(uint ID)
        {

            foreach (var e in ConnectedPlayers.Values)
                foreach (var t in e.Values)
                    if (t.ID == ID)
                        return t;

            return null; 
        }
        /// <summary>
        /// Checks by dataBaseID if Online
        /// </summary>
        /// <param CharacterName="ID"></param>
        /// <returns></returns>
        public bool isOnline(uint id)
        {
            foreach (var e in ConnectedPlayers.Values)
                foreach (var t in e.Values)
                    if (t.DataBaseID == id && t.State != PlayerState.Connected_LoginWindow)
                        return true;

            return false;
        }
        /// <summary>
        /// Disconnects a player
        /// </summary>
        /// <param CharacterName="id"></param>
        public void DisconnectPlayer(uint id)
        {
            foreach (var e in ConnectedPlayers.Values)
                foreach(var r in e.Values )
                    if (r.DataBaseID == id) { r.Disconnect(); return; }
        }
        /// <summary>
        /// Transfer players to the main game thread
        /// </summary>
        /// <param CharacterName="map"></param>
        /// <param CharacterName="src"></param>
        /// <returns></returns>
        public bool TransferinGame(ushort map, ref Player src)
        {
            lock (mylock)
            {
                bool loadmap = false;
                // load map if necessary
                if (MapList.ContainsKey(map))
                    try { MapList[map].onLogin(ref src); loadmap = true; }
                    catch (Exception ex) { Utilities.LogServices.Log(ex.Message); }
                else
                {
                    try
                    {
                        var newmap = cGlobal.GetMap(map);

                        if (newmap != null)
                        {
                            newmap.onLogin(ref src);
                            MapList.TryAdd(newmap.MapID, newmap);
                            Utilities.LogServices.Log("Loaded Map (" + newmap.MapID.ToString() + ") " + newmap.Name);
                            loadmap = true;
                        }
                    }
                    catch (Exception ex) { Utilities.LogServices.Log(ex.Message); }
                }
                return loadmap;
            }
        }

        public void Teleport(byte portalID, WarpData map, Player target)
        {
            Player r = target;
            if (MapList.ContainsKey(map.DstMap))
                try { MapList[map.DstMap].onWarp_In(portalID, ref r, map); }
                catch (Exception ex) { Utilities.LogServices.Log(ex.Message); }
            else
            {
                try
                {
                    var newmap = cGlobal.GetMap(map.DstMap);

                    if (newmap != null)
                    {
                        newmap.onWarp_In(portalID, ref r, map);
                        MapList.TryAdd(newmap.MapID, newmap);
                        Utilities.LogServices.Log("Loaded Map (" + newmap.MapID.ToString() + ") " + newmap.Name);
                    }
                }
                catch (Exception ex) { Utilities.LogServices.Log(ex.Message); }
            }
        }
        /// <summary>
        /// Used via GM command
        /// </summary>
        /// <param CharacterName="portalID"></param>
        /// <param CharacterName="map"></param>
        /// <param CharacterName="target"></param>
        /// <param CharacterName="arg"></param>
        //public void Teleport(byte portalID, WarpData map, Player target, string arg)
        //{
        //    var mapid = target.CurrentMap.MapID;
        //    Player r = target;
        //    if (MapList.ContainsKey(map.DstMap))
        //        try
        //        {
        //            target.CurrentMap.onWarp_Out(portalID, ref target, map);
        //            MapList[map.DstMap].onWarp_In(portalID, ref r, map);
        //            switch (arg)
        //            {
        //                case "MAPGM":
        //                    {
        //                        foreach (var y in Clients.Values.Where(c => c.CurrentMap.MapID == mapid && c.GM))
        //                        {
        //                            Player u = y;
        //                            MapList[mapid].onWarp_Out(portalID, ref u, map);
        //                            MapList[map.DstMap].onWarp_In(portalID, ref u, map);
        //                        }
        //                    } break;
        //            }
        //        }
        //        catch (Exception d) { DLogger.ErrorLog(d); }
        //    else
        //    {
        //        try
        //        {
        //            foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
        //                               where c.IsClass && c.IsSubclassOf(typeof(MapPlugin))
        //                               select c))
        //            {
        //                var m = (Activator.CreateInstance(y) as MapPlugin);
        //                if (m.MapID == map.DstMap)
        //                {
        //                    target.CurrentMap.onWarp_Out(portalID, ref target, map);
        //                    m.onWarp_In(portalID, ref r, map);
        //                    switch (arg)
        //                    {
        //                        case "MAPGM":
        //                            {
        //                                foreach (var p in Clients.Values.Where(c => c.CurrentMap.MapID == mapid && c.GM))
        //                                {
        //                                    Player u = p;
        //                                    MapList[mapid].onWarp_Out(portalID, ref u, map);
        //                                    m.onWarp_In(portalID, ref u, map);
        //                                }
        //                            } break;
        //                    }
        //                    MapList.TryAdd(m.MapID, m);
        //                }
        //            }
        //        }
        //        catch (Exception e) { DLogger.ErrorLog(e); }
        //    }
        //}
       
        /// <summary>
        /// Broadcasts a packet to all
        /// </summary>
        /// <param CharacterName="pkt"></param>
        public void BroadcastTo(SendPacket pkt)
        {
            foreach (var d in ConnectedPlayers.Values)
                foreach (var p in d.Values)
                    p.Send(pkt);
        }
        /// <summary>
        /// Broadcast's a direct packet or a packet that will ignore the selcected
        /// </summary>
        /// <param CharacterName="pkt"></param>
        /// <param CharacterName="directTo">Person to send to/ or avoid</param>
        /// <param CharacterName="avoid">Avoid the Person and send to everyone else</param>
        public void BroadcastTo(SendPacket pkt, uint? directTo = null, bool exclude = false)
        {
            foreach (var d in ConnectedPlayers.Values)
                foreach(var p in d.Values)
                    if (p.ID == directTo && exclude)
                    continue;
                    else if (p.ID == directTo && !exclude)
                {
                    p.Send(pkt); return;
                }
                    else if (exclude && p.ID != directTo)
                    p.Send(pkt);
        }

        public void PlayerDisconnected(object src)
        {
            
            Player c = (Player)src;
            //if (WorldEvent != null) WorldEvent(c, WorldEventType.PlayerLogoff);
            //dc socket
            c.Disconnect();
            //Inform Maps we Dced
            try { c.CurrentMap.onLogout(ref c); }
            catch (Exception e) { }

            //unlock CharacterName
            cGlobal.gCharacterDataBase.unLockName(c.CharacterName);
            //send dc packet
            SendPacket bye = new SendPacket();
            bye.PackArray(new byte[] { 1, 1 });
            bye.Pack32(c.ID);
            BroadcastTo(bye, c.ID, true);
            //save data
            if (cGlobal.gCharacterDataBase.WritePlayer(c.ID, c))
                Utilities.LogServices.Log(c.UserName + " Info has been Fully Saved");

            Utilities.LogServices.Log(String.Format("Client {0} {1} Disconnected.", c.ClientIP+":"+c.ClientPort, c.UserName));
        }

        public void SendCurrentPlayers(Player to)
        {
            SendPacket p = new SendPacket();
            foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
                               where c.IsClass && c.IsSubclassOf(typeof(Bots.GmBot))
                               select c))
            {
                var c = (Activator.CreateInstance(y) as Bots.GmBot);
                p = new SendPacket();
                p.PackArray(new byte[] { 4 });
                p.Pack32(c.ID);
                p.Pack8((byte)c.Body); //body style
                p.Pack8((byte)c.Element); //element
                p.Pack8(c.Level); //level
                p.Pack16((c.CurrentMap == null) ? c.LoginMap : c.CurrentMap.MapID); //map id
                p.Pack16(c.X); //x
                p.Pack16(c.Y); //y
                p.Pack8(0); p.Pack8(c.Head); p.Pack8(0);
                p.Pack16(c.HairColor);
                p.Pack16(c.SkinColor);
                p.Pack16(c.ClothingColor);
                p.Pack16(c.EyeColor);
                p.Pack8(c.WornCount);//clothesAmmt); // ammt of clothes
                p.PackArray(c.Worn_Equips);
                p.Pack32(0); p.Pack8(0); //??
                p.PackBoolean(c.Reborn); //is rebirth
                p.Pack8((byte)c.Job); //rb class
                p.PackString(c.CharacterName);//(BYTE*)c.CharacterName,c.nameLen); //CharacterName
                p.PackString(c.Nickname);//(BYTE*)c.nick,c.nickLen); //nickname
                p.Pack8(255); //??
                to.Send(p);
            }
            foreach (var e in ConnectedPlayers.Values)
                foreach(var d in e.Values.Where(c=>c.inGame))
            {
                Character c = d;
                p = new SendPacket();
                p.PackArray(new byte[] { 4 });
                p.Pack32(d.ID);
                p.Pack8((byte)c.Body); //body style
                p.Pack8((byte)c.Element); //element
                p.Pack8(c.Level); //level
                p.Pack16(c.CurrentMap.MapID); //map id
                p.Pack16(c.X); //x
                p.Pack16(c.Y); //y
                p.Pack8(0); p.Pack8(c.Head); p.Pack8(0);
                p.Pack16(c.HairColor);
                p.Pack16(c.SkinColor);
                p.Pack16(c.ClothingColor);
                p.Pack16(c.EyeColor);
                p.Pack8(c.WornCount);//clothesAmmt); // ammt of clothes
                p.PackArray(c.Worn_Equips);
                p.Pack32(0); p.Pack8(0); //??
                p.PackBoolean(c.Reborn); //is rebirth
                p.Pack8((byte)c.Job); //rb class
                p.PackString(c.CharacterName);//(BYTE*)c.CharacterName,c.nameLen); //CharacterName
                p.PackString(c.Nickname);//(BYTE*)c.nick,c.nickLen); //nickname
                p.Pack8(255); //??
                to.Send(p);
                to.onPlayerLogin(c.ID);
                d.onPlayerLogin(to.ID);
            }
        }    

        public void MapUpdated(Map newmap)
        {
            Map old = null;

            if(MapList.ContainsKey(newmap.MapID))
            {
                MapList.TryRemove(newmap.MapID, out old);
                if(old != null)
                {
                    old.MapneedsUpdate = true;
                    //EndedMaps.Add(new KeyValuePair<Map, DateTime>(old, DateTime.Now.AddMinutes(3)));
                }
            }
        }
    }
}
