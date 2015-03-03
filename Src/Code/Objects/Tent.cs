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
using System.Threading;
using System.Collections.Concurrent;
using Wlo.Core;

namespace Wonderland_Private_Server.Code.Objects
{
    #region class tent map
    public class Tent:Map//Map allows the server to treat this as a map however
    {
        readonly object locker = new object();
        Player owner;
        public Dictionary<byte, TentFloor> Floors = new Dictionary<byte, TentFloor>();
        Map Loc;

        TentInventoryManager Warehouse;

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
            Warehouse = new TentInventoryManager(src);
            Closed = true;
            MType = MapType.Tent;
            owner = src;
            Floors.Add(1, new TentFloor(1));//lets do floor one for now

            LoadTentData();
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
        
        void LoadTentData()
        {
            //pre setup tent with base
        }

        public void Open()
        {
            if (!Closed) return;
            MapX = owner.X;
            MapY = owner.Y;
            Loc = owner.CurrentMap;
            Loc.onTentOpened(this);
            SendPacket opentent = new SendPacket();
            opentent.Pack(new byte[] { 62, 59 });
            opentent.Pack(2);
            owner.Send(opentent);
            Closed = false;
        }
        public void Close()
        {
            //if (Closed) return;
            //Loc.onTentClosing(this);
            //WarpData warp = new WarpData();
            //warp.DstMap = Loc.MapID;
            //warp.DstX_Axis = MapX;
            //warp.DstY_Axis = MapY;
            ////warp Players  out
            //foreach(var f in Floors)
            //{

            //    for(int a = 0; a< Players.Values.Count;a++)
            //    {
            //        Players.Values.ToList()[a].DataOut = SendType.Multi;
            //        SendPacket warpConf = new SendPacket();
            //        warpConf.Pack(new byte[] { 20, 7 });
            //        SendPacket tmp = new SendPacket();
            //        tmp.Pack(new byte[] { 23, 32 });
            //        tmp.Pack(Players.Values.ToList()[a].ID);
            //        Players.Values.ToList()[a].Send(tmp);
            //        tmp = new SendPacket();
            //        tmp.Pack(new byte[] { 23, 112 });
            //        tmp.Pack(p.ID);
            //        Players.Values.ToList()[a].Send(tmp);
            //        tmp = new SendPacket();
            //        tmp.Pack(new byte[] { 23, 132 });
            //        tmp.Pack(p.ID);
            //        Players.Values.ToList()[a].Send(tmp);

            //        onWarp_Out(f.Key, ref  Players.Values.ToList()[a], warp, false);// warp out of map
            //        p.X = warp.DstX_Axis;//switch x
            //        p.Y = warp.DstY_Axis;//switch y
            //        cGlobal.WLO_World.onTelePort(f.Key, warp, ref p);
            //        p.DataOut = SendType.Normal;
                    
            //    }
            //}
            //Closed = true;
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
            p.Pack(new byte[] { 62, 14 });
            p.Pack(floorcolor);
            t.Send(p);

            //wallpaper
            p = new SendPacket();
            p.Pack(new byte[] { 62, 15 });
            p.Pack(wallcolor);
            t.Send(p);

            //65,11 ???

            p = new SendPacket();
            p.Pack(new byte[] { 65, 7 });
            p.Pack(0);
            t.Send(p);

            //extended Tent Item Info


            //ParkingGarage Info

            p = new SendPacket();
            p.Pack(new byte[] { 23, 138 });
            t.Send(p);

            foreach( var r in Players.Values)
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
            //p = new SendPacket();
            //p.Pack(new byte[] { 20, 8 });
            //t.Send(p);
            //t.CharacterState = PlayerState.inMap;
        }

        public override void UpdateMap()
        {
             do
            {

                Thread.Sleep(100);
            }
             while (true);
        }
    }
#endregion

    public class TentFloor
    {
        byte floorloc;
        ConcurrentDictionary<byte ,TentItem> ItemTent; // she started have 2 itens....



        public delegate void BuildDelegate(byte nkey);
        public BuildDelegate BuildTimerEventHandler;

        System.Windows.Forms.Timer t;

        
        public bool Mill; // have mill tool ? true or false
        byte CurrentJob;

        
        Dictionary<byte, ItemBuild> Build;
        Dictionary<byte, byte> Axis = new Dictionary<byte, byte>();
        int count { get { return ItemTent.Count; } }
        int pount { get { return Build.Count; } }
        public TentFloor(byte ID)
        {
            floorloc = ID;
            ItemTent = new ConcurrentDictionary<byte, TentItem>();
            Build = new Dictionary<byte, ItemBuild>();

            LoadItemTent();// holy test

            //New Character Setup
            if(ItemTent.Count == 0)
            {

            }
            if(ItemTent.Values.Count(c=>c.ItemID == 38049) == 0)
            {
                TentItem tmp = new TentItem();
                tmp.CopyFrom(cGlobal.gItemManager.GetItem(38049));
                tmp.tentX = 32;
                tmp.tentY = 46;
                tmp.tentZ = 1;
                tmp.ukn = 10;
                ItemTent.TryAdd(1, tmp);
                
            }
            if (ItemTent.Values.Count(c => c.ItemID == 38055) == 0)
            {
                TentItem tmp = new TentItem();
                tmp.CopyFrom(cGlobal.gItemManager.GetItem(38055));
                tmp.tentX = 44;
                tmp.tentY = 50;
                tmp.tentZ = 1;
                tmp.ukn = 10;
                ItemTent.TryAdd(2, tmp);
            }
        }

        // holy test  ##################
        public void LoadItemTent()
        {
        }
        public void Create_NewObject_Tent(Player src, RecvPacket r)
        {
            //byte t = r.Unpack8(2);
            //ushort Index = r.Unpack16(4);
            //byte Total = r.Unpack8(13); // total material used compound max 5
            //ushort material1 = src.Inv[r.Unpack8(14)].ItemID; ; //  get item pos inventory
            //byte qnt1 = r.Unpack8(15);

            //var formula = cGlobal.gCompoundDat.buildList[Index]; // get  formula
            
            //var item = cGlobal.gItemManager.GetItem(formula.resultID); // get item
            
            //if (ItemTent.ContainsKey(t))
            //{
            //    // Tool used == tool
            //    if (ItemTent[t].ItemID == formula.toolID) // tool == tool
            //    {
            //        if (CheckMaterial()) // have material to building? yes
            //        {
            //            byte c = (byte)(pount + 1);
            //            if (AddBuilding(c, item,formula))
            //            {
            //                StartBuild(src,c); // started building
            //            }
            //        }
            //    }
            //}            

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
            s.Pack(new byte[] {62, 4});
            s.Pack(p.UserID);
            #region Loop
            foreach (var i in ItemTent.Values)
            {
                s.Pack(i.index);
                s.Pack(i.ItemID);
                s.Pack(i.tentX);
                s.Pack(i.tentY);
                s.Pack(i.tentZ);
                s.Pack(i.rotate);
                s.Pack(i.especial);
                s.Pack(i.a1);
                s.Pack(i.a2);
                s.Pack(i.a3);
                s.Pack(i.a4);
                s.Pack(i.a5);
                s.Pack(i.a6);
                s.Pack(i.a7);
                s.Pack(i.a8);
                s.Pack(i.a9);
                s.Pack(i.a10);
                s.Pack(i.ukn);
                s.Pack(0);
                s.Pack(floorloc);
                s.Pack(i.pick);
                s.Pack(0);
                s.Pack(0);
            }
            #endregion
            p.Send(s);
        }
        void Send64_10(Player p)
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] {64,10,0,0,0,0,0 });            
            p.Send(s);
        }
        void Send64_9(Player p)
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] {64,9});
            p.Send(s);
        }
        void Send64_11(Player p)
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 64,11});
            p.Send(s);
        }
        void Send64_2(Player p,byte nkey)
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 64,2});
            s.Pack(nkey);
            p.Send(s);
        }
        void Send64_1(Player p, byte nkey)
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 64, 1 });
            s.Pack(nkey);
            s.Pack(Build[nkey].item.ItemID);
            s.Pack(0);
            s.Pack(1);
            p.Send(s);            
        }
        
        void Send64_4(Player p, byte nkey)
        {
            Build.Remove(nkey);
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 64, 4 });            
            s.Pack(nkey);
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
                s.Pack(new byte[] { 62, 1 });
                s.Pack(c); // index list item tent
                s.Pack(ItemTent[c].ItemID);
                s.Pack(ItemTent[c].tentX); // x
                s.Pack(ItemTent[c].tentY); // y
                s.Pack(ItemTent[c].tentZ);//z
                s.Pack(ItemTent[c].rotate);// (i.rotate);
                s.Pack(ItemTent[c].especial);//i.especial);
                s.Pack(0);//.a1);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(0);
                s.Pack(ItemTent[c].ukn);// if tool = 10 i.ukn);
                s.Pack(0);
                s.Pack(ItemTent[c].floor);//i.floor);
                s.Pack(ItemTent[c].pick);
                s.Pack(0);
                s.Pack(0);
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
                    s.Pack(new byte[] {62,3});
                    s.Pack(pos);
                    s.Pack(ax);
                    s.Pack(ay);
                    s.Pack(floor);
                    s.Pack(rotate);
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
                ////default = auto 0;
                //TentItem b = new TentItem();
                //b.citem = i;
                //b.floor = CurFloor;                
                //b.ukn = 10;
                //b.tentX = 48;///x
                //b.tentY = 46;//y
                //b.tentZ = 1; //visible.
                //ItemTent.Add(nkey, b);

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
                s.Pack(new byte[] {64,3});
                s.Pack(i);
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
                s.Pack(new byte[] {64,1});
                s.Pack(i);
                s.Pack(Build[i].item.ItemID);
                s.Pack(Build[i].CurTimer);
                s.Pack(1);
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

        //    t.Init();
        //}
        //public void timer_Tick(object sender, EventArgs e)
        //{
        //    t.Stop();
        //    if (TimerEventHandler != null)
        //        TimerEventHandler();
        //}
        
    }    
    
}
