using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Maps;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.DataManagement.DataFiles;
using System.Timers;

namespace Wonderland_Private_Server.Code.Objects
{
    #region class tent map
    public class Tent:Map//Map allows the server to treat this as a map however
    {
        readonly object locker = new object();
        Player owner;
        public Dictionary<byte, TentFloor> Floors = new Dictionary<byte, TentFloor>();
        Map Loc;

        public ushort MapX;
        public ushort MapY;
        public bool Closed;
        
        ushort floorcolor = 39062, wallcolor = 39064;
        
        public Tent()
        {
            // holy test need dataobjetct to load
            TentFloor f = new TentFloor(1);
        }
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
                return (owner != null) ? (ushort)owner.ID : (ushort)0;
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
            t.Tent.Floors[1].Send62_4(t);
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
#endregion

    public class TentFloor
    {
        public delegate void BuildDelegate(byte nkey);
        public BuildDelegate BuildTimerEventHandler;
        System.Windows.Forms.Timer t;
        byte floorloc;
        public bool Mill; // have mill tool ? true or false
        byte CurrentJob;
        cCompound2Dat CB = new cCompound2Dat();
        Dictionary<byte ,TentItem> ItemTent; // she started have 2 itens....
        Dictionary<byte, ItemBuild> Build;
        Dictionary<byte, byte> Axis = new Dictionary<byte, byte>();
        int count { get { return ItemTent.Count; } }
        int pount { get { return Build.Count; } }
        public TentFloor(byte ID)
        {
            floorloc = ID;
            ItemTent = new Dictionary<byte ,TentItem>();
            Build = new Dictionary<byte, ItemBuild>();

            LoadItemTent();// holy test
        }
        // holy test  ##################
        public void LoadItemTent()
        {
            CB.Load("C:\\tmp\\Compound2.dat");
            TentItem c = new TentItem();
            c.index = 1;
            c.ItemID = 38049;
            c.tentX = 32;
            c.tentY = 46;
            c.tentZ = 1;
            c.rotate = 0;
            c.especial = 0;
            c.a1 = 0;
            c.a2 = 0;
            c.a3 = 0;
            c.a4 = 0;
            c.a5 = 0;
            c.a6 = 0;
            c.a7 = 0;
            c.a8 = 0;
            c.a9 = 0;
            c.a10 = 0;
            c.ukn = 10;
            c.floor = 0;
            c.pick = 0;
            ItemTent.Add(1,c);
            c = new TentItem();
            c.index = 2;
            c.ItemID = 38055;
            c.tentX = 44;
            c.tentY = 50;
            c.tentZ = 1;
            c.rotate = 0;
            c.especial = 0;
            c.a1 = 0;
            c.a2 = 0;
            c.a3 = 0;
            c.a4 = 0;
            c.a5 = 0;
            c.a6 = 0;
            c.a7 = 0;
            c.a8 = 0;
            c.a9 = 0;
            c.a10 = 0;
            c.ukn = 10;
            c.floor = 0;
            c.pick = 0;
            ItemTent.Add(2,c);
            c = new TentItem();
            c.index = 3;
            c.ItemID = 39046;
            c.tentX = 0;
            c.tentY = 0;
            c.tentZ = 0;
            c.rotate = 0;
            c.especial = 0;
            c.a1 = 0;
            c.a2 = 0;
            c.a3 = 0;
            c.a4 = 0;
            c.a5 = 0;
            c.a6 = 0;
            c.a7 = 0;
            c.a8 = 0;
            c.a9 = 0;
            c.a10 = 0;
            c.ukn = 10;
            c.floor = 0;
            c.pick = 1;
            ItemTent.Add(3, c);
        }
        public void Create_NewObject_Tent(Player src, RecvPacket r)
        {
            byte t = r.Unpack8(2);
            ushort Index = r.Unpack16(4);
            //byte Total = r.Unpack8(13); // total material used compound max 5
            //ushort material1 = src.Inv[r.Unpack8(14)].ItemID; ; //  get item pos inventory
            //byte qnt1 = r.Unpack8(15);

            var formula = CB.buildList[Index]; // get  formula
            
            var item = cGlobal.gItemManager.GetItem(formula.resultID); // get item
            
            if (ItemTent.ContainsKey(t))
            {
                // Tool used == tool
                if (ItemTent[t].ItemID == formula.toolID) // tool == tool
                {
                    if (CheckMaterial()) // have material to building? yes
                    {
                        byte c = (byte)(pount + 1);
                        if (AddBuilding(c, item,formula))
                        {
                            StartBuild(src,c); // started building
                        }
                    }
                }
            }            

        }
        bool CheckMaterial()
        {
            // here need verify item in inventory
            return true;
        }
        bool AddBuilding(byte nKey, cItem ci,cBuildElement c)
        {            
            ItemBuild i = new ItemBuild();
            i.item = ci;
            i.TimerTotal = c.buildTime;
            i.CurTimer = 0;
            i.qnt = 1;

            if (Build.ContainsKey(nKey))
            {
                return false;
            }
            else
            {
                Build.Add(nKey, i);
                return true;
            }               
        }

        void StartBuild(Player src, byte nkey)
        {
            Send64_1(src, nkey);
            Send64_10(src);
            Send64_1(src, nkey);
            Send64_2(src, nkey);
            Send64_10(src);
            Send64_11(src);
            Process(src,Build[nkey].TimerTotal);          
            BuildDone(src, nkey); // if item done send this.            
        }

        void Process(Player src, ushort Interval)
        {
            t = new System.Windows.Forms.Timer();
            t.Interval = Interval; // specify interval time as you want
            //t.Tag = src.UserID;
            t.Tick += new EventHandler(timer_Tick);
            t.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            t.Stop();           
            BuildDone(((Player)sender), CurrentJob);
        }
        #region Send Packet
        public void Send62_4(Player p)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] {62, 4});
            s.Pack32(p.UserID);
            #region Loop
            foreach (var i in ItemTent.Values)
            {
                s.Pack16(i.index);
                s.Pack16(i.ItemID);
                s.Pack32(i.tentX);
                s.Pack32(i.tentY);
                s.Pack32(i.tentZ);
                s.Pack8(i.rotate);
                s.Pack32(i.especial);
                s.Pack16(i.a1);
                s.Pack16(i.a2);
                s.Pack16(i.a3);
                s.Pack16(i.a4);
                s.Pack16(i.a5);
                s.Pack16(i.a6);
                s.Pack16(i.a7);
                s.Pack16(i.a8);
                s.Pack16(i.a9);
                s.Pack16(i.a10);
                s.Pack32(i.ukn);
                s.Pack32(0);
                s.Pack8(i.floor);
                s.Pack8(i.pick);
                s.Pack32(0);
                s.Pack32(0);
            }
            #endregion
            p.Send(s);
        }
        void Send64_10(Player p)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] {64,10,0,0,0,0,0 });            
            p.Send(s);
        }
        void Send64_9(Player p)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] {64,9});
            p.Send(s);
        }
        void Send64_11(Player p)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 64,11});
            p.Send(s);
        }
        void Send64_2(Player p,byte nkey)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 64,2});
            s.Pack8(nkey);
            p.Send(s);
        }
        void Send64_1(Player p, byte nkey)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 64, 1 });
            s.Pack8(nkey);
            s.Pack16(Build[nkey].item.ItemID);
            s.Pack32(0);
            s.Pack8(1);
            p.Send(s);            
        }
        
        void Send64_4(Player p, byte nkey)
        {
            Build.Remove(nkey);
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 64, 4 });            
            s.Pack8(nkey);
            p.Send(s);

        }
        #endregion

        void BuildDone(Player src, byte Currentkey)
        {
            byte c = (byte)(count + 1);
            if (Convert_Item_To_TentItem(ref src, 1, Currentkey,c))
            {
                Send64_9(src);
            }
            else
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 62, 1 });
                s.Pack16(c); // index list item tent
                s.Pack16(ItemTent[c].ItemID);
                s.Pack32(ItemTent[c].tentX); // x
                s.Pack32(ItemTent[c].tentY); // y
                s.Pack32(ItemTent[c].tentZ);//z
                s.Pack8(ItemTent[c].rotate);// (i.rotate);
                s.Pack32(ItemTent[c].especial);//i.especial);
                s.Pack16(0);//.a1);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack16(0);
                s.Pack32(ItemTent[c].ukn);// if tool = 10 i.ukn);
                s.Pack32(0);
                s.Pack8(ItemTent[c].floor);//i.floor);
                s.Pack8(ItemTent[c].pick);
                s.Pack32(0);
                s.Pack32(0);
                src.Send(s);                
            }

            Send64_4(src, Currentkey); // remove item list build
        }
        public void Rotate_Move_Object(Player src,byte pos,byte ax,byte ay, byte floor,byte rotate)
        {  
                if (ItemTent.ContainsKey(pos))
                {                   
                    ItemTent[pos].tentX= ax;
                    ItemTent[pos].tentY = ay;
                    ItemTent[pos].rotate = rotate;

                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] {62,3});
                    s.Pack16(pos);
                    s.Pack32(ax);
                    s.Pack32(ay);
                    s.Pack32(floor);
                    s.Pack8(rotate);
                    src.Send(s);
                    //broadcast current tent here.
                }
            }

        bool Convert_Item_To_TentItem(ref Player src,byte CurFloor,byte curkey,byte nkey )
        {
            var i = Build[curkey].item;  // get item done.
            if (Check_Especial(i.ItemID))
            {
                InvItemCell c = new InvItemCell();
                c.CopyFrom(cGlobal.gItemManager.GetItem(i.ItemID));
                c.Ammt = 1;
                src.Inv.AddItem(c);

                return true;
            }
            else
            {
                //default = auto 0;
                TentItem b = new TentItem();
                b.citem = i;
                b.floor = CurFloor;                
                b.ukn = 10;
                b.tentX = 48;///x
                b.tentY = 46;//y
                b.tentZ = 1; //visible.
                ItemTent.Add(nkey, b);

                return false;
            }
        }
        bool Check_Especial(ushort ItemID)
        {
            switch (ItemID)
            {
                case 38004: return true;//	Great Driver 
                case 38031: return true;//	Brand Iron 
                case 38035: return true;//	Rope Saw	
                case 38036: return true;//	Wooden Saw
                case 38037: return true;//	Whetstone
                case 38040: return true;//Needle
                case 38054: return true;//	Pliers
                case 38058: return true;//	Stone Knife
                    
                default: return false;

            }
        }
        public void StopBuild(Player src, byte i)
        {
            if (Build.ContainsKey(i))
            {
                // stop Build item
                Build[i].CurTimer = 1234; // get cur timer
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {64,3});
                s.Pack8(i);
                src.Send(s);
            }
        }
        public void ContinueBuild(Player src, byte i)
        {
            if (Build.ContainsKey(i))
            {
                Send64_10(src);

                // continue Build item                
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {64,1});
                s.Pack8(i);
                s.Pack16(Build[i].item.ItemID);
                s.Pack32(Build[i].CurTimer);
                s.Pack8(1);
                src.Send(s);
                Send64_2(src, i);
            }
        }
    }


    public class ItemBuild
    {
        //public delegate void TimerTick();
        //public event TimerTick TimerEventHandler;
        public cItem item;               
        public ushort CurTimer;
        public ushort TimerTotal;
        public byte qnt;
       // public bool End { get { return End; } }

       // System.Windows.Forms.Timer t;

        //public void start()
        //{
        //    t = new System.Windows.Forms.Timer();
        //    t.Interval = 15000; // specify interval time as you want
        //    t.Tick += new EventHandler(timer_Tick);

        //    t.Start();
        //}
        //public void timer_Tick(object sender, EventArgs e)
        //{
        //    t.Stop();
        //    if (TimerEventHandler != null)
        //        TimerEventHandler();
        //}
        
    }    
    
}
