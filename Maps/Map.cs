using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Maps
{
    public class Map
    {
        protected MapType MType;
        protected readonly object locker;
        protected bool GmOnly;
        protected Dictionary<byte, DroppedItem> Items_Dropped;
        protected Dictionary<uint, Player> mapPlayers;
        public Dictionary<uint, Player> Players { get { return mapPlayers; } }
        protected Dictionary<byte, WarpData> Destinations;
        protected Dictionary<byte, WarpPortal> Portals;
        protected Dictionary<ushort, MapObject > MapObjects;
        protected List<Battle> Battles;
        protected Dictionary<uint,Tent> Tents;

        public virtual ushort MapID { get { return 0; } }
        public virtual string Name { get { return "None"; } }

        public MapType TypeofMap { get { return MType; } }
        protected bool updateready; public bool MapneedsUpdate { set { updateready = value; } }

        public Map()
        {
            mapPlayers = new Dictionary<uint, Player>();
            locker = new object();
            Destinations = new Dictionary<byte, WarpData>();
            Portals = new Dictionary<byte, WarpPortal>();
            MapObjects = new Dictionary<ushort, MapObject>();
            Tents = new Dictionary<uint, Tent>();
            Battles = new List<Battle>();
            Items_Dropped = new Dictionary<byte, DroppedItem>(255);
            LoadData();
        }

        protected virtual void LoadData()
        {
            Utilities.LogServices.Log("Initializing Map " + MapID + " " + Name);

//            #region load Interactable Objects for this map

////load data from data files

//            MapData srcdata = myhost.EveDataManager.GetMapData(MapID);

//            foreach (var n in srcdata.Npclist)
//            {
//                MapObject m =  new MapObject();
//                m.CickID = n.clickId;
//                m.ObjInfo = n;
//                MapObjects.Add(m.CickID,m);
//            }


//            foreach (var y in (from c in myDllAssembly.GetTypes()
//                               where c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(InteractableObj))
//                               select c))
//            {
//                InteractableObj m = null; 

//                try
//                {
//                    m = (Activator.CreateInstance(y) as InteractableObj);
//                    if(MapObjects.ContainsKey(m.clickID))
//                    MapObjects[m.clickID].DataOverride = m;
//                }
//                catch { Utilities.LogServices.Log((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "LoadData", new Exception("failed to load Map Object " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
//            }

//            DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + MapObjects.Count + " Map Objects");
//            #endregion

//            #region load Warp Destinations for this map

//            foreach (var y in (from c in myDllAssembly.GetTypes()
//                               where c.IsClass && c.IsPublic && typeof(WarpData).IsAssignableFrom(c)
//                               select c))
//            {
//                WarpData m = null;
//                try
//                {
//                     m = (Activator.CreateInstance(y) as WarpData);
//                }
//                catch { Utilities.LogServices.Log("PserverDataPlugin", "Map.cs LoadData", new Exception("failed to load Warp Destination " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
//                    if (!Destinations.ContainsKey((byte)m.ID))
//                        Destinations.Add((byte)m.ID, m);
                
//            }
//            DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + Destinations.Count + " Warp Destinations");
//            #endregion

//            #region load Warp Portals for this map

//            foreach (var y in (from c in myDllAssembly.GetTypes()
//                               where c.IsClass && c.IsPublic && typeof(WarpPortal).IsAssignableFrom(c)
//                               select c))
//            {
//                WarpPortal m = null;
//                try
//                {
//                    m = (Activator.CreateInstance(y) as WarpPortal);
//                }
//                catch { Utilities.LogServices.Log("PserverDataPlugin", "Map.cs LoadData", new Exception("failed to load Warp Portal " + (Activator.CreateInstance(y) as InteractableObj).clickID)); }
//                if (!Portals.ContainsKey((byte)m.ID))
//                    Portals.Add((byte)m.ID, m);
//            }
            
//            DLogger.DllLog((myDllAssembly == null) ? Assembly.GetExecutingAssembly().FullName : myDllAssembly.FullName, "loaded " + Portals.Count + " Warp Portals");
//            #endregion

        }

        public virtual void onLogin(ref Player player)
        {
            for (int a = 0; a < Players.Count; a++)
            {
                if (mapPlayers.Values.ToList()[a].ID == player.ID) return;
            }
            player.CurrentMap = this;
            player.inGame = true;
            for (int a = 0; a < Players.Count; a++)
            {
                //send to them
                Broadcast(player._3Data(), player.ID);
                SendPacket p = new SendPacket();
                p.PackArray(new byte[] { 5, 0 });
                p.Pack32(player.ID);
                p.PackArray(player.Eqs.Worn_Equips);
                Broadcast(p, player.ID);
                p = new SendPacket();
                p.PackArray(new byte[] { 10, 3 });
                p.Pack32(player.ID);
                p.Pack8(255);
                Broadcast(p, mapPlayers.Values.ToList()[a].ID);//maybe guild info???
                p = new SendPacket();
                p.PackArray(new byte[] { 5, 8 });
                p.Pack32(player.ID);
                p.Pack8(0);
                Broadcast(p, mapPlayers.Values.ToList()[a].ID);

                //send to me
                p = new SendPacket();
                p.Pack8(7);
                p.Pack32(mapPlayers.Values.ToList()[a].ID);
                p.Pack16(MapID);
                p.Pack16(mapPlayers.Values.ToList()[a].X);
                p.Pack16(mapPlayers.Values.ToList()[a].Y);
                player.Send(p);
                p = new SendPacket();
                p.PackArray(new byte[] { 5, 0 });
                p.Pack32(mapPlayers.Values.ToList()[a].ID);
                p.PackArray(mapPlayers.Values.ToList()[a].Eqs.Worn_Equips);
                player.Send(p);
            }
            mapPlayers.Add(player.ID, player);
            SendMapInfo(player);
        }
        public virtual void onLogout(ref Player player)
        {
            if (Players.ContainsKey(player.ID))
            {
               if(Tents.ContainsKey(player.ID))
                   Tents[player.ID].Close();

                Players.Remove(player.ID);
            }
        }
        public bool ProccessInteraction(int where, ref Player by, int? answer = null)
        {

            if (MapObjects.ContainsKey((ushort)where))
            {
                by.DataOut = SendType.Multi;
                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 6, 2, 1 });
                by.Send(tmp);
                MapObjects[(ushort)where].Interact(ref by, answer);
                by.DataOut = SendType.Normal;
                return true;
            }
            else
                return false;
        }
        public virtual void UpdateMap()
        {
        }

        public virtual bool Teleport(TeleportType teletype, ref Player sender, byte portalID, WarpData warp = null)
        {
            WarpData mapid = null;
            if (GmOnly && !sender.GM)
            {
                SendPacket p = new SendPacket();
                p = new SendPacket();
                p.PackArray(new byte[] { 2, 3 });
                p.Pack32(100);
                p.PackNString("Only GMs can Enter %#");
                sender.Send(p);
                return false;
            }
            if (teletype != TeleportType.Regular || MType == MapType.Tent) goto skip;
            WarpPortal door = null;

            if (!Portals.ContainsKey(portalID)) return false;
            door = Portals[portalID];

            if (door == null || !door.Enter(ref sender)) return false;
            mapid = Destinations[(byte)door.Dst];

        skip:
            sender.State = PlayerState.InGame_Warping;

            if (mapid != null || warp != null || portalID == 1) { } else return false;
            if (warp != null) mapid = warp;
            else if (portalID == 1)  //create warp from Prev Map
            {
                mapid = sender.PrevMap;
                mapid.ID = portalID;
            }

            sender.DataOut = SendType.Multi;
            if (teletype == TeleportType.Regular)
            {
                SendPacket warpConf = new SendPacket();
                warpConf.PackArray(new byte[] { 20, 7 });
                sender.Send(warpConf);
            }

            SendPacket tmp = new SendPacket();
            tmp.PackArray(new byte[] { 23, 32 });
            tmp.Pack32(sender.ID);
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.PackArray(new byte[] { 23, 112 });
            tmp.Pack32(sender.ID);
            sender.Send(tmp);
            tmp = new SendPacket();
            tmp.PackArray(new byte[] { 23, 132 });
            tmp.Pack32(sender.ID);
            sender.Send(tmp);

            onWarp_Out(portalID, ref sender, mapid,(teletype == TeleportType.Tent));// warp out of map
            //save current map
            sender.PrevMap = new WarpData();
            sender.PrevMap.DstMap = sender.CurrentMap.MapID;
            sender.PrevMap.DstX_Axis = sender.X;
            sender.PrevMap.DstY_Axis = sender.Y;

            sender.X = mapid.DstX_Axis;//switch x
            sender.Y = mapid.DstY_Axis;//switch y

            //if (teletype != TeleportType.Tent)
            //    cGlobal.WLO_World.Teleport(portalID, mapid, sender);
            //else
            //    Tents[mapid.DstMap].onWarp_In(0, ref sender, null);

            sender.DataOut = SendType.Normal;
            return true;
        }

        public virtual void onWarp_In(byte portalID,ref Player src, WarpData from)
        {
            lock (locker)
            {
                for (int a = 0; a < Players.Count; a++)
                {
                    if (mapPlayers.Values.ToList()[a].ID == src.ID) return;
                }
                src.CurrentMap = this;
                Players.Add(src.ID,src);
                SendAc12(src,portalID, from);

                for (int a = 0; a < Players.Count; a++)
                {
                    if (mapPlayers.Values.ToList()[a] != src)
                    {
                    //send to them
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 5, 0 });
                    p.Pack32(src.ID);
                    p.PackArray(src.Eqs.Worn_Equips);
                    mapPlayers.Values.ToList()[a].Send(p);
                    p = new SendPacket();
                    p.PackArray(new byte[] { 10, 3 });
                    p.Pack32(src.ID);
                    p.Pack8(255);
                    mapPlayers.Values.ToList()[a].Send(p);//maybe guild info???

                    //send to me
                    p = new SendPacket();
                    p.PackArray(new byte[] { 7 });
                    p.Pack32(mapPlayers.Values.ToList()[a].ID);
                    p.Pack16(MapID);
                    p.Pack16(mapPlayers.Values.ToList()[a].X);
                    p.Pack16(mapPlayers.Values.ToList()[a].Y);
                    src.Send(p);
                    p = new SendPacket();
                    p.PackArray(new byte[] { 5, 0 });
                    p.Pack32(mapPlayers.Values.ToList()[a].ID);
                    p.PackArray(mapPlayers.Values.ToList()[a].Eqs.Worn_Equips);
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
            Player f;
            lock (locker)
            {
                SendAc12(src,portalID, To,toTent);
                Players.Remove(src.ID);
            }
        }

        #region Tent Related
        public void onTentOpened(Tent Tentsrc)
        {
            if (!Tents.ContainsKey(Tentsrc.MapID))
            {
                SendPacket opentent = new SendPacket();
                opentent.PackArray(new byte[] { 65, 1 });
                opentent.Pack32(Tentsrc.MapID);
                opentent.Pack16(36002);
                opentent.Pack32(Tentsrc.MapX);
                opentent.Pack32(Tentsrc.MapY);
                opentent.Pack16(0);
                Broadcast(opentent);
                Tents.Add(Tentsrc.MapID,Tentsrc);
            }
        }
        public void onTentClosing(Tent tent)
        {
            SendPacket opentent = new SendPacket();
            opentent.PackArray(new byte[] {65, 4});
            opentent.Pack32(tent.MapID);
            Broadcast(opentent);
            Tents.Remove(tent.MapID);
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
            tmp.PackArray(new byte[] { 65, 3 });
            foreach (Tent r in Tents.Values)
            {
                tmp.Pack32(r.MapID);
                tmp.Pack16(36002);
                tmp.Pack32(r.MapX);
                tmp.Pack32(r.MapY);
                tmp.Pack8(0);
                tmp.Pack8(0);
            }
            p.Send(tmp);
        }

        #endregion

        #region BattleSection

        //public void onPk_Started(Player starter, Player enemy) //TODO these should be lists of players
        //{
        //    Battle battle = new Battle(myhost.GameDataBase.GetBattleBG(MapID), myhost);
        //    battle.TypeofBattle = eBattleType.pk;
        //    battle.startedby = starter;

        //    //starter team
        //    BattleArea right = new BattleArea(5, myhost,ref battle);
        //    starter.BattlePosition = BattleSide.Attacking;
        //    starter.BattleScene = right;
        //    right.Add(eBattleType.pk, starter);
        //    foreach (Player d in starter.TeamMembers)
        //    {
        //        d.BattlePosition = BattleSide.Attacking;
        //        d.BattleScene = right;
        //        right.Add(eBattleType.pk,d);
        //    }

        //    //enemy team
        //    BattleArea left = new BattleArea(2, myhost, ref battle);
        //    enemy.BattlePosition = BattleSide.Defending;
        //    enemy.BattleScene = left;
        //    left.Add(eBattleType.pk, enemy);
        //    foreach (Player d in enemy.TeamMembers)
        //    {
        //        d.BattlePosition = BattleSide.Defending;
        //        d.BattleScene = left;
        //        left.Add(eBattleType.pk, d);
        //    }

        //    //watch
        //    BattleArea watch = new BattleArea(4, myhost, ref battle);

        //    battle.Side.Add(2, left);
        //    battle.Side.Add(5, right);            
        //    battle.Side.Add(4, watch);
            
        //    battle.StartBattle();
        //    //battle.BattleState = eBattleState.Active;
        //    //MapUpdate += battle.Process;
        //    //battle.StartRound();
        //    //Battles_in_Map.Add(battle);
        //}
        //public void onMob_Ambush()
        //{
        //    //20,12 packet
        //}
        //public void StartPKNpc(Player starter, PetFighter enemy) //TODO these should be lists of players
        //{

        //    //Battle battle = new Battle(g);
        //    //battle.Type = eBattleType.pk;
        //    //battle.Background = g.GameDataBase.GetMapBackground(MapID);

        //    //Player f = new Player(g);
        //    //f.SetFrom(starter);
        //    //f.clickID = enemy.clickID;
        //    //f.Position = BattleSide.right;
        //    //battle.startedby = f;
        //    //f.StartedBattle = true;
        //    //battle.FighterJoined(f);
        //    //foreach (Player d in starter.character.MyTeam.TeamMembers)
        //    //{
        //    //    Player fe = new Player(g);
        //    //    fe.SetFrom(d);
        //    //    fe.clickID = enemy.clickID;
        //    //    fe.Position = BattleSide.right;
        //    //    battle.FighterJoined(fe);
        //    //}
        //    //enemy.Position = BattleSide.left;
        //    //battle.FighterJoined(enemy);
        //    //battle.Send_BattleInfo();
        //    //battle.BattleState = eBattleState.Active;
        //    //MapUpdate += battle.Process;
        //    //battle.StartRound();
        //    //Battles_in_Map.Add(battle);
        //}


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

                    Items_Dropped.Add((byte)(a + 1), gi);

                    foreach (Player c in Players.Values)
                    {
                        SendPacket p = new SendPacket();
                        p.PackArray(new byte[] { 23, 3 });
                        p.Pack16(item.ItemID);
                        p.Pack16(gi.X);
                        p.Pack16(gi.Y);
                        p.Pack32(0);
                        p.PackBoolean(c == src);//true = 1
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
                foreach (Player c in mapPlayers.Values)
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
            p.PackArray(new byte[] { 23, 2 });
            p.Pack16(itemIndex);
            if (animate)
                p.Pack8(1);
            target.Send(p);
            Items_Dropped.Remove((byte)itemIndex);
        }
        #endregion
        protected virtual void SendMapInfo(Player t)
        {
            SendPacket p = new SendPacket();
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 138 });
            t.Send(p);
            /* p = new SendPacket();
             p.PackArray(new byte[]{(6, 2);
             p.Pack32(1);
             p.SetSize();
             g.SendPacket(t, p);*/
            #region Send Npc
            #endregion
            #region Send Item
            if (Items_Dropped.Count > 0)
            {
                int unk = 0;
                p = new SendPacket();
                p.PackArray(new byte[] { 23, 4 });
                for (byte a = 0; a < Items_Dropped.ToList().Count; a++)
                {
                    if (Items_Dropped[a].NonExpirable)
                    {
                        p.Pack8(3);
                        p.Pack8(1);
                        unk = Items_Dropped[a].Data.UnknownWord1;
                    }

                    p.Pack16((ushort)a);
                    p.Pack32((ushort)Items_Dropped[a].ItemID);
                    p.Pack16((ushort)Items_Dropped[a].X);
                    p.Pack16((ushort)Items_Dropped[a].Y);
                    p.Pack32((uint)unk);
                }
                t.Send(p);
            }
            #endregion
            //SendNpcs(t);
            //SendItems(t);

            for (int a = 0; a < Players.Count; a++)
            {
                p = new SendPacket();
                p.PackArray(new byte[] { 23, 122 });
                p.Pack32(mapPlayers.Values.ToList()[a].ID);
                t.Send(p);
                if (mapPlayers.Values.ToList()[a].ID != t.ID)
                {
                    p = new SendPacket();
                    p.PackArray(new byte[] { 10, 3 });
                    p.Pack32(mapPlayers.Values.ToList()[a].ID);
                    p.Pack8(255);
                    t.Send(p);
#region Emotes used on Map
                    if (mapPlayers.Values.ToList()[a].Emote > 0)
                    {
                        p = new SendPacket();
                        p.PackArray(new byte[] { 32, 2 });
                        p.Pack32(mapPlayers.Values.ToList()[a].ID);
                        p.Pack8(mapPlayers.Values.ToList()[a].Emote);
                        t.Send(p);
                    }
#endregion

                    #region Pets in Map
                     if(t.Pets.BattlePet != null)//to them
                     {
                         SendPacket tmp = new SendPacket();
                         tmp.PackArray(new byte[] { 15, 4 });
                         tmp.Pack32(t.ID);
                         tmp.Pack32(t.Pets.BattlePet.ID);
                         tmp.Pack8(0);
                         tmp.Pack8(1);
                         tmp.PackString(t.Pets.BattlePet.Name);
                         tmp.Pack16(0);//weapon
                         mapPlayers.Values.ToList()[a].Send(tmp);
                     }
                    if(mapPlayers.Values.ToList()[a].Pets.BattlePet != null)//to me
                    {
                        SendPacket tmp = new SendPacket();
                        tmp.PackArray(new byte[] { 15, 4 });
                        tmp.Pack32(mapPlayers.Values.ToList()[a].ID);
                        tmp.Pack32(mapPlayers.Values.ToList()[a].Pets.BattlePet.ID);
                        tmp.Pack8(0);
                        tmp.Pack8(1);
                        tmp.PackString(mapPlayers.Values.ToList()[a].Pets.BattlePet.Name);
                        tmp.Pack16(0);//weapon
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
                    //    qp.PackArray(new byte[]{(11, 4);
                    //    qp.Pack8(2);
                    //    qp.Pack32(plist[a].CharacterID);
                    //    qp.Pack16(0);
                    //    qp.Pack8(0);
                    //    qp.Send();
                    //}
                    //Send_32_2(t);
                    //23_76                    
                }
                p = new SendPacket();
                p.PackArray(new byte[] { 23, 76 });
                p.Pack32(mapPlayers.Values.ToList()[a].ID);
                t.Send(p);
            }
            //39_9
            //SendPacket gh = new SendPacket(g);
            //gh.PackArray(new byte[] { 244, 68, 5, 0, 22, 6, 1, 0, 1, 244, 68, 5, 0, 22, 6, 21, 0, 1, 244, 68, 5, 0, 22, 6, 22, 0, 1, 244, 68, 5, 0, 22, 6, 23, 0, 1, 244, 68, 5, 0, 22, 6, 24, 0, 1, });
            // cServer.Send(gh, t);
            /*tmp = new SendPacket(g);
            tmp.PackArray(new byte[]{(6, 2);
            tmp.Pack8(1);
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
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 102 });
            t.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 20, 8 });
            t.Send(p);
            //t.CharacterState = PlayerState.inMap;
        }

        public void Broadcast(SendPacket tmp)
        {
            mapPlayers.Values.ToList().ForEach(c => c.Send(tmp));
        }
        public void Broadcast(SendPacket tmp, uint ignore)
        {
            mapPlayers.Values.Where(c => c.ID != ignore).ToList().ForEach(c => c.Send(tmp));
        }

        #region Quick Data Send
        void SendAc12(Player target, byte portalID, WarpData To,bool toTent = false)
        {
            SendPacket sp = new SendPacket();
            sp.PackArray(new byte[] { 12 });
            sp.Pack32(target.ID);
            sp.Pack16((toTent) ? (ushort)63507 : To.DstMap);
            sp.Pack16(To.DstX_Axis);
            sp.Pack16(To.DstY_Axis);
            sp.Pack16(portalID);
            sp.Pack8(0);
            if(toTent)
            {
                sp.Pack16(1);
                sp.Pack8(1);
                sp.Pack8(1);
            }
            mapPlayers.Values.ToList().ForEach(new Action<Player>(c => c.Send(sp)));
        }
        #endregion
    }
}
