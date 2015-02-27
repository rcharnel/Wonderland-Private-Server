using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Interface;
using Wonderland_Private_Server.DataManagement.DataFiles;
using Wlo.Core;

namespace Wonderland_Private_Server.Maps
{
    public class Map
    {
        MapData info;
        protected MapType MType;
        protected readonly object locker;
        protected bool GmOnly;
        public bool Kill;

        public System.Diagnostics.Stopwatch IdleTimer = new System.Diagnostics.Stopwatch();

        public ConcurrentDictionary<uint, Player> Players;

        #region MapData Def
        protected Dictionary<byte, WarpInfo> Destinations;
        protected Dictionary<byte, Entry_Exit_Point_Entries> Portals;
        protected Dictionary<byte, EventsinMapEntries> Events;
        protected Dictionary<byte, MapObjectEntries> MapObjects;
        #endregion

        protected ConcurrentDictionary<byte, DroppedItem> Items_Dropped;
        protected ConcurrentDictionary<int, Battle> Battles;
        protected ConcurrentDictionary<uint, Tent> Tents;

        public virtual ushort MapID { get { return (info != null) ? info.mapID : (ushort)0; } }
        public virtual string Name { get { return "Unknown Name"; } }
        public MapType TypeofMap { get { return MType; } }

        public Map(MapData mapsrc = null)
        {
            info = mapsrc;
            Destinations = new Dictionary<byte, WarpInfo>();
            Players = new ConcurrentDictionary<uint,Player>();
            locker = new object();
            Portals = new Dictionary<byte, Entry_Exit_Point_Entries>();
            Events = new Dictionary<byte, EventsinMapEntries>();
            MapObjects = new Dictionary<byte, MapObjectEntries>();
            Tents = new ConcurrentDictionary<uint, Tent>();
            Battles = new ConcurrentDictionary<int, Battle>();
            Items_Dropped = new ConcurrentDictionary<byte, DroppedItem>(10,255);
            LoadData();

            Thread tmp = new Thread(new ThreadStart(UpdateMap));
            tmp.Name = (MType == MapType.Regular)? MapID.ToString():"Tent "+new Random().Next(24578) + " Thread";
            tmp.Init();
        }

        protected virtual void LoadData()
        {
            Utilities.LogServices.Log("Initializing Map " + MapID + " " + Name);
            string loc = this.GetType().FullName.Replace(this.GetType().Name,"");
            
           if(info != null)
           {
               foreach (var r in info.Entry_Points)
                   Portals.Add((byte)r.clickID, r);
               Utilities.LogServices.Log(string.Format("Loaded {0} {1}",Portals.Count,"Warp Portals"));

               foreach (var r in info.WarpLoc)
                   Destinations.Add((byte)r.clickID, r);
               Utilities.LogServices.Log(string.Format("Loaded {0} {1}", Destinations.Count, "Warp Destinations"));

               foreach (var r in info.Events)
                   Events.Add((byte)r.clickID, r);
               Utilities.LogServices.Log(string.Format("Loaded {0} {1}", Events.Count, "Map Events"));

               foreach (var r in info.Npclist)
                  MapObjects.Add((byte)r.clickId, r);
               Utilities.LogServices.Log(string.Format("Loaded {0} {1}", Events.Count, "Map Objects"));
           }

        }        
        
        public virtual void UpdateMap()
        {
            do
            {
                if (Players.Count == 0 && !IdleTimer.IsRunning)
                    IdleTimer.Restart();
                else if (Players.Count > 0 && IdleTimer.IsRunning)
                    IdleTimer.Stop();

                foreach(var p in Players.Values)
                    if (!p.isAlive)
                    {
                        Player tmp;
                        Players.TryRemove(p.ID, out tmp);
                        Utilities.LogServices.Log("Player "+tmp.CharacterName+" removed from Map due to client disconnection");
                    }

                foreach (var battl in Battles.Values)
                    battl.Process();

                foreach (var obj in MapObjects.Values)
                    obj.Update(DateTime.Now, Players.Values.ToList());
                Thread.Sleep(100);
            }
            while (!Kill);
        }

        public bool ProccessInteraction(int where, ref Player by, int? answer = null)
        {
            Npc h = cGlobal.gNpcManager.GetNpc(17000);
            //if (MapObjects.ContainsKey((ushort)where))
            //{
            //    by.DataOut = SendType.Multi;
            //    SendPacket tmp = new SendPacket();
            //    tmp.Pack(new byte[] { 6, 2, 1 });
            //    by.Send(tmp);
            //    MapObjects[(ushort)where].Interact(ref by, answer);
            //    by.DataOut = SendType.Normal;
            //    return true;
            //}
            //else
                return false;
        }
       
        public virtual bool Teleport(TeleportType teletype, ref Player sender, byte portalID, WarpData warp = null)
        {
            sender.DataOut = SendType.Multi;
            if (teletype == TeleportType.Regular || teletype == TeleportType.CmD)
            {
                SendPacket warpConf = new SendPacket();
                warpConf.Pack(new byte[] { 20, 7 });
                sender.Send(warpConf);
            }

            SendPacket tmp = new SendPacket();
            tmp.Pack(new byte[] { 23, 32 });
            tmp.Pack(sender.ID);
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.Pack(new byte[] { 23, 112 });
            tmp.Pack(sender.ID);
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.Pack(new byte[] { 23, 132 });
            tmp.Pack(sender.ID);
            sender.Send(tmp);
            sender.State = PlayerState.InGame_Warping;
            //onWarp_Out(portalID, ref sender, warp, (teletype == TeleportType.Tent));// warp out of map

            switch (teletype)
            {
                #region Regular Warp
                case TeleportType.Regular:
                    {
                        if (TypeofMap != MapType.Regular && portalID == 1)  //create warp from Prev Map
                        {
                            onWarp_Out(portalID, ref sender, sender.PrevMap, (teletype == TeleportType.Tent));// warp out of map
                        }
                        else
                        {
                            var WarpID = (int)Events[Portals[portalID].unknownbytearray1[0]].SubEntry[0].SubEntry[0].dialog2;
                            if (WarpID == 0)
                            {

                                tmp = new SendPacket();
                                tmp.Pack(new byte[] { 20, 8 });
                                sender.Send(tmp); return false;
                            }
                            onWarp_Out(portalID, ref sender,new WarpData(Destinations[(byte)WarpID]),(teletype == TeleportType.Tent));// warp out of map
                            cGlobal.WLO_World.onTelePort(portalID, new WarpData(Destinations[(byte)WarpID]), ref sender);                           
                        }
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
                        onWarp_Out(portalID, ref sender, warp, (teletype == TeleportType.Tent));// warp out of map
                        sender.X = warp.DstX_Axis;//switch x
                        sender.Y = warp.DstY_Axis;//switch y
                        Tents[warp.DstMap].onWarp_In(0, ref sender, null);
                    } break;
                #endregion
                case TeleportType.Tool:/*t.inv.RemoveInv((byte)Entry, 1);*/ break;
                #region Cmd Warp
                case TeleportType.CmD:
                    {
                        onWarp_Out(portalID, ref sender, warp, (teletype == TeleportType.Tent));// warp out of map
                        sender.X = warp.DstX_Axis;//switch x
                        sender.Y = warp.DstY_Axis;//switch y
                        cGlobal.WLO_World.onTelePort(portalID, warp,ref sender);       
                    }break;
                #endregion
                case TeleportType.Login: onLogin(ref sender); break;
            }
            sender.DataOut = SendType.Normal;
            return true;
        }

        void onLogin(ref Player player)
        {
            player.CurrentMap = this;
            player.inGame = true;
            foreach (var r in Players.Values)
            {
                //send to them
                Broadcast(player._3Data(), player.ID);
                SendPacket p = new SendPacket();
                p.Pack(new byte[] { 5, 0 });
                p.Pack(player.ID);
                p.Pack(player.Eqs.Worn_Equips);
                Broadcast(p, player.ID);
                p = new SendPacket();
                p.Pack(new byte[] { 10, 3 });
                p.Pack(player.ID);
                p.Pack(255);
                Broadcast(p, r.ID);//maybe guild info???
                p = new SendPacket();
                p.Pack(new byte[] { 5, 8 });
                p.Pack(player.ID);
                p.Pack(0);
                Broadcast(p, r.ID);

                //send to me
                p = new SendPacket();
                p.Pack(7);
                p.Pack(r.ID);
                p.Pack(MapID);
                p.Pack(r.X);
                p.Pack(r.Y);
                player.Send(p);
                p = new SendPacket();
                p.Pack(new byte[] { 5, 0 });
                p.Pack(r.ID);
                p.Pack(r.Eqs.Worn_Equips);
                player.Send(p);
            }
            if (Players.ContainsKey(player.ID))
                Players.TryUpdate(player.ID, player, Players[player.ID]);
            else
                Players.TryAdd(player.ID, player);
            SendMapInfo(player);
        }
        public virtual void onWarp_In(byte portalID,ref Player src, WarpData from)
        {
            lock (locker)
            {
                src.CurrentMap = this;
                if (Players.ContainsKey(src.ID))
                    Players.TryUpdate(src.ID, src, Players[src.ID]);
                else
                    Players.TryAdd(src.ID, src);

                SendAc12(src,portalID, from);

                foreach( var r in Players.Values)
                {
                    if (r != src)
                    {
                    //send to them
                    SendPacket p = new SendPacket();
                    p.Pack(new byte[] { 5, 0 });
                    p.Pack(src.ID);
                    p.Pack(src.Eqs.Worn_Equips);
                    r.Send(p);
                    p = new SendPacket();
                    p.Pack(new byte[] { 10, 3 });
                    p.Pack(src.ID);
                    p.Pack(255);
                    r.Send(p);//maybe guild info???

                    //send to me
                    p = new SendPacket();
                    p.Pack(new byte[] { 7 });
                    p.Pack(r.ID);
                    p.Pack(MapID);
                    p.Pack(r.X);
                    p.Pack(r.Y);
                    src.Send(p);
                    p = new SendPacket();
                    p.Pack(new byte[] { 5, 0 });
                    p.Pack(r.ID);
                    p.Pack(r.Eqs.Worn_Equips);
                    src.Send(p);
                    }
                }

                SendOpenTents(src);

                src.QueueStart();
                SendMapInfo(src);
                src.QueueEnd();
            }

        }
        protected virtual void onWarp_Out(byte portalID,ref Player src, WarpData To,bool toTent)
        {
            lock (locker)
            {
                src.PrevMap = new WarpData();
                src.PrevMap.DstMap = MapID;
                src.PrevMap.DstX_Axis = src.X;
                src.PrevMap.DstY_Axis = src.Y;
                SendAc12(src,portalID, To,toTent);
                Players.TryRemove(src.ID, out src);
            }
        }

        #region Tent Related
        public void onTentOpened(Tent Tentsrc)
        {
            if (!Tents.ContainsKey(Tentsrc.MapID))
            {
                SendPacket opentent = new SendPacket();
                opentent.Pack(new byte[] { 65, 1 });
                opentent.Pack(Tentsrc.MapID);
                opentent.Pack(36002);
                opentent.Pack(Tentsrc.MapX);
                opentent.Pack(Tentsrc.MapY);
                opentent.Pack(0);
                Broadcast(opentent);
                Tents.TryAdd(Tentsrc.MapID,Tentsrc);
            }
        }
        public void onTentClosing(Tent tent)
        {
            SendPacket opentent = new SendPacket();
            opentent.Pack(new byte[] {65, 4});
            opentent.Pack(tent.MapID);
            Broadcast(opentent);
            Tents.TryRemove(tent.MapID,out tent);
        }
        public void onEnterTent(UInt32 ID, Player p)
        {
            WarpData tmp = new WarpData();
            tmp.DstMap = (ushort)ID;
            tmp.DstX_Axis = 460;
            tmp.DstY_Axis = 700;
            Teleport(TeleportType.Tent, ref p, 0, tmp);
        }

        void SendOpenTents(Player p)
        {
            SendPacket tmp = new SendPacket();
            tmp.Pack(new byte[] { 65, 3 });
            foreach (Tent r in Tents.Values)
            {
                tmp.Pack(r.MapID);
                tmp.Pack(36002);
                tmp.Pack(r.MapX);
                tmp.Pack(r.MapY);
                tmp.Pack(0);
                tmp.Pack(0);
            }
            if (tmp.Buffer.Count() > 6)
                p.Send(tmp);
        }

        #endregion

        #region BattleSection

        public void onPk_Started(Player starter,Player enemy) //TODO these should be lists of players
        {
            int a = 1;
            while(Battles.ContainsKey(a))
                a++;

            Battle battle = new Battle(cGlobal.gGameDataBase.GetBattleBG(MapID),a);
            battle.TypeofBattle = eBattleType.pk;
            battle.startedby = starter;

            //starter team
            starter.BattlePosition = BattleSide.Attacking;
            starter.BattleScene = battle;
            battle[BattleSide.Attacking].Add(eBattleType.pk, starter);

            foreach (Player d in starter.TeamMembers)
            {
                d.BattlePosition = BattleSide.Attacking;
                d.BattleScene = battle;
                battle[BattleSide.Attacking].Add(eBattleType.pk, d);
            }

            //enemy team
            enemy.BattlePosition = BattleSide.Defending;
            battle[BattleSide.Defending].Add(eBattleType.pk, enemy);
            enemy.BattleScene = battle;
            foreach (Player d in enemy.TeamMembers)
            {
                d.BattlePosition = BattleSide.Defending;
                d.BattleScene = battle;
                battle[BattleSide.Defending].Add(eBattleType.pk, d);
            }

            battle.StartBattle();
            battle.BattleState = eBattleState.Active;
            battle.StartRound();
            Battles.TryAdd(a,battle);
        }
        public void onMob_Ambush(Player starter, Fighter enemy)
        {
            //20,12 packet
        }
        public void onNpcPk(Player starter, Fighter enemy) //TODO these should be lists of players
        {
            int a = 1;
            while(Battles.ContainsKey(a))
                a++;

            Battle battle = new Battle(cGlobal.gGameDataBase.GetBattleBG(MapID),a);
            battle.TypeofBattle = eBattleType.pk;
            battle.startedby = starter;

            //starter team
            starter.BattlePosition = BattleSide.Attacking;
            starter.ClickID = enemy.ClickID;
            starter.BattleScene = battle;
            battle[BattleSide.Attacking].Add(eBattleType.pk, starter);

            foreach (Player d in starter.TeamMembers)
            {
                d.BattlePosition = BattleSide.Attacking;
                d.ClickID = enemy.ClickID;
                d.BattleScene = battle;
                battle[BattleSide.Attacking].Add(eBattleType.pk, d);
            }

            //enemy team
            enemy.BattlePosition = BattleSide.Defending;
            battle[BattleSide.Defending].Add(eBattleType.pk, enemy);
            //foreach (Player d in enemy.TeamMembers)
            //{
            //    d.BattlePosition = BattleSide.Defending;
            //    d.BattleScene = battle;
            //    battle[BattleSide.Defending].Add(eBattleType.pk, d);
            //}

            battle.StartBattle();
            battle.BattleState = eBattleState.Active;
            battle.StartRound();
            Battles.TryAdd(a, battle);
        }


        #endregion

         //public void UpdateRiceBall(UInt16 npcID, Player src)
        //{
        //    foreach (Player c in characters_in_map)
        //    {
        //        if (src.riceBall.active) g.ac5.Send_5(npcID, src, c);
        //        else g.ac5.Send_5(0, src, c);
        //    }
        //}

        #region Item
        public virtual cItem DropItem(cItem item, Player src)
        {
            var rand = new Random();
            int max = item.Ammt;
            int cnt = 0;

            while (cnt < max)
            {
                if ((Items_Dropped.Count + 1) <= 255)
                {

                    DroppedItem gi = new DroppedItem();
                    gi.CopyFrom(item);
                    gi.Ammt = 1;
                    gi.X = (ushort)(src.X - 25 + (rand.Next(0, 220) * 1.2));
                    gi.Y += (ushort)(src.Y + (rand.Next(0, 114) * 1.2));
                    gi.Expires = DateTime.Now.AddMinutes(2);

                    byte a = 0;
                    while (Items_Dropped.ContainsKey((byte)(a + 1)) && a < 255)
                    {
                        if (a > 255)
                        {
                            item.Ammt = (byte)(cnt + 1);
                            return item;
                        } a++;
                    }

                    Items_Dropped.TryAdd((byte)(a + 1), gi);

                    foreach (Player c in Players.Values)
                    {
                        SendPacket p = new SendPacket();
                        p.Pack(new byte[] { 23, 3 });
                        p.Pack(item.ItemID);
                        p.Pack(gi.X);
                        p.Pack(gi.Y);
                        p.Pack(0);
                        p.Pack(c == src);//true = 1
                        c.Send(p);
                    }
                }
                else
                    break;
                cnt++;
            }

            if(cnt != max)
            {
                item.Ammt = (byte)(cnt + 1);
                return item;
            }
            return null;
        }
        public virtual void PickUpItem(UInt16 itemLoc, ref Player src)
        {
            if (!Items_Dropped.ContainsKey((byte)itemLoc)) return;

            InvItemCell t = new InvItemCell();
            t.CopyFrom(Items_Dropped[(byte)itemLoc]);

            if (src.Inv.AddItem(t) > 0)
            {
                //gItem.CopyFrom(DroppedItems[itemIndex]);
                //foreach (ItemsinMapEntries o in ItemAreas)
                //{
                //    if (DroppedItems[itemIndex].entryid == o.clickID && DroppedItems[itemIndex].Respawns)
                //    {
                //        o.pickedup = true;
                //        o.dropin = DateTime.Now.AddMinutes(2);
                //    }
                //}
                foreach (Player c in Players.Values)
                {
                    if (c.ID == src.ID)
                        RemGItem(itemLoc, c, true);
                    else
                        RemGItem(itemLoc, c, false);
                }

            }
        }        
        public void RemGItem(UInt16 itemIndex, Player target, bool animate)
        {
            //if (target.CharacterState == PlayerState.Warping || target.CharacterState == PlayerState.Logging_In ||
            //    target.CharacterState == PlayerState.Logging_Off || target.CharacterState == PlayerState.inTent ||
            //    target.CharacterState == PlayerState.CharacterSelection || target.CharacterState == PlayerState.Creating ||
            //    target.CharacterState == PlayerState.LoginWindow || target.CharacterState == PlayerState.Disconnected) return;
            SendPacket p = new SendPacket();
            p.Pack(new byte[] { 23, 2 });
            p.Pack(itemIndex);
            if (animate)
                p.Pack(1);
            target.Send(p);
            DroppedItem f;
            Items_Dropped.TryRemove((byte)itemIndex,out f);
        }
        #endregion
        protected virtual void SendMapInfo(Player t)
        {
            SendPacket p = new SendPacket();
            p = new SendPacket();
            p.Pack(new byte[] { 23, 138 });
            t.Send(p);
            /* p = new SendPacket();
             p.Pack(new byte[]{(6, 2);
             p.Pack(1);
             p.SetSize();
             g.SendPacket(t, p);*/
            #region Send Npc
            #endregion
            #region Send Item
            if (Items_Dropped.Count > 0)
            {
                int unk = 0;
                p = new SendPacket();
                p.Pack(new byte[] { 23, 4 });
                for (byte a = 0; a < Items_Dropped.ToList().Count; a++)
                {
                    if (Items_Dropped[a].NonExpirable)
                    {
                        p.Pack(3);
                        p.Pack(1);
                        unk = Items_Dropped[a].Control;
                    }

                    p.Pack((ushort)a);
                    p.Pack((ushort)Items_Dropped[a].ItemID);
                    p.Pack((ushort)Items_Dropped[a].X);
                    p.Pack((ushort)Items_Dropped[a].Y);
                    p.Pack((uint)unk);
                }
                t.Send(p);
            }
            #endregion
            //SendNpcs(t);
            //SendItems(t);

            foreach(var r in Players.Values)
            {
                p = new SendPacket();
                p.Pack(new byte[] { 23, 122 });
                p.Pack(r.ID);
                t.Send(p);

                if (r.ID != t.ID)
                {
                    p = new SendPacket();
                    p.Pack(new byte[] { 10, 3 });
                    p.Pack(r.ID);
                    p.Pack(255);
                    t.Send(p);
                    #region Emotes used on Map
                    if (r.Emote > 0)
                    {
                        p = new SendPacket();
                        p.Pack(new byte[] { 32, 2 });
                        p.Pack(r.ID);
                        p.Pack(r.Emote);
                        t.Send(p);
                    }
                    #endregion

                    #region Pets in Map
                    if (t.Pets.BattlePet != null)//to them
                    {
                        SendPacket tmp = new SendPacket();
                        tmp.Pack(new byte[] { 15, 4 });
                        tmp.Pack(t.ID);
                        tmp.Pack(t.Pets.BattlePet.ID);
                        tmp.Pack(0);
                        tmp.Pack(1);
                        tmp.Pack(t.Pets.BattlePet.Name);
                        tmp.Pack(0);//weapon
                        r.Send(tmp);
                    }
                    if (r.Pets.BattlePet != null)//to me
                    {
                        SendPacket tmp = new SendPacket();
                        tmp.Pack(new byte[] { 15, 4 });
                        tmp.Pack(r.ID);
                        tmp.Pack(r.Pets.BattlePet.ID);
                        tmp.Pack(0);
                        tmp.Pack(1);
                        tmp.Pack(r.Pets.BattlePet.Name);
                        tmp.Pack(0);//weapon
                        t.Send(tmp);
                    }

                    #endregion
                    //if (characters_in_map[a].riceBall.id > 0)
                    //{
                    //    if (characters_in_map[a].riceBall.active) g.ac5.Send_5(characters_in_map[a].riceBall.id, characters_in_map[a], t);
                    //}
                    //if (t.riceBall.id > 0)
                    //{
                    //    if (t.riceBall.active) g.ac5.Send_5(t.riceBall.id, t, characters_in_map[a]);
                    //}

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
                    //if (Player.PlayerID != t.PlayerID)
                    //g.ac23.Send_74(Player.PlayerID, 0, c); //TODO find out what this does
                    //AC 15,4 //possibly pet info for players on map with pets

                    //if (plist[a].CharacterState == PlayerState.inBattle)
                    //{
                    //    SendPacket qp = new SendPacket(t);
                    //    qp.Pack(new byte[]{(11, 4);
                    //    qp.Pack(2);
                    //    qp.Pack(plist[a].CharacterID);
                    //    qp.Pack(0);
                    //    qp.Pack(0);
                    //    qp.Send();
                    //}
                    //Send_32_2(t);
                    //23_76                    
                }
                p = new SendPacket();
                p.Pack(new byte[] { 23, 76 });
                p.Pack(r.ID);
                t.Send(p);
            }
            //39_9
            //SendPacket gh = new SendPacket(g);
            //gh.Pack(new byte[] { 244, 68, 5, 0, 22, 6, 1, 0, 1, 244, 68, 5, 0, 22, 6, 21, 0, 1, 244, 68, 5, 0, 22, 6, 22, 0, 1, 244, 68, 5, 0, 22, 6, 23, 0, 1, 244, 68, 5, 0, 22, 6, 24, 0, 1, });
            // cServer.Send(gh, t);
            /*tmp = new SendPacket(g);
            tmp.Pack(new byte[]{(6, 2);
            tmp.Pack(1);
            tmp.SetSize();
            tmp.Player = t;
            tmp.Send();
            for (int a = 0; a < 1; a++)
            {
                gh = new SendPacket(g);
                gh.Pack(new byte[] { 244, 68, 2, 0, 20, 11, 244, 68, 2, 0, 20, 10 });
                t.DatatoSend.Enqueue(gh);
            }
            for (int a = 0; a < 1; a++)
            {
                gh = new SendPacket(g);
                gh.Pack(new byte[] { 244, 68, 2, 0, 20, 10 });
                t.DatatoSend.Enqueue(gh);
            }*/
            p = new SendPacket();
            p.Pack(new byte[] { 23, 102 });
            t.Send(p);
            p = new SendPacket();
            p.Pack(new byte[] { 20, 8 });
            t.Send(p);
            //t.CharacterState = PlayerState.inMap;
        }

        public void Broadcast(SendPacket tmp)
        {
            Players.Values.ToList().ForEach(c => c.Send(tmp));
        }
        public void Broadcast(SendPacket tmp, uint ignore)
        {
            Players.Values.Where(c => c.ID != ignore).ToList().ForEach(c => c.Send(tmp));
        }

        #region Quick Data Send
        void SendAc12(Player target, byte portalID, WarpData To,bool toTent = false)
        {
            SendPacket sp = new SendPacket();
            sp.Pack(new byte[] { 12 });
            sp.Pack(target.ID);
            sp.Pack((toTent) ? (ushort)63507 : To.DstMap);
            sp.Pack(To.DstX_Axis);
            sp.Pack(To.DstY_Axis);
            sp.Pack(portalID);
            sp.Pack(0);
            if(toTent)
            {
                sp.Pack(1);
                sp.Pack(1);
                sp.Pack(1);
            }
            //mapPlayers.Values.ToList().ForEach(new Action<Player>(c => c.Send(sp)));
        }
        #endregion
    }
}
