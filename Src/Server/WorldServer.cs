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
using Game.Code;
using Game.Maps;
using Network;
using Plugin;
using RCLibrary.Core.Networking;

namespace Server
{
    /// <summary>
    /// Handles the Recv and SendPacket Proccesing of all clients
    /// </summary>
    public class WorldServer:MapSystem,WorldServerHost
    {
        Thread Mainthrd,Eventthrd;
        bool killFlag;
        readonly ManualResetEvent mylock;
        //readonly Semaphore ProcessLock,SendLock;
        //public event onWorldEvent WorldEvent;
        //event wOnlineCheck OnlineCheck;
        //event wDCPlayerEvent DCPlayer;

        /// <summary>
        /// List of Players
        /// </summary>
        Queue<Player> QueuedPlayerLogin;


        /// <summary>
        /// Maps loaded into the world
        /// </summary>
        ConcurrentDictionary<ushort, GameMap> MapList;
               
        // System.Diagnostics.Stopwatch exptimer = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// Clients that are in the Process of Logging into the Server
        /// </summary>
        //public int Clients_Connected { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count); return a; } }
        ///// <summary>
        ///// Clients playing in the Server
        ///// </summary>
        //public int Clients_inGame { get { int a = 0; ConnectedPlayers.Values.ToList().ForEach(c => a += c.Values.Count(n => n.inGame)); return a; } }

        public WorldServer(PluginManager src):base(src)
        {
            mylock = new ManualResetEvent(false);
            QueuedPlayerLogin = new Queue<Player>();
            MapList = new ConcurrentDictionary<ushort, GameMap>();
        }
        
        public void OnLogin(Player client)
        {
            QueuedPlayerLogin.Enqueue(client);
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
            while (Eventthrd != null && Eventthrd.IsAlive) { Thread.Sleep(1); }
            Eventthrd = null;
        }

        void MainLoop()
        {
            Queue<Player> DeadClients = new Queue<Player>();

            do
            {
                #region Queued Clients


                if (QueuedPlayerLogin.Count > 0)
                {
                    Player src;

                    if (!(src = QueuedPlayerLogin.Dequeue()).isDisconnected())
                        CommenceLogin(src);
                }
                #endregion
                Thread.Sleep(2);
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
        public bool onTelePort(TeleportType teletype, byte portalID, WarpData map, Player target)
        {
             
            if (MapList.Values.Count(c => c.MapID == map.DstMap) == 0)
            {
                //Create Map
                GameMap tmp = null;
                if ((tmp = GetMap(map.DstMap)) != null)
                {
                    //cGlobal.gGameDataBase.SetupMap(ref tmp);
                    if (MapList.TryAdd((ushort)tmp.MapID, tmp))
                        DebugSystem.Write(DebugItemType.Info_Heavy, "Loaded Map {0}", tmp.MapID);
                }
                else
                    return false;
            }

            MapList.Values.Single(c => c.MapID == map.DstMap).Teleport(teletype, target, portalID, map);
            return true;
        }
       
        /// <summary>
        /// Broadcasts a packet to all
        /// </summary>
        /// <param CharacterName="pkt"></param>
        public void Broadcast(SendPacket pkt)
        {
            //foreach (var p in Players)
            //    p.Send(pkt);
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

        public void CommenceLogin(Player src)
        {

            src.Flags.Add(PlayerFlag.Logging_into_Map);

            src.Send(SendPacket.FromFormat("bb", 20, 8));
            src.Send(SendPacket.FromFormat("bbbw", 24, 5, 183, 0));
            src.Send(SendPacket.FromFormat("bbbw", 24, 5, 53, 0));
            src.Send(SendPacket.FromFormat("bbbw", 24, 5, 52, 0));
            src.Send(SendPacket.FromFormat("bbbw", 24, 5, 54, 0));
            src.Send(SendPacket.FromFormat("bbbswb", 70, 1, 23, "Something", 194, 0));
            src.Send(SendPacket.FromFormat("bbb", 20, 33, 0));
            //-----Player Stats values--------
            src.Send8_1(false);
            Thread.Sleep(5);
            src.Send(SendPacket.FromFormat("bbb", 14, 13, 3));
            //-----Im Mall List
            // //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
            src.Send(SendPacket.FromFormat("bbw", 75, 8, 0));
            SendPacket d = new SendPacket(new byte[] { 244, 68, 41, 0, 104, 1, 1, 0, 12, 44, 137, 1, 45, 137, 1, 25, 134, 1, 24, 134, 1, 22, 134, 1, 23, 134, 1, 76, 133, 1, 99, 133, 1, 100, 133, 1, 41, 133, 1, 91, 133, 1, 88, 133, 1 });

            src.Send(d);

            //------Player Base Info------------------
            cGlobal.gGameDataBase.LoadFinalData(src);
            src.SendCharacterData();
            cGlobal.gCharacterDataBase.SendOnlineCharacters(src);
            cGlobal.gCharacterDataBase.OnCharacterJoin(src);
            src.Disconnected += cGlobal.gCharacterDataBase.OnCharacterLeave;
            //-----------send sidebar---------------------
            //------------Player Data---------------------
            src.Send_5_3();
            src.Send(new SendPacket(src.Inv.GetAC23_5()));
            src.Send(new SendPacket(src._23_11Data));
            ////SendQuest----------------------
            // SendPacket g = new SendPacket();
            // //    g.PackArray(new byte[]{(24, 6);
            // //    g.PackArray(new byte[] { 001, 008, 047, 001, 002, 244, 050, 001, 003, 012, 043, 001 });
            // //    g.SetSize();
            // //    Send(g);
            // //    g = new SPacket();
            // //    g.PackArray(new byte[]{(53, 10);
            // //    g.PackArray(new byte[] { 032, 164, 036, 002, 037, 240, 038, 041, 058, 048, 083, 015 });
            // //    g.SetSize();
            // //    Send(g);
            // //    g = new SPacket();
            // //    g.PackArray(new byte[]{(26, 7);
            // //    g.PackArray(new byte[] { 001, 002, 002, 128, 003, 002, 004, 128 ,008,
            // //                066, 009, 096, 010, 008, 011, 010 ,013, 001 });
            // //    g.SetSize();
            // //    Send(g);
            src.Send(SendPacket.FromFormat("bbd", 26, 4, src.Gold));
            src.Send(new SendPacket(src.Settings.ToArray()));
            //src.MyFriends.SendFriendList();

            // //pets
            // //-----------------------------------   
            //---------Warp Info---------------------------------------------------
            // //put me in my maps list

            src.Flags.Add(PlayerFlag.Warping);
            if (!onTelePort(TeleportType.Login, 0, new WarpData() { DstMap = src.LoginMap, DstX_Axis = src.CurX, DstY_Axis = src.CurY }, src))
            {
                var ex = new Exception("Map " + src.LoginMap + " not found for player");
                DebugSystem.Write(new ExceptionData(ex));
                throw ex;
            }

            src.Send(SendPacket.FromFormat("bbb", 5, 15, 0));
            src.Send(SendPacket.FromFormat("bbw", 62, 53, 2));
            src.Send(SendPacket.FromFormat("bbb", 5, 21, src.Slot));
            src.Send(SendPacket.FromFormat("bbdw", 5, 11, 15085, 5000));
            // //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0, 
            //---------------------------------
            //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items
            //--------------------------------------
            src.Send(SendPacket.FromFormat("bbb", 5, 14, 2));
            src.Send(SendPacket.FromFormat("bbb", 5, 16, 0));
            src.Send(SendPacket.FromFormat("bbbl", 23, 140, 3, DateTime.Now.ToOADate()));
            src.Send(SendPacket.FromFormat("bbbl", 25, 44, 2, DateTime.Now.ToOADate()));
            // //g.ac23.Send_106(1, 1);
            src.Send(SendPacket.FromFormat("bbb", 23, 160, 3));
            src.Send(SendPacket.FromFormat("bbb", 75, 7, 1));
            src.Send(SendPacket.FromFormat("bbbs", 23, 57, 0, "Welcome to the  WLO 4 EVER Community Server :! Enjoy !!"));
            src.Send(SendPacket.FromFormat("bbb", 69, 1, 71));
            src.Send(SendPacket.FromFormat("bbb", 20, 60, 1));
            src.Send(new SendPacket(new byte[] { 244,68,13,0,66, 1, 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 }));

            for (byte a = 1; a < 11; a++)
                src.Send(SendPacket.FromFormat("bbbw", 5, 13, a, 0));
            for (byte a = 1; a < 11; a++)
                src.Send(SendPacket.FromFormat("bbbw", 5, 24, a, 0));

            src.Send(SendPacket.FromFormat("bbbw", 23, 162, 2, 0));
            src.Send(SendPacket.FromFormat("bbd", 26, 10, 0));
            src.Send(SendPacket.FromFormat("bbw", 23, 204, 1));
            src.Send(SendPacket.FromFormat("bbbbd", 23, 208, 2, 3, 0));
            src.Send(SendPacket.FromFormat("bbbbd", 23, 208, 2, 4, 0));
            src.Send(SendPacket.FromFormat("bb", 1, 11));
            src.Send(SendPacket.FromFormat("bbbbbb", 15, 19, 4, 6, 9, 94));
            src.Send(new SendPacket(new byte[] { 244,68,19,0,54, 89, 2, 2, 90, 2, 1, 91, 2, 1, 189, 2, 2, 190, 2, 1, 191, 2, 1 }));
            src.Send(SendPacket.FromFormat("bbdddd", 35, 4, 0, 0, 0, 0));//first 0 is im
            src.Send(SendPacket.FromFormat("bbbbbb", 90, 1, 0, 2, 2, 3));
            src.Send(SendPacket.FromFormat("bb", 5, 4));
            //src.SetSendMode(SendMode.Normal);
            src.Flags.Add(PlayerFlag.InMap);


        }
    }
}
