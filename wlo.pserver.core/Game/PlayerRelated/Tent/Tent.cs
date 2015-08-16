using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Collections.Concurrent;
using DataFiles;
using Game.Maps;
using Network;

namespace Game.Code
{



    public class Tent : GameMap
    {
        Game.Player _owner;
        GameMap _ownerMap;
        //List<TentFloor> _floors;

        uint _mapx, _mapy;
        bool _locked, _closed;

        ushort _floorcolor = 39062, _wallcolor = 39064;

        public Tent(Game.Player src)
        {
            _owner = src;
            m_mapid = src.CharID;
            m_name = src.CharName + "'s Home";
            //_floors = new List<TentFloor>();
            //_floors.Add(new TentFloor() { MapID = (ushort)_floors.Count });
            //_closed = true;
        }

        public uint X { get { return _mapx; } }
        public uint Y { get { return _mapy; } }

        public void Open()
        {
            if (!_closed) return;
            _mapx = _owner.CurX;
            _mapy = _owner.CurY;
            _ownerMap = (GameMap)_owner.CurMap;
            _ownerMap.onTentOpened(this);
            _owner.Send(SendPacket.FromFormat("bbb", 62, 59, 2));
            _closed = false;
        }
        public void Close()
        {
            if (_closed) return;
            _ownerMap.onTentClosing(this);
            WarpData warp = new WarpData();
            warp.DstMap = (ushort)_ownerMap.MapID;
            warp.DstX_Axis = (ushort)_mapx;
            warp.DstY_Axis = (ushort)_mapy;
            //warp Players  out
            //foreach (var f in Floors)
            //{

            //    for (int a = 0; a < Players.Values.Count; a++)
            //    {
            //        Players.Values.ToList()[a].DataOut = SendType.Multi;
            //        SendPacket warpConf = new SendPacket();
            //        warpConf.PackArray(new byte[] { 20, 7 });
            //        SendPacket tmp = new SendPacket();
            //        tmp.PackArray(new byte[] { 23, 32 });
            //        tmp.Pack(Players.Values.ToList()[a].ID);
            //        Players.Values.ToList()[a].SendPacket(tmp);
            //        tmp = new SendPacket();
            //        tmp.PackArray(new byte[] { 23, 112 });
            //        tmp.Pack(p.ID);
            //        Players.Values.ToList()[a].SendPacket(tmp);
            //        tmp = new SendPacket();
            //        tmp.PackArray(new byte[] { 23, 132 });
            //        tmp.Pack(p.ID);
            //        Players.Values.ToList()[a].SendPacket(tmp);

            //        onWarp_Out(f.Key, ref Players.Values.ToList()[a], warp, false);// warp out of map
            //        p.X = warp.DstX_Axis;//switch x
            //        p.Y = warp.DstY_Axis;//switch y
            //        cGlobal.WLO_World.onTelePort(f.Key, warp, ref p);
            //        p.DataOut = SendType.Normal;

            //    }
            //}
            _closed = true;
        }

        #region Map Base
        public override uint MapID
        {
            get
            {
                return _owner.CharID;
            }

            set
            {
            }
        }
        protected override void onWarp_In(byte portalID, Player src, WarpData from)
        {
            base.onWarp_In(portalID, src, from);

            // add to first floor
            // _floors[0].onPlayerEntered(src);

        }
        protected override void onWarp_Out(byte portalID, Player src, WarpData To, bool toTent)
        {
            base.onWarp_Out(portalID, src, To, toTent);
        }
        protected async override void SendMapInfo(Player t, bool login = false)
        {
            SendPacket p = new SendPacket();



            //build queue
            //storeroom
            #region TentItems (62,4)
            #endregion

            t.Send(SendPacket.FromFormat("bbw", 62, 14, _floorcolor));//floor
            t.Send(SendPacket.FromFormat("bbw", 62, 15, _wallcolor));//wallpaper

            //65,11 ???

            p = new SendPacket();
            p.PackArray(new byte[] { 65, 7 });
            p.Pack16(0);
            t.Send(p);

            //extended Tent Item Info


            //ParkingGarage Info

            //p = new SendPacket();
            //p.PackArray(new byte[] { 23, 138 });
            //t.SendPacket(p);

            RCLibrary.Core.Networking.PacketBuilder tmp = new RCLibrary.Core.Networking.PacketBuilder();
            tmp.Begin(null);
            tmp.Add(SendPacket.FromFormat("bb", 23, 138));

            foreach (var r in m_playerlist)
            {
                tmp.Add(SendPacket.FromFormat("bbd", 23, 122, r.CharID));
                tmp.Add(SendPacket.FromFormat("bbdb", 10, 3, r.CharID, 255));

                if (r.CharID != t.CharID)
                {
                    r.Send(SendPacket.FromFormat("bbd", 23, 122, t.CharID));
                    r.Send(SendPacket.FromFormat("bbdb", 10, 3, t.CharID, 255));

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

            tmp.Add(SendPacket.FromFormat("bb", 23, 102));
            tmp.Add(SendPacket.FromFormat("bb", 20, 8));
            t.Flags.Add(Game.PlayerFlag.InTent); //t.CharacterState = PlayerState.inMap;
        }

        #endregion

        void onPlayerLeaving(Game.Player src)
        {
        }
    }
    

    public class ItemBuild
    {
        //public delegate void TimerTick();
        //public event TimerTick TimerEventHandler;
        //public Item item;               
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
