using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using DataFiles;
using RCLibrary.Core.Networking;
using Network;
using Game;
using Game.Code;
using Game.Maps;
using System.Reflection;

namespace Game
{
    public interface IMap
    {
        uint MapID { get; set; }
        MapType Type { get; }
        //void Broadcast(SendPacket pkt);
        //void Broadcast(SendPacket pkt, string parameter, params object[] To);
        bool Teleport(TeleportType teletype, Player sender, byte portalID, WarpData warp = null);
    }

    public class GameMap : Plugin.PluginObj, IDisposable, IMap
    {
        readonly object mlock = new object();

        Queue<Task> QueuedTasks = new Queue<Task>(252);

        protected List<Player> m_playerlist;
        protected List<Item> ItemsDropped;
        //protected ConcurrentDictionary<int, Battle> Battles;
        //protected ConcurrentDictionary<uint, Tent> Tents;

        protected Queue<Player> DisconnectedQueue;
        protected Queue<KeyValuePair<DateTime, Action>> WaitingtoLogin;

        protected uint m_mapid;
        protected string m_name;

        bool shutdown = false;



        public GameMap()
        {
            m_playerlist = new List<Player>();
            ItemsDropped = new List<Item>(255);
            DisconnectedQueue = new Queue<Player>(50);
            WaitingtoLogin = new Queue<KeyValuePair<DateTime, Action>>(105);
            //Tents = new ConcurrentDictionary<uint, Tent>();
            //Battles = new ConcurrentDictionary<int, Battle>();
        }
        public GameMap(Plugin.PluginHost host, System.IO.FileInfo src)
            : base(src)
        {
            myhost = (Plugin.PluginHost)host;
            m_playerlist = new List<Player>();
            ItemsDropped = new List<Item>(255);
            DisconnectedQueue = new Queue<Player>(50);
            WaitingtoLogin = new Queue<KeyValuePair<DateTime, Action>>(105);
            //Tents = new ConcurrentDictionary<uint, Tent>();
            //Battles = new ConcurrentDictionary<int, Battle>();
            LoadData();

        }


        /// <summary>
        /// Called to Load Additional Data contained within the individual maps
        /// </summary>
        protected virtual void LoadData()
        {
            DebugSystem.Write("["+Assembly.GetAssembly(this.GetType()).FullName+"] - Initializing Map " + MapID + " - " + MapName);
            #region load Interactable Objects for this map

            //load data from data files


            //foreach (var y in (from c in myDllAssembly.GetTypes()
            //                   where c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(InteractableObj))
            //                   select c))
            //{
            //    InteractableObj m = null;

            //    try
            //    {
            //        m = (Activator.CreateInstance(y) as InteractableObj);
            //        if (MapObjects.ContainsKey(m.clickID))
            //            MapObjects[m.clickID].DataOverride = m;
            //    }
            //    catch { DLogger.DllError((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "LoadData", new Exception("failed to load Map Object " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
            //}

            //DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + MapObjects.Count + " Map Objects");
            //#endregion

            //#region load Warp Destinations for this map

            //foreach (var y in (from c in myDllAssembly.GetTypes()
            //                   where c.IsClass && c.IsPublic && typeof(WarpData).IsAssignableFrom(c)
            //                   select c))
            //{
            //    WarpData m = null;
            //    try
            //    {
            //        m = (Activator.CreateInstance(y) as WarpData);
            //    }
            //    catch { DLogger.DllError("PserverDataPlugin", "Map.cs LoadData", new Exception("failed to load Warp Destination " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
            //    if (!Destinations.ContainsKey((byte)m.ID))
            //        Destinations.Add((byte)m.ID, m);

            //}
            //DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + Destinations.Count + " Warp Destinations");
            //#endregion

            //#region load Warp Portals for this map

            //foreach (var y in (from c in myDllAssembly.GetTypes()
            //                   where c.IsClass && c.IsPublic && typeof(WarpPortal).IsAssignableFrom(c)
            //                   select c))
            //{
            //    WarpPortal m = null;
            //    try
            //    {
            //        m = (Activator.CreateInstance(y) as WarpPortal);
            //    }
            //    catch { DLogger.DllError("PserverDataPlugin", "Map.cs LoadData", new Exception("failed to load Warp Portal " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
            //    if (!Portals.ContainsKey((byte)m.ID))
            //        Portals.Add((byte)m.ID, m);
            //}

            //DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + Portals.Count + " Warp Portals");
            #endregion

        }

        #region Properties
        public virtual MapType Type { get { return MapType.RegularMap; } }
        public virtual uint MapID { get { lock (mlock) return m_mapid; } set { lock (mlock)m_mapid = value; } }
        public virtual string MapName { get { return ""; } }
        #endregion

        public void Dispose()
        {
        }

        public async void Process()
        {
            try
            {
                //Parallel.ForEach(m_playerlist, player =>
                //{
                //    if (player.IdleTimer() > new TimeSpan(0, 30, 0) || player.isDisconnected())
                //    {
                //        player.Disconnect();
                //        DisconnectedQueue.Enqueue(player);
                //    }
                //    else
                //        player.ProcessSocket();
                //});
            }
            catch { }

            #region Task Handling
            if (QueuedTasks.Count > 0)
            {
                if (QueuedTasks.Peek().IsCompleted)
                {
                    DebugSystem.Write(DebugItemType.Info_Heavy, "Map {0} Task ID: {1} " + ((!QueuedTasks.Peek().IsFaulted) ? "has completed successfully" : "has faulted with Exception " + string.Join(",", QueuedTasks.Peek().Exception.InnerExceptions)), MapID, QueuedTasks.Peek().Id);

                    if (QueuedTasks.Peek().IsFaulted)
                        DebugSystem.Write(new ExceptionData(QueuedTasks.Peek().Exception));
                    QueuedTasks.Dequeue();
                }
                else if (QueuedTasks.Peek().Status == TaskStatus.Created)
                    QueuedTasks.Peek().Start();
                else if (shutdown && QueuedTasks.Peek().Status != TaskStatus.Created)
                    QueuedTasks.Enqueue(QueuedTasks.Dequeue());
            }
            #endregion

            if (WaitingtoLogin.Count > 0 && WaitingtoLogin.Peek().Key < DateTime.Now)
                WaitingtoLogin.Dequeue().Value.Invoke();

            while (DisconnectedQueue.Count > 0)
            {
                var p = DisconnectedQueue.Dequeue();
                m_playerlist.Remove(p);
            }
        }

        #region Events/Funcs/Actions
        public Action<byte, byte> onItemDropped_fromMap;
        public Func<Item, bool> onItemPickup_fromMap;

        #endregion

        #region Item

        public void onItemDrop(Player src, byte loc, byte ammt)
        {
            Task dropItem = new Task(() =>
            {
                byte amt = ammt;
                //send Request to Inv to drop item

                if (src.Inv[loc].ItemID > 0)
                {
                    //if item does not equal null, inv has an item that needs to be dropped in map
                    //if item is not inv will send destroy packet.

                    var rand = new Random();
                    int cnt = 0;

                    for (int a = 0; a < 256; a++)
                    {
                        if (cnt < amt && ItemsDropped[a].ItemID == 0)
                        {
                            DroppedItem gi = new DroppedItem();
                            gi.CopyFrom(src.Inv[loc]);
                            gi.Ammt = 1;
                            gi.X = src.CurX;
                            gi.Y = src.CurY;
                            //gi..StartCountDown();


                            //src.SendPacket(SendPacket.FromFormat("bbwwwdb", 23, 3, gi.ItemID, gi.DropX, gi.DropY, 0, 1));
                            //Broadcast(SendPacket.FromFormat("bbwwwdb", 23, 3, gi.ItemID, gi.DropX, gi.DropY, 0, 0), "Ex", src.CharID);
                            cnt++;
                        }
                        else if (cnt >= amt)
                            break;
                    }
                    onItemDropped_fromMap(loc, (byte)cnt);
                }
            }, TaskCreationOptions.PreferFairness);
            QueuedTasks.Enqueue(dropItem);
        }
        public void onItemPickup(Player src, byte pos)
        {
            Task dropItem = new Task(() =>
            {
                byte loc = pos;
                if (ItemsDropped[loc - 1].ItemID > 0)
                {
                    Item res = new Item();
                    res.CopyFrom(ItemsDropped[loc - 1]);
                    ItemsDropped[loc - 1].Clear();

                    if (onItemPickup_fromMap(res))
                    {
                        //foreach (ItemsinMapEntries o in ItemAreas)
                        //{
                        //    if (DroppedItems[itemIndex].entryid == o.clickID && DroppedItems[itemIndex].Respawns)
                        //    {
                        //        o.pickedup = true;
                        //        o.dropin = DateTime.Now.AddMinutes(2);
                        //    }
                        //}
                        src.Send(new SendPacket(Packet.FromFormat("bbwb", 23, 2, res.ItemID, 1)));
                        Broadcast(new SendPacket(Packet.FromFormat("bbwb", 23, 2, res.ItemID, 0)), "Ex", src.CharID);
                    }
                }
            });
            QueuedTasks.Enqueue(dropItem);
        }
        #endregion

        #region Warping

        async void onLogin(Player player)
        {
            DebugSystem.Write(DebugItemType.Info_Heavy, "onLogin Task started at {0} for {1}", DateTime.Now.ToShortTimeString(), player.CharName);
            //cGlobal.gGameDataBase.onPlayerLogin(player);

            player.CurMap = this;
            player.Flags.Remove(PlayerFlag.Logging_into_Map);

            if (!m_playerlist.Contains(player))
                m_playerlist.Add(player);

            foreach (var r in m_playerlist)
            {
                if (r != player)
                {
                    //send to them
                    Broadcast(player.ToAC3Packet(), "Ex", player.CharID);
                    SendPacket p = new SendPacket();
                    p.Pack8((byte)5);
                    p.Pack8((byte)0);
                    p.Pack32(player.CharID);
                    p.PackArray(player.Worn_Equips);
                    p.SetHeader();
                    Broadcast(p);
                    p = new SendPacket();
                    p.Pack8((byte)10);
                    p.Pack8((byte)3);
                    p.Pack32(player.CharID);
                    p.Pack8((byte)255);
                    p.SetHeader();
                    Broadcast(p);
                    p = new SendPacket();
                    p.Pack8((byte)5);
                    p.Pack8((byte)8);
                    p.Pack32(player.CharID);
                    p.Pack8((byte)0);
                    p.SetHeader();
                    Broadcast(p);

                    //send to me
                    p = new SendPacket();
                    p.Pack8((byte)7);
                    p.Pack32(r.CharID);
                    p.Pack16((ushort)MapID);
                    p.Pack16(r.CurX);
                    p.Pack16(r.CurY);
                    p.SetHeader();
                    player.Send(p);
                    p = new SendPacket();
                    p.Pack8((byte)5);
                    p.Pack8((byte)0);
                    p.Pack32(r.CharID);
                    p.PackArray(r.Worn_Equips);
                    p.SetHeader();
                    player.Send(p);
                }

            }
            SendMapInfo(player, true);

        }

        async protected virtual void onWarp_In(byte portalID, Player src, WarpData from)
        {

            src.CurMap = this;

                if (!m_playerlist.Contains(src))
                    m_playerlist.Add(src);

            SendAc12(src, portalID, from);

            foreach (var r in m_playerlist)
            {
                if (r != src)
                {
                    //send to them
                    SendPacket p = new SendPacket();
                    p.Pack8((byte)5);
                    p.Pack8((byte)0);
                    p.Pack32(src.CharID);
                    p.PackArray(src.Worn_Equips);
                    p.SetHeader();
                    r.Send(p);
                    p = new SendPacket();
                    p.Pack8((byte)10);
                    p.Pack8((byte)3);
                    p.Pack32(src.CharID);
                    p.Pack8((byte)255);
                    p.SetHeader();
                    r.Send(p);//maybe guild info???

                    //send to me
                    p = new SendPacket();
                    p.Pack8((byte)7);
                    p.Pack32(r.CharID);
                    p.Pack16((ushort)MapID);
                    p.Pack32(r.CurX);
                    p.Pack32(r.CurY);
                    p.SetHeader();
                    src.Send(p);
                    p = new SendPacket();
                    p.Pack8(5);
                    p.Pack8(0);
                    p.Pack32(r.CharID);
                    p.PackArray(r.Worn_Equips);
                    p.SetHeader();
                    src.Send(p);
                }
            }
            SendMapInfo(src);
        }

        async protected virtual void onWarp_Out(byte portalID, Player src, WarpData To, bool toTent)
        {
            //src.PrevMap = new WarpData();
            //src.PrevMap.DstMap = (ushort)MapID;
            //src.PrevMap.DstX_Axis = src.CurX;
            //src.PrevMap.DstY_Axis = src.CurY;

            SendAc12(src, portalID, To, toTent);
                m_playerlist.Remove(src);
        }

        public virtual bool Teleport(TeleportType teletype, Player sender, byte portalID, WarpData warp = null)
        {
            if (teletype == TeleportType.Regular || teletype == TeleportType.CmD)
                sender.Send(new SendPacket(Packet.FromFormat("bb",20,7)));

            SendPacket tmp = new SendPacket();
            tmp.Pack8((byte)23);
            tmp.Pack8((byte)32);
            tmp.Pack32(sender.CharID);
            tmp.SetHeader();
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.Pack8((byte)23);
            tmp.Pack8((byte)112);
            tmp.Pack32(sender.CharID);
            tmp.SetHeader();
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.Pack8((byte)23);
            tmp.Pack8((byte)132);
            tmp.Pack32(sender.CharID);
            tmp.SetHeader();
            sender.Send(tmp);
            sender.Flags.Add(PlayerFlag.Warping);
            onWarp_Out(portalID, sender, warp, (teletype == TeleportType.Tent));// warp out of map

            switch (teletype)
            {
                #region Regular Warp
                case TeleportType.Regular:
                    {
                        //if (TypeofMap != MapType.Regular && portalID == 1)  //create warp from Prev Map
                        //{
                        //    onWarp_Out(portalID, ref sender, sender.PrevMap, (teletype == TeleportType.Tent));// warp out of map
                        //}
                        //else
                        //{
                        //    var WarpID = (int)Events[Portals[portalID].unknownbytearray1[0]].SubEntry[0].SubEntry[0].dialog2;
                        //    if (WarpID == 0)
                        //    {

                        //        tmp = new SendPacket();
                        //        tmp.PackArray(new byte[] { 20, 8 });
                        //        sender.Send(tmp); return false;
                        //    }
                        //    onWarp_Out(portalID, ref sender, new WarpData(Destinations[(byte)WarpID]), (teletype == TeleportType.Tent));// warp out of map
                        //    cGlobal.WLO_World.onTelePort(portalID, new WarpData(Destinations[(byte)WarpID]), ref sender);
                        //}
                    } break;
                #endregion
                case TeleportType.Special:
                    {
                        //WarpInfo f = new WarpInfo();
                        //f.clickID = i.id;
                        //f.mapID = i.mapTo;
                        //f.x = i.x;
                        //f.y = i.y;
                        //globals.gServer.Multipkt_Request(t);
                        //var map = Phase1Warp(t, f, false);
                        //globals.gServer.Queue_Request(t);
                        //map.Phase2Warp(t);
                        //globals.packet.cCharacter.DatatoSend.Enqueue(globals.gServer.GenerateQueuepkt(t));
                        //globals.gServer.SendCombinepkt(t);
                    } break;
                case TeleportType.Quest:
                    {
                        //var exitpoint = mapData.Entry_Points[Entry - 1];
                        //var WarpID = (int)mapData.Events[exitpoint.unknownbytearray1[0] - 1].SubEntry[0].SubEntry[0].dialog2;
                        //globals.gServer.Queue_Request(t);

                        //var map = Phase1Warp(t, mapData.WarpLoc[WarpID - 1]);
                        //globals.packet.cCharacter.DatatoSend.Enqueue(globals.gServer.GenerateQueuepkt(t));
                        //globals.gServer.Queue_Request(t);
                        //map.Phase2Warp(t);
                        //globals.packet.cCharacter.DatatoSend.Enqueue(globals.gServer.GenerateQueuepkt(t));
                    } break;
                #region Tent Warp
                case TeleportType.Tent:
                    {
                        onWarp_Out(portalID, sender, warp, (teletype == TeleportType.Tent));// warp out of map
                        sender.CurX = warp.DstX_Axis;//switch x
                        sender.CurY = warp.DstY_Axis;//switch y
                        //Tents[warp.DstMap].onWarp_In(0, sender, warp);
                    } break;
                #endregion
                case TeleportType.Tool:/*t.inv.RemoveInv((byte)Entry, 1);*/ break;
                #region Cmd Warp
                case TeleportType.CmD:
                    {
                        //onWarp_Out(portalID, ref sender, warp, (teletype == TeleportType.Tent));// warp out of map
                        //sender.X = warp.DstX_Axis;//switch x
                        //sender.Y = warp.DstY_Axis;//switch y
                        //cGlobal.WLO_World.onTelePort(portalID, warp, ref sender);
                    } break;
                #endregion
                case TeleportType.Login: onLogin(sender); break;
            }
            return true;
        }

        protected async virtual void SendMapInfo(Player t, bool login = false)
        {
              RCLibrary.Core.Networking.PacketBuilder tmp = new RCLibrary.Core.Networking.PacketBuilder();
                tmp.Begin(null);
                tmp.Add(SendPacket.FromFormat("bb", 23, 138));
                /* p = new SendPacket();
                 p.PackArray(new byte[]{(6, 2);
                 p.Pack(1);
                 p.SetSize();
                 g.SendPacket(t, p);*/
                #region Send Npc
                #endregion
                #region Send Item
                //if (Items_Dropped.Count > 0)
                //{
                //    int unk = 0;
                //    p = new SendPacket();
                //    p.PackArray(new byte[] { 23, 4 });
                //    for (byte a = 0; a < Items_Dropped.ToList().Count; a++)
                //    {
                //        if (Items_Dropped[a].NonExpirable)
                //        {
                //            p.Pack((byte)3);
                //            p.Pack((byte)1);
                //            unk = Items_Dropped[a].Control;
                //        }

                //        p.Pack16((ushort)a);
                //        p.Pack((ushort)Items_Dropped[a].ItemID);
                //        p.Pack16((ushort)Items_Dropped[a].X);
                //        p.Pack16((ushort)Items_Dropped[a].Y);
                //        p.Pack((uint)unk);
                //    }
                //    t.Send(p);
                //}
                #endregion
                //SendNpcs(t);
                //SendItems(t);
                SendOpenTents(t);


                foreach (var r in m_playerlist)
                {
                    tmp.Add(SendPacket.FromFormat("bbd", 23, 122, r.CharID));
                    tmp.Add(SendPacket.FromFormat("bbdb", 10, 3, r.CharID, 255));

                    if (r.CharID != t.CharID)
                    {
                        //r.SendPacket(SendPacket.FromFormat("bbd", 23, 122, t.CharID));
                        //r.SendPacket(SendPacket.FromFormat("bbdb", 10, 3, t.CharID, 255));

                        if (r.Emote != 0)
                            tmp.Add(SendPacket.FromFormat("bbdb", 32, 2, r.CharID, r.Emote));

                        #region Pets in Map
                        //if (t.Pets.BattlePet != null)//to them
                        //{
                        //    SendPacket tmp = new SendPacket();
                        //    tmp.PackArray(new byte[] { 15, 4 });
                        //    tmp.Pack(t.CharID);
                        //    tmp.Pack(t.Pets.BattlePet.ID);
                        //    tmp.Pack((byte)0);
                        //    tmp.Pack((byte)1);
                        //    tmp.PackString(t.Pets.BattlePet.Name);
                        //    tmp.Pack16(0);//weapon
                        //    r.Send(tmp);
                        //}
                        //if (r.Pets.BattlePet != null)//to me
                        //{
                        //    SendPacket tmp = new SendPacket();
                        //    tmp.PackArray(new byte[] { 15, 4 });
                        //    tmp.Pack(r.CharID);
                        //    tmp.Pack(r.Pets.BattlePet.ID);
                        //    tmp.Pack((byte)0);
                        //    tmp.Pack((byte)1);
                        //    tmp.PackString(r.Pets.BattlePet.Name);
                        //    tmp.Pack16(0);//weapon
                        //    t.Send(tmp);
                        //}

                        #endregion
                        #region Riceball
                        //if (characters_in_map[a].riceBall.id > 0)
                        //{
                        //    if (characters_in_map[a].riceBall.active) g.ac5.Send_5(characters_in_map[a].riceBall.id, characters_in_map[a], t);
                        //}
                        //if (t.riceBall.id > 0)
                        //{
                        //    if (t.riceBall.active) g.ac5.Send_5(t.riceBall.id, t, characters_in_map[a]);
                        //}
                        #endregion
                        #region Team
                        //if (t.MyTeam.PartyLeader && t.MyTeam.hasParty && plist[a] != t)
                        //{
                        //    SendPacket fg = t.MyTeam._13_6;
                        //    plist[a].Send(fg);
                        //}
                        //if (plist[a].MyTeam.PartyLeader && plist[a].MyTeam.hasParty)
                        //{
                        //    SendPacket fg = plist[a].MyTeam._13_6;
                        //    t.Send(fg);
                        //}
                        #endregion
                        //if (Player.PlayerID != t.PlayerID)
                        //g.ac23.Send_74(Player.PlayerID, 0, c); //TODO find out what this does
                        #region Pets in Map
                        //AC 15,4 //possibly pet info for players on map with pets

                        //if (plist[a].CharacterState == PlayerState.inBattle)
                        //{
                        //    SendPacket qp = new SendPacket(t);
                        //    qp.PackArray(new byte[]{(11, 4);
                        //    qp.Pack((byte)2);
                        //    qp.Pack(plist[a].CharacterID);
                        //    qp.Pack16(0);
                        //    qp.Pack((byte)0);
                        //    qp.Send();
                        //}
                        #endregion
                        //23_76                    
                    }
                    tmp.Add(SendPacket.FromFormat("bbd", 23, 76, r.CharID));

                }
                //39_9
                //SendPacket gh = new SendPacket(g);
                //gh.PackArray(new byte[] { 244, 68, 5, 0, 22, 6, 1, 0, 1, 244, 68, 5, 0, 22, 6, 21, 0, 1, 244, 68, 5, 0, 22, 6, 22, 0, 1, 244, 68, 5, 0, 22, 6, 23, 0, 1, 244, 68, 5, 0, 22, 6, 24, 0, 1, });
                // cServer.Send(gh, t);
                /*tmp = new SendPacket(g);
                tmp.PackArray(new byte[]{(6, 2);
                tmp.Pack((byte)1);
                tmp.SetSize();
                tmp.Player = t;
                tmp.Send();
                for (int a = 0; a < 1; a++)
                {
                    gh = new SendPacket(g);
                    gh.PackArray(new byte[] { 244, 68, 2, 0, 20, 11, 244, 68, 2, 0, 20, 10 });
                    t.DatatoSend.Enqueue(gh);
                }
                for (int a = 0; a < 1; a++)
                {
                    gh = new SendPacket(g);
                    gh.PackArray(new byte[] { 244, 68, 2, 0, 20, 10 });
                    t.DatatoSend.Enqueue(gh);
                }*/
                tmp.Add(SendPacket.FromFormat("bb", 23, 102));
                tmp.Add(SendPacket.FromFormat("bb", 20, 8));
                t.Flags.Add(PlayerFlag.InMap); //t.CharacterState = PlayerState.inMap;
                t.Send(new SendPacket(tmp.End(),tmp.End().Count()));
            }

        #endregion

        #region Tent
        //public void onTentOpened(Tent Tentsrc)
        //{
        //    if (Tents.TryAdd(Tentsrc.MapID, Tentsrc))
        //        Broadcast(SendPacket.FromFormat("bbdWddW", 65, 1, Tentsrc.MapID, 36002, Tentsrc.X, Tentsrc.Y, 0));
        //}
        //public void onTentClosing(Tent tent)
        //{
        //    if (Tents.TryRemove(tent.MapID, out tent))
        //        Broadcast(SendPacket.FromFormat("bbd", 65, 4, tent.MapID));
        //}
        public void onEnterTent(UInt32 ID, Player p)
        {
            WarpData tmp = new WarpData();
            tmp.DstMap = (ushort)ID;
            tmp.DstX_Axis = 460;
            tmp.DstY_Axis = 700;
            Teleport(TeleportType.Tent, p, 0, tmp);
        }

        void SendOpenTents(Player p)
        {
            //if (Tents.Count > 0)
            //{
            //    SendPacket tmp = new SendPacket();
            //    tmp.Pack8((byte)65);
            //    tmp.Pack8((byte)3);
            //    Parallel.ForEach(Tents.Values, r =>
            //    {
            //        tmp.Pack32(r.MapID);
            //        tmp.Pack16(36002);
            //        tmp.Pack32(r.X);
            //        tmp.Pack32(r.Y);
            //        tmp.Pack8((byte)0);
            //        tmp.Pack8((byte)0);
            //    });
            //    p.Send(tmp);
            //}
        }

        #endregion

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
        public async void Broadcast(SendPacket pkt, string parameter, params object[] To)
        {
                switch (parameter)
                {
                    case "ALL": m_playerlist.ForEach(c => c.Send(pkt)); break;
                    case "Ex": m_playerlist.Where(c => To.Count(d => Convert.ToUInt32(d) == c.CharID) == 0).ToList().ForEach(c => c.Send(pkt)); break;
                    case "To": m_playerlist.Where(c => To.Count(d => Convert.ToUInt32(d) == c.CharID) > 0).ToList().ForEach(c => c.Send(pkt)); break;
                }
        }

        void SendAc12(Player target, byte portalID, WarpData To, bool toTent = false)
        {
            SendPacket sp = new SendPacket();
            sp.Pack8(12);
            sp.Pack32(target.CharID);
            sp.Pack16((toTent) ? (ushort)63507 : To.DstMap);
            sp.Pack16(To.DstX_Axis);
            sp.Pack16(To.DstY_Axis);
            sp.Pack16(portalID);
            sp.Pack8(0);
            if (toTent)
            {
                sp.Pack16(1);
                sp.Pack8(1);
                sp.Pack8(1);
            }
            sp.SetHeader();
            Broadcast(sp);
        }

    }
}
