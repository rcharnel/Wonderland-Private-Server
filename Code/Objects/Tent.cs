using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Maps;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Tent:Map//Map allows the server to treat this as a map however
    {
        readonly object locker = new object();
        Player owner;
        Dictionary<byte, TentFloor> Floors = new Dictionary<byte, TentFloor>();
        Map Loc;

        public ushort MapX;
        public ushort MapY;
        public bool Closed;

        ushort floorcolor = 39062, wallcolor = 39064;
        
        public Tent(Player src):base()
        {
            Closed = true;
            MType = MapType.Tent;
            owner = src;
            Floors.Add(1, new TentFloor(1));//lets do floor one for now
        }
        public override ushort MapID
        {
            get
            {
                return (ushort)owner.ID;
            }
        }
        public string Name
        {
            get
            {
                return owner.CharacterName + "'s Home";
            }
        }

        
        override protected  void LoadData()
        {
            //pre setup tent with base
        }
        public void Save()
        {
            // save info
        }
        public void Load()
        {
            //add other info
        }

        public void Open()
        {
            if (!Closed) return;
            MapX = owner.X;
            MapY = owner.Y;
            Loc = owner.CurrentMap;
            Loc.onTentOpened(this);
            SendPacket opentent = new SendPacket();
            opentent.PackArray(new byte[] { 62, 59 });
            opentent.Pack8(2);
            owner.Send(opentent);
            Closed = false;
        }
        public void Close()
        {
            if (Closed) return;
            Loc.onTentClosing(this);
            WarpData warp = new WarpData();
            warp.DstMap = Loc.MapID;
            warp.DstX_Axis = MapX;
            warp.DstY_Axis = MapY;
            //warp Players  out
            foreach(var f in Floors)
            {

                foreach (var p in mapPlayers.Values.ToList())
                {
                    p.DataOut = SendType.Multi;
                    SendPacket warpConf = new SendPacket();
                    warpConf.PackArray(new byte[] { 20, 7 });
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 23, 32 });
                    tmp.Pack32(p.ID);
                    p.Send(tmp);
                    tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 23, 112 });
                    tmp.Pack32(p.ID);
                    p.Send(tmp);
                    tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 23, 132 });
                    tmp.Pack32(p.ID);
                    p.Send(tmp);
                    onWarp_Out(f.Key, p, warp);// warp out of map
                    p.X = warp.DstX_Axis;//switch x
                    p.Y = warp.DstY_Axis;//switch y
                    //cGlobal.WLO_World.Teleport(f.Key, warp, p);
                    p.DataOut = SendType.Normal;
                    
                }
            }
            Closed = true;
        }

        public override void onWarp_In(byte portalID, ref Player src, WarpData from)
        {
            lock (locker)
            {
                for (int a = 0; a < Players.Count; a++)
                {
                    if (mapPlayers.Values.ToList()[a].ID == src.ID) return;
                } 

                src.CurrentMap = this;
                mapPlayers.Add(src.ID, src);

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
                src.QueueStart();
                SendMapInfo(src);
                src.QueueEnd();
            }
        }
        protected override void SendMapInfo(Player t)
        {
            SendPacket p = new SendPacket();

            #region TentItems
            #endregion

            //build queue


            //storeroom


            //floor
            p = new SendPacket();
            p.PackArray(new byte[] { 62, 14 });
            p.Pack16(floorcolor);
            t.Send(p);

            //wallpaper
            p = new SendPacket();
            p.PackArray(new byte[] { 62, 15 });
            p.Pack16(wallcolor);
            t.Send(p);

            //65,11 ???

            p = new SendPacket();
            p.PackArray(new byte[] { 65, 7 });
            p.Pack16(0);
            t.Send(p);

            //extended Tent Item Info


            //ParkingGarage Info

            p = new SendPacket();
            p.PackArray(new byte[] { 23, 138 });
            t.Send(p);

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
                    if (t.Pets.BattlePet != null)//to them
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
                    if (mapPlayers.Values.ToList()[a].Pets.BattlePet != null)//to me
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
            //p = new SendPacket();
            //p.PackArray(new byte[] { 20, 8 });
            //t.Send(p);
            //t.CharacterState = PlayerState.inMap;
        }
        public void onWarp_Out(byte portalID, Player src, WarpData To)
        {
          mapPlayers.Remove(src.ID);
        }

        public void UpdateMap()
        {

        }
    }

    public class TentFloor
    {
        byte floorloc;

        cBuildElement CB = new cBuildElement();
        List<TentItem> ItemTent;
        int count { get { return ItemTent.Count; } }
        public TentFloor(byte ID)
        {
            floorloc = ID;
            ItemTent = new List<TentItem>();           
        }

        //public void Destroyed_Item(byte pos)
        //{
        //    for (int a = 0; a < ItemTent.Count; a++)
        //    {
        //        if (ItemTent[a].pos == pos)
        //        {
        //            ItemTent.Remove(ItemTent[a]);
                    
        //        }

        //    }
        //}
        //public void Tool_Manager(byte inv)
        //{
        //    ushort id = 0;
        //    switch (id)
        //    {

        //        case 38058: Add_Tool(38058, 38049); break;
        //    }
        //}
        //void Add_Tool(ushort tool,ushort item)
        //{
        //    CTent c = new CTent();
        //    c.pos =(byte)(count + 1);
        //    c.id = tool;
        //    c.qnt = 1;
        //    Manufacture_Tool(item, c.pos);
        //    ItemTent.Add(c);
        //}
        //void Manufacture_Tool(ushort item,byte pos)
        //{
        //    for (int a = 0; a < ItemTent.Count; a++)
        //    {
        //        if (ItemTent[a].id == item)
        //        {
        //            if (ItemTent[a].ac0 != 0)
        //            {
        //                ItemTent[a].ac0 = pos;
        //            }
        //            else if (ItemTent[a].ac1 != 0)
        //            {
        //                ItemTent[a].ac1 = pos;
        //            }
        //            else 
        //            {
        //                ItemTent[a].ac2 = pos;
        //            }
        //        }
        //    }
        //}
        //public void Send_62_4(Player p)
        //{
            
        //    SendPacket s = new SendPacket();
        //    s.PackArray(new byte[] {62,4 });
        //    s.Pack32(p.ID);
        //    for(byte a =0; a < ItemTent.Count; a++)
        //    {

        //        s.Pack8(ItemTent[a].pos);
        //        s.Pack8(0);
        //        s.Pack16(ItemTent[a].id);
        //        s.Pack8(ItemTent[a].ax);
        //        s.PackArray(new byte[] { 0, 0, 0 });
        //        s.Pack8(ItemTent[a].ay);
        //        s.PackArray(new byte[] { 0, 0, 0 });
        //        s.Pack8(ItemTent[a].qnt);
        //        s.PackArray(new byte[] { 0, 0, 0 });
        //        s.Pack8(ItemTent[a].rotate); 
        //        s.Pack8(ItemTent[a].especial); 
        //        s.PackArray(new byte[] { 0, 0, 0 });
        //        s.Pack8(ItemTent[a].ac0);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac1);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac2);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac3);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac4);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac5);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac6);
        //        s.Pack8(0);
        //        s.Pack8(ItemTent[a].ac7);
        //        s.PackArray(new byte[] { 0, 0, 0, 0, 0 });
        //        s.Pack8(ItemTent[a].ukn);
        //        s.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0 });
        //        s.Pack8(ItemTent[a].floor);
        //        s.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        //    }

        //    p.Send(s);

        //}
        //public void Remove_Object_Tent(byte pos)
        //{
        //    for (int a = 0; a < ItemTent.Count; a++)
        //    {
        //        if (ItemTent[a].pos == pos)
        //        {
        //            ItemTent.Remove(ItemTent[a]);
        //            //add here pack inventory
        //        }

        //    }

        //}
        //public void Add_Object_inTent(byte inv, byte ax, byte ay, byte qnt)
        //{
        //    ushort id = 0;///get id item in inventory [inv][]
        //                  ///Get item in item.dat
        //    CTent t = new CTent();
        //    t.pos = Convert.ToByte(count + 1);
        //    t.id = id;
        //    t.ax = ax;
        //    t.ay = ay;
        //    t.qnt = qnt;
            

        //    SendPacket s = new SendPacket();
        //    s.PackArray(new byte[] { 62, 1 });
        //    s.Pack8(t.pos);
        //    s.Pack8(0);
        //    s.Pack16(t.id);
        //    s.Pack8(t.ax);
        //    s.PackArray(new byte[] { 0, 0, 0 });
        //    s.Pack8(t.ay);
        //    s.PackArray(new byte[] { 0, 0, 0 });
        //    s.Pack8(t.qnt);
        //    s.PackArray(new byte[] { 0, 0, 0 });
        //    s.Pack8(t.rotate); // rotate
        //    s.Pack8(t.especial); //
        //    s.PackArray(new byte[] { 0, 0, 0 });
        //    s.Pack8(t.ac0);
        //    s.Pack8(0);
        //    s.Pack8(t.ac1);
        //    s.Pack8(0);
        //    s.Pack8(t.ac2);
        //    s.Pack8(0);
        //    s.Pack8(t.ac3);
        //    s.Pack8(0);
        //    s.Pack8(t.ac4);
        //    s.Pack8(0);
        //    s.Pack8(t.ac5);
        //    s.Pack8(0);
        //    s.Pack8(t.ac6);
        //    s.Pack8(0);
        //    s.Pack8(t.ac7);
        //    s.PackArray(new byte[] { 0, 0, 0, 0, 0});
        //    s.Pack8(t.ukn);
        //    s.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0});
        //    s.Pack8(t.floor);
        //    s.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0,0,0 });

        //    ItemTent.Add(t);

        //}
        //public void Rotate_Move_Object(byte pos,byte ax,byte ay,byte qnt, byte rotate)
        //{
        //    for (int a = 0; a < ItemTent.Count; a++)
        //    {
        //        if (ItemTent[a].pos == pos)
        //        {
        //            ItemTent[a].pos = pos;
        //            ItemTent[a].ax = ax;
        //            ItemTent[a].ay = ay;
        //            ItemTent[a].qnt = qnt;
        //            ItemTent[a].rotate = rotate;


        //        }

        //    }

        //}
    }
    
}
