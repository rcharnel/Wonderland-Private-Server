using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Game;
using Game.Maps;
using System.Collections.Concurrent;
using RCLibrary.Networking;


namespace Server
{
    public class WloWorldNode
    {
        Thread m_thrd;
        readonly AsyncLock listlock = new AsyncLock();C:\Users\Rommel JR\Documents\Projects\Wonderland-Private-Server\Src\Server\Network\
        readonly AsyncLock loginlock = new AsyncLock();
        bool Killnow;

        WloSocketListener m_listerner;

        List<Player> m_localPlayerlist, m_externalPlayerlist;

        ConcurrentStack<Player> ConnectedQueue, DisconnectedQueue;

        List<Game.Maps.GameMap> Maplist = new List<Game.Maps.GameMap>();


        public WloWorldNode()
        {
            m_listerner = new WloSocketListener(6414);
            m_localPlayerlist = new List<Player>();
            m_externalPlayerlist = new List<Player>();
            ConnectedQueue = new ConcurrentStack<Player>();
            DisconnectedQueue = new ConcurrentStack<Player>();
        }

        #region Events
        void c_onNewClient(object sender, WloClient e)
        {
            ConnectedQueue.Push(new Player(ref e));
        }
        void onDisconnectedClient(Player src)
        {
            DisconnectedQueue.Push(src);
        }

        public event Action<Player[]> onNewPlayer;//for Form
        public event Action<Player> onPlayerDced;//for Form
        #endregion

        public void Open()
        {
            ConnectedQueue = new ConcurrentStack<Player>();
            DisconnectedQueue = new ConcurrentStack<Player>();
            DebugSystem.Write("Starting Wlo Game World");
            DebugSystem.Write("Created by IloveWlo Dev Team");
            //GameEvents = new EventSystem();
            //GameEvents.Start();

            m_thrd = new Thread(new ThreadStart(MainMapLoop));
            m_thrd.IsBackground = true;
            m_thrd.Init();
            
            DebugSystem.Write("Starting TCP Listener");
            m_listerner.onNewClient += c_onNewClient;
            m_listerner.Initialize();
        }

        public void Close(bool force = false)
        {
            DebugSystem.Write("Stopping Listener");
            m_listerner.Kill();
            DebugSystem.Write("Disconnecting Players");
            m_localPlayerlist.AsParallel().ForAll(c => c.Disconnect());
            DebugSystem.Write("Waiting for Players to properly disconnect");
            while (m_localPlayerlist.Count > 0)
                Thread.Sleep(1);
            DebugSystem.Write("Clearing External Players");
            m_externalPlayerlist.Clear();

            //GameEvents.Stop();
            Killnow = true;
        }

        /// <summary>
        /// If the Server is Currently Running
        /// </summary>
        public bool isRunning { get { return (m_thrd != null && m_thrd.IsAlive); } }
        /// <summary>
        /// If the Server is Listening for new connections
        /// </summary>
        public bool isListening { get { return m_listerner.IsRunning(); } }


        void MainLoop()
        {
            

            do
            {
            //    int a = 20;

            //    while (ConnectedQueue.Count > 0 && (--a) > 0)
            //        ConnectedPlayers.Add(ConnectedQueue.Dequeue());

            //    foreach (var curplayer in ConnectedPlayers)
            //        if (curplayer.IdleTimer() > new TimeSpan(0, 5, 0))
            //        {
            //            DebugSystem.Write(System.Drawing.Color.FromArgb(255, 128, 255), String.Format("Client {0} timed out at Login.", curplayer.SockAddress()));
            //            curplayer.Disconnect();
            //            DisconnectedQueue.Enqueue(curplayer);
            //        }
            //        else if (curplayer.isDisconnected())
            //        {
            //            DebugSystem.Write(System.Drawing.Color.FromArgb(255, 128, 255), String.Format("Client {0} has Disconnected.", curplayer.SockAddress()));
            //            curplayer.Disconnect();
            //            DisconnectedQueue.Enqueue(curplayer);
            //        }

            Thread.Sleep(5);
            }
            while (!Killnow);
        }

        void MainMapLoop()
        {

            ParallelOptions options = new ParallelOptions();

            do
            {
                Player[] pitems = new Game.Player[50];

                if (DisconnectedQueue.TryPopRange(pitems, 0, 50) > 0)
                {
                    Parallel.ForEach(pitems.Where(c => c != null).ToList(), src =>
                    {
                        m_localPlayerlist.Remove(src);
                        DebugSystem.Write(String.Format("Client {0} has Disconnected.", src.SockAddress()));
                    });
                }

                if (ConnectedQueue.TryPopRange(pitems, 0, 50) > 0)
                {
                    try
                    {
                        pitems.Where(c => c != null).AsParallel().ForAll(p => p.OnDisconnect = onDisconnectedClient);
                        m_localPlayerlist.AddRange(pitems.Where(c => c != null).ToArray());
                        onNewPlayer(pitems.Where(c => c != null).ToArray());
                    }
                    catch (Exception f) { DebugSystem.Write(new ExceptionData(f)); }
                   
                }

                try
                {
                    options.MaxDegreeOfParallelism = 1 + (Maplist.Count / 2);
                    Parallel.ForEach(Maplist, options, curMap => { curMap.Process(); });
                }
                catch (AggregateException e)
                {
                    foreach (Exception f in e.InnerExceptions)
                        DebugSystem.Write(new ExceptionData(f));
                }
                Thread.Sleep(1);
            }
            while (!Killnow);

            Parallel.ForEach(Maplist, curMap => curMap.Dispose());
        }

        /// <summary>
        /// Broadcasts a packet to all who are in a Map
        /// </summary>
        /// <param CharacterName="pkt"></param>
        public void Broadcast(SendPacket pkt)
        {
            Broadcast(pkt, "ALL");
        }
        /// <summary>
        /// Broadcasts a SendPacket to certain people who are in a Map
        /// </summary>
        /// <param name="pkt"></param>
        /// <param name="To">"Multiple target IDs as string to send to specific people"</param>
        public void Broadcast(SendPacket pkt, string parameter, params object[] To)
        {
            Maplist.AsParallel().ForAll(c => c.Broadcast(pkt, parameter, To));
        }

        /// <summary>
        /// Searches for a Player and invokes the corresponding action
        /// </summary>
        /// <param name="src"></param>
        /// <param name="work"></param>
        /// <returns>true if succeeded or false if it failed</returns>
        public bool DoAction(uint src,Action<Player> work)
        {
            try
            {
                if (cGlobal.gGameDataBase.isOnline((int)src))
                {
                    work.Invoke(m_localPlayerlist.Single(c => c.CharID == src));
                    return true;
                }
            }
            catch (Exception r) { DebugSystem.Write(new ExceptionData(r)); }
            return false;
        }

        public async void onTelePort(TeleportType teletype, byte portalID, Game.Maps.WarpData map, Player target)
        {
            using (var releaser = await listlock.LockAsync())
            {
                if (Maplist.Count(c => c.MapID == map.DstMap) == 0)
                {
                    //Create Map
                    Game.Maps.GameMap tmp = cGlobal.gMapManager.GetMap(map.DstMap);
                    //cGlobal.gGameDataBase.SetupMap(ref tmp);
                    Maplist.Add(tmp);
                }


                Maplist.Single(c => c.MapID == map.DstMap).Teleport(teletype, target, portalID, map);
                DebugSystem.Write(DebugItemType.Info_Heavy,"Loaded Map {0}",  map.DstMap);
            }
        }

        public async void CommenceLogin(Player src)
        {

            src.Flags.Add(PlayerFlag.Logging_into_Map);

            src.SendPacket(SendPacket.FromFormat("bb", 20, 8));
            src.SendPacket(SendPacket.FromFormat("bbbw", 24, 5, 183, 0));
            src.SendPacket(SendPacket.FromFormat("bbbw", 24, 5, 53, 0));
            src.SendPacket(SendPacket.FromFormat("bbbw", 24, 5, 52, 0));
            src.SendPacket(SendPacket.FromFormat("bbbw", 24, 5, 54, 0));
            src.SendPacket(SendPacket.FromFormat("bbbswb", 70, 1, 23, "Something", 194, 0));
            src.SendPacket(SendPacket.FromFormat("bbb", 20, 33, 0));
            //-----Player Stats values--------
            src.Send8_1(false);
            Thread.Sleep(5);
            src.SendPacket(SendPacket.FromFormat("bbb", 14, 13, 3));
            //-----Im Mall List
            // //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
            src.SendPacket(SendPacket.FromFormat("bbw", 75, 8, 0));
            SendPacket d = new SendPacket(new byte[] { 244, 68, 41, 0, 104, 1, 1, 0, 12, 44, 137, 1, 45, 137, 1, 25, 134, 1, 24, 134, 1, 22, 134, 1, 23, 134, 1, 76, 133, 1, 99, 133, 1, 100, 133, 1, 41, 133, 1, 91, 133, 1, 88, 133, 1 });

            src.SendPacket(d);

            using (var releaser = await loginlock.LockAsync())
            {
            //------Player Base Info------------------
            await cGlobal.gGameDataBase.LoadPlayerData(src);
            src.Send_3_Me();
            cGlobal.gGameDataBase.SendOnlineCharacters(src);
            //-----------send sidebar---------------------
            //------------Player Data---------------------
            src.Send_5_3();
            src.SendPacket(SendPacket.FromArray(src.Inv.GetAC23_5()));
            src.SendPacket(SendPacket.FromArray(src._23_11Data));
            //-------------------------Quest----------------------
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
            src.SendPacket(SendPacket.FromFormat("bbd", 26, 4, src.Gold));
            src.SendPacket(SendPacket.FromArray(src.Settings.ToArray()));
            src.MyFriends.SendFriendList();

            // //pets
            // //-----------------------------------   
            //---------Warp Info---------------------------------------------------
            // //put me in my maps list
            
                src.Flags.Add(PlayerFlag.Warping);
                onTelePort(TeleportType.Login, 0, new WarpData() { DstMap = src.LoginMap, DstX_Axis = src.CurX, DstY_Axis = src.CurY }, src);
            }
            src.SendPacket(SendPacket.FromFormat("bbb", 5, 15, 0));
            src.SendPacket(SendPacket.FromFormat("bbw", 62, 53, 2));
            src.SendPacket(SendPacket.FromFormat("bbb", 5, 21, src.Slot));
            src.SendPacket(SendPacket.FromFormat("bbdw", 5, 11, 15085, 5000));
            // //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0, 
            //---------------------------------
            //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items
            //--------------------------------------
            src.SendPacket(SendPacket.FromFormat("bbb", 5, 14, 2));
            src.SendPacket(SendPacket.FromFormat("bbb", 5, 16, 0));
            src.SendPacket(SendPacket.FromFormat("bbbl", 23, 140, 3, DateTime.Now.ToOADate()));
            src.SendPacket(SendPacket.FromFormat("bbbl", 25, 44, 2, DateTime.Now.ToOADate()));
            // //g.ac23.Send_106(1, 1);
            src.SendPacket(SendPacket.FromFormat("bbb", 23, 160, 3));
            src.SendPacket(SendPacket.FromFormat("bbb", 75, 7, 1));
            src.SendPacket(SendPacket.FromFormat("bbbs", 23, 57, 0, "Welcome to the  WLO 4 EVER Community Server :! Enjoy !!"));
            src.SendPacket(SendPacket.FromFormat("bbb", 69, 1, 71));
            src.SendPacket(SendPacket.FromFormat("bbb", 20, 60, 1));
            src.SendPacket(SendPacket.FromArray(new byte[] { 66, 1, 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 }));

            for (byte a = 1; a < 11; a++)
                src.SendPacket(SendPacket.FromFormat("bbbw", 5, 13, a, 0));
            for (byte a = 1; a < 11; a++)
                src.SendPacket(SendPacket.FromFormat("bbbw", 5, 24, a, 0));

            src.SendPacket(SendPacket.FromFormat("bbbw", 23, 162, 2, 0));
            src.SendPacket(SendPacket.FromFormat("bbd", 26, 10, 0));
            src.SendPacket(SendPacket.FromFormat("bbw", 23, 204, 1));
            src.SendPacket(SendPacket.FromFormat("bbbbd", 23, 208, 2, 3, 0));
            src.SendPacket(SendPacket.FromFormat("bbbbd", 23, 208, 2, 4, 0));
            src.SendPacket(SendPacket.FromFormat("bb", 1, 11));
            src.SendPacket(SendPacket.FromFormat("bbbbbb", 15, 19, 4, 6, 9, 94));
            src.SendPacket(SendPacket.FromArray(new byte[] { 54, 89, 2, 2, 90, 2, 1, 91, 2, 1, 189, 2, 2, 190, 2, 1, 191, 2, 1 }));
            src.SendPacket(SendPacket.FromFormat("bbdddd", 35, 4, 0, 0, 0, 0));//first 0 is im
            src.SendPacket(SendPacket.FromFormat("bbbbbb", 90, 1, 0, 2, 2, 3));
            src.SendPacket(SendPacket.FromFormat("bb", 5, 4));
            //src.SetSendMode(SendMode.Normal);
            src.Flags.Add(PlayerFlag.InMap);


        }
    }
}
