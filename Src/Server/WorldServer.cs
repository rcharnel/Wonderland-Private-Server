using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Reflection;
using Game;
using Network;
using Plugin;

namespace Server
{
    /// <summary>
    /// Handles the Recv and SendPacket Proccesing of all clients
    /// </summary>
    public class WorldServer:WorldServerHost
    {
        Thread Mainthrd,Mapthrd,Eventthrd;
        bool killFlag;
        readonly ManualResetEvent mylock;
        //readonly Semaphore ProcessLock,SendLock;
        //public event onWorldEvent WorldEvent;
        //event wOnlineCheck OnlineCheck;
        //event wDCPlayerEvent DCPlayer;

        /// <summary>
        /// List of Players
        /// </summary>
        List<Player> Players = new List<Player>();


        /// <summary>
        /// Maps loaded into the world
        /// </summary>
        ConcurrentDictionary<ushort, GameMap> MapList = new ConcurrentDictionary<ushort, GameMap>();
               
        // System.Diagnostics.Stopwatch exptimer = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// Clients that are in the Process of Logging into the Server
        /// </summary>
        //public int Clients_Connected { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count); return a; } }
        ///// <summary>
        ///// Clients playing in the Server
        ///// </summary>
        //public int Clients_inGame { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count(n => n.inGame)); return a; } }

        public WorldServer()
        {
            mylock = new ManualResetEvent(false);
        }



        public void OnNewClient(Player client)
        {

            if (Players.Contains(client))
                client.Disconnect();
            else
                Players.Add(client);

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
            Mainthrd = new Thread(new ThreadStart(MainLoop));
            Mainthrd.Name = "World Manager Main Thread";
            Mainthrd.Init();
            Mapthrd = new Thread(new ThreadStart(Mapwrk));
            Mapthrd.Name = "World Manager Map Thread";
            Mapthrd.Init();
            Eventthrd = new Thread(new ThreadStart(Eventwrk));
            Eventthrd.Name = "World Manager Event Thread";
            Eventthrd.Init();
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

        void MainLoop()
        {
            Queue<Player> DeadClients = new Queue<Player>();

            do
            {
                #region Queued Clients

                try
                {

                    Parallel.ForEach<Player>(Players, new Action<Player>(c =>
                        {
                            if (c.TimeIdle > new TimeSpan(0, 5, 0) || c.isDisconnected())
                            {
                                if (!c.isDisconnected())
                                    c.Disconnect();
                                DebugSystem.Write(DebugItemType.Info_Light, "Client {0} timed out.", c.SockAddress());
                                c.Send(new SendPacket(new byte[] { 1, 7 }));
                                DeadClients.Enqueue(c);
                            }


                        }));
                }
                catch (AggregateException e)
                {
                    foreach (var se in e.InnerExceptions)
                    {
                    }
                }

                if (DeadClients.Count > 0)
                {
                }
                #endregion
                Thread.Sleep(1);
            }
            while (!killFlag);

            //Parallel.ForEach<Player>(Players, new Action<Player>(c =>
            //        {
            //            c.Disconnect();
            //            onPlayerDisconnected(ref c);
            //        }));



        }



        #region Threads
        //void Mainwrk()// processes connected clients
        //{
        //    exptimer.Start();
        //    do
        //    {
        //        #region Connected Clients
        //        foreach (var c in Players)
        //        {
        //            try
        //            {
        //                Player j;
        //                if (!c.inGame && c.Elapsed > new TimeSpan(0, (c.inGame) ? 35 : 5, 0))
        //                {
        //                    Players.TryRemove(c.ID, out j);

        //                    DebugSystem.Write(String.Format("Client {0} timed out at Login.", j.ClientIP + ":" + j.ClientPort));
        //                    SendPacket sp = new SendPacket(true);// PSENDPACKET PackSend = new SENDPACKET;
        //                    sp.Pack(new byte[] { 1, 7 });//PackSend->Header(63,2);
        //                    j.Send(sp);
        //                    j.BlockSave = true;
        //                    j.Disconnect();
        //                    continue;
        //                }
        //                else if (!c.isAlive)
        //                {
        //                    Players.TryRemove(c.ID, out j);
        //                    onPlayerDisconnected(ref j);
        //                }
        //            }
        //            catch (Exception ex) { DebugSystem.Write(ex); }
        //        }
        //        #endregion


        //        Thread.Sleep(120);

        //        try
        //        {
        //            if (exptimer.Elapsed >= new TimeSpan(0, 1, 20))
        //            {
        //                foreach (var m in MapList.Values.ToList())
        //                    try
        //                    {
        //                        foreach (var p in m.Players.Values.ToList())
        //                            p.CurExp += (int)((p.Level * new Random().Next(1, 700)) * 2.2);
        //                    }
        //                    catch { }
        //                exptimer.Restart();
        //            }
        //        }
        //        catch { }
        //    }
        //    while (!killFlag);

        //    foreach (var p in Players.Values)
        //    {
        //        p.Disconnect();
        //        //onPlayerDisconnected(p);
        //    }

        //    MapList.Clear();
        //}
        void Mapwrk()// processes tick
        {
            do
            {               

                //foreach (var map in MapList.Values.ToList())
                //    map.UpdateMap();
                Thread.Sleep(2);
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
        #endregion

        //public Player GetPlayer(uint ID)
        //{

        //    foreach (var t in Players.Values)
        //            if (t.ID == ID)
        //                return t;

        //    return null; 
        //}
        /// <summary>
        /// Checks by dataBaseID if Online
        /// </summary>
        /// <param CharacterName="ID"></param>
        /// <returns></returns>
        public bool isOnline(uint Databaseid)
        {
            //foreach (var t in Players)
            //        if (t.DataBaseID == Databaseid && t.State != PlayerState.Connected_LoginWindow)
            //            return true;

            return false;
        }
        /// <summary>
        /// Disconnects a player
        /// </summary>
        /// <param CharacterName="id"></param>
        public void DisconnectPlayer(uint Databaseid)
        {
            //foreach (var r in Players)
            //        if (r.DataBaseID == Databaseid) { r.Disconnect(); return; }
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
        public void Broadcast(SendPacket pkt)
        {
            foreach (var p in Players)
                p.Send(pkt);
        }
        /// <summary>
        /// Broadcast's a direct packet or a packet that will ignore the selcected
        /// </summary>
        /// <param CharacterName="pkt"></param>
        /// <param CharacterName="directTo">Person to send to/ or avoid</param>
        /// <param CharacterName="avoid">Avoid the Person and send to everyone else</param>
        public void Broadcast(SendPacket pkt, uint? directTo = null, bool exclude = false)
        {
            //foreach (var p in Players)
            //        if (p.ID == directTo && exclude)
            //        continue;
            //        else if (p.ID == directTo && !exclude)
            //    {
            //        p.Send(pkt); return;
            //    }
            //        else if (exclude && p.ID != directTo)
            //        p.Send(pkt);
        }

        //public override void onPlayerDisconnected(ref Player src)
        //{
        //    lock (mylock)
        //    {
        //        //base.onPlayerDisconnected(ref src);
        //        ////if (WorldEvent != null) WorldEvent(c, WorldEventType.PlayerLogoff);
        //        ////dc socket
        //        //src.Disconnect();

        //        ////unlock CharacterName
        //        //cGlobal.gCharacterDataBase.unLockName(src.CharacterName);
        //        ////send dc packet
        //        //SendPacket bye = new SendPacket();
        //        //bye.Pack(new byte[] { 1, 1 });
        //        //bye.Pack(src.ID);
        //        //BroadcastTo(bye, src.ID, true);
        //        ////save data
        //        //if (cGlobal.gCharacterDataBase.WritePlayer(src.ID, src))
        //        //    DebugSystem.Write(src.UserName + " Info has been Fully Saved");

        //        //DebugSystem.Write(String.Format("Client {0} {1} Disconnected.", src.ClientIP + ":" + src.ClientPort, src.UserName));
        //    }
        //}

        //public void SendCurrentPlayers(Player to)
        //{
        //    //SendPacket p = new SendPacket();
        //    //foreach (var y in (from c in Assembly.GetExecutingAssembly().GetTypes()
        //    //                   where c.IsClass && c.IsSubclassOf(typeof(Bots.GmBot))
        //    //                   select c))
        //    //{
        //    //    var c = (Activator.CreateInstance(y) as Bots.GmBot);
        //    //    p = new SendPacket();
        //    //    p.Pack(new byte[] { 4 });
        //    //    p.Pack(c.ID);
        //    //    p.Pack((byte)c.Body); //body style
        //    //    p.Pack((byte)c.Element); //element
        //    //    p.Pack(c.Level); //level
        //    //    p.Pack((c.CurrentMap == null) ? c.LoginMap : c.CurrentMap.MapID); //map id
        //    //    p.Pack(c.X); //x
        //    //    p.Pack(c.Y); //y
        //    //    p.Pack(0); p.Pack(c.Head); p.Pack(0);
        //    //    p.Pack(c.HairColor);
        //    //    p.Pack(c.SkinColor);
        //    //    p.Pack(c.ClothingColor);
        //    //    p.Pack(c.EyeColor);
        //    //    p.Pack(c.WornCount);//clothesAmmt); // ammt of clothes
        //    //    p.Pack(c.Worn_Equips);
        //    //    p.Pack(0); p.Pack(0); //??
        //    //    p.Pack(c.Reborn); //is rebirth
        //    //    p.Pack((byte)c.Job); //rb class
        //    //    p.Pack(c.CharacterName);//(BYTE*)c.CharacterName,c.nameLen); //CharacterName
        //    //    p.Pack(c.Nickname);//(BYTE*)c.nick,c.nickLen); //nickname
        //    //    p.Pack(255); //??
        //    //    to.Send(p);
        //    //}
        //    //foreach (var d in Players.Where(c => c.inGame))
        //    //{
        //    //    Character c = d;
        //    //    p = new SendPacket();
        //    //    p.Pack(new byte[] { 4 });
        //    //    p.Pack(d.ID);
        //    //    p.Pack((byte)c.Body); //body style
        //    //    p.Pack((byte)c.Element); //element
        //    //    p.Pack(c.Level); //level
        //    //    p.Pack(c.CurrentMap.MapID); //map id
        //    //    p.Pack(c.X); //x
        //    //    p.Pack(c.Y); //y
        //    //    p.Pack(0); p.Pack(c.Head); p.Pack(0);
        //    //    p.Pack(c.HairColor);
        //    //    p.Pack(c.SkinColor);
        //    //    p.Pack(c.ClothingColor);
        //    //    p.Pack(c.EyeColor);
        //    //    p.Pack(c.WornCount);//clothesAmmt); // ammt of clothes
        //    //    p.Pack(c.Worn_Equips);
        //    //    p.Pack(0); p.Pack(0); //??
        //    //    p.Pack(c.Reborn); //is rebirth
        //    //    p.Pack((byte)c.Job); //rb class
        //    //    p.Pack(c.CharacterName);//(BYTE*)c.CharacterName,c.nameLen); //CharacterName
        //    //    p.Pack(c.Nickname);//(BYTE*)c.nick,c.nickLen); //nickname
        //    //    p.Pack(255); //??
        //    //    to.Send(p);
        //    //    to.onPlayerLogin(c.ID);
        //    //    d.onPlayerLogin(to.ID);
        //    //}
        //}    
    }
}
