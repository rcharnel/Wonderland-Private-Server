using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using Server.Events;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RCLibrary.Core.Networking;
using Network;
using Game.Code;

    namespace Game
    {
        
        public class PlayerFlagManager
        {
            List<PlayerFlag> m_Flags;

            public PlayerFlagManager()
            {
                m_Flags = new List<PlayerFlag>();
            }

            public void Add(params PlayerFlag[] flag)
            {
                foreach (var f in flag)
                    if (!m_Flags.Contains(f))
                        m_Flags.Add(f);
            }
            public void Remove(params PlayerFlag[] flag)
            {
                foreach (var f in flag)
                    if (m_Flags.Contains(f))
                        m_Flags.Remove(f);
            }
            public bool HasFlag(PlayerFlag flag)
            {
                return m_Flags.Contains(flag);
            }
        }


        public class Player : Game.Character, IDisposable, INotifyPropertyChanged
        {
            readonly object mlock = new object();

            Client3 m_socket;
            Thread net;

            PlayerFlagManager m_Flags;

            public Queue<SendPacket> QueueData;
            SendMode m_sendMode;

            //WarpData prevMap;
            //WarpData returnSpawnMap;
            //WarpData recordMap;
            //WarpData gpsMap;

            byte emote;

            User m_useracc;
            //Game.Code.Inventory m_inv;
            //clientSettings m_setting;
            //MailManager m_Mail;
            //Friendlist m_friendlist;
            //RiceBall m_riceball;
            //PetList m_petlist;
            //Tent m_tent;

            public Player(Client3 src,DataFiles.PhxItemDat itemdat)
                : base(src.SendPacket,itemdat)
            {
                m_socket = src;
                m_socket.onConnectionLost += m_socket_onConnectionLost;
                m_socket.onPacketRecved = ProcessSocket;
                QueueData = new Queue<SendPacket>(25);


                //m_inv = new Inventory(this);
                //onWearEquip = m_inv.onWearEquip;
                //onEquip_Remove = m_inv.onUnEquip;

                m_useracc = new User();
                Flags = new PlayerFlagManager();
                //m_setting = new clientSettings();
                //m_friendlist = new Friendlist(new Action<SendPacket>(SendPacket));
                //m_Mail = new MailManager(this);
                //m_tent = new Tent(this);
                //m_petlist = new PetList(this);

                while (m_socket.m_IncomingPackets.Count > 0)
                {
                    IPacket p;
                    m_socket.m_IncomingPackets.TryDequeue(out p);
                    ProcessSocket(p);
                }


            }
            ~Player()
            {
            }


            public void Dispose()
            {
                m_useracc = null;
            }

            public override void Clear()
            {
                m_Flags = new PlayerFlagManager();
                //m_inv.RemoveAll(true);
                QueueData = new Queue<SendPacket>(25);
                UserAcc.Clear();
                base.Clear();
            }

            #region Properties
            public PlayerFlagManager Flags
            {
                get
                {
                    lock (mlock) return m_Flags;
                }
                set
                {
                    lock (mlock) m_Flags = value;
                }
            }
            //public Inventory Inv { get { return m_inv; } }
            //public override IMap CurMap
            //{
            //    get
            //    {
            //        return base.CurMap;
            //    }
            //    set
            //    {
            //        if (base.CurMap != null)
            //        {
            //            if (base.CurMap is GameMap)
            //            {
            //                prevMap = new WarpData();
            //                prevMap.DstMap = (ushort)base.CurMap.MapID;
            //                prevMap.DstX_Axis = CurX;
            //                prevMap.DstY_Axis = CurY;

            //                (base.CurMap as GameMap).onItemDropped_fromMap = null;
            //                (base.CurMap as GameMap).onItemPickup_fromMap = null;
            //            }
            //        }
            //        base.CurMap = value;
            //        if (base.CurMap is GameMap)
            //        {
            //            (base.CurMap as GameMap).onItemDropped_fromMap = m_inv.onItemDropped_fromMap;
            //            (base.CurMap as GameMap).onItemPickup_fromMap = m_inv.onItemPickedUp_fromMap;
            //        }
            //    }
            //}
            //public WarpData PrevMap
            //{
            //    get { lock (mlock) return prevMap; }
            //    set { lock (mlock)prevMap = value; }
            //}
            public byte Emote { get { lock (mlock)return emote; } set { lock (mlock)emote = value; } }
            //public MailManager Mail { get { return m_Mail; } }
            //public Friendlist MyFriends { get { return m_friendlist; } }
            //public RiceBall Disguise { get { return m_riceball; } }
            //public PetList Pets { get { return m_petlist; } }
            //public Tent Tent { get { return m_tent; } }

            #endregion

            #region Event
            void m_socket_onConnectionLost()
            {
                if (net != null && net.IsAlive) net.Abort();
                if (OnDisconnect != null) OnDisconnect(this);
            }
            public void onTick_Tick()
            {
                OnPropertyChanged("DisplayName");
            }
            #endregion

            #region Gui Update
            public string DisplayName { get { return SockAddress() + " ID: " + UserID + " User: " + UserAcc.UserName + " Char: " + CharName; } }

            #endregion

            #region IEventRequester
            public object WhoIam { get { return this; } }
            public bool isThere { get { return !isDisconnected(); } }
            #endregion

            #region ISocket

            public Action<Player> OnDisconnect;

            public void Send(SendPacket p)
            {
                p.SetHeader();
                p.Encode();
                m_socket.SendPacket(p);
            }

            public void Send(SendPacket p, RCLibrary.Core.Networking.PacketFlags pFlags)
            {
                p.Flags = pFlags;
                Send(p);

            }


            public void ProcessSocket(IPacket g)
            {
                try
                {
                    RCLibrary.Core.Networking.Packet p;
                    p = (RCLibrary.Core.Networking.Packet)g;

                    if (m_socket.isDisconnected()) { return; }
                    DebugSystem.Write(DebugItemType.Network_Heavy, "Recv Data from {0} Data:{1}", SockAddress(), p.ToString());

                    var b = p.Unpack8();
                    Network.ActionCodes.AC ac = Network.ActionCodes.AC.GetAction(b);
                    if (ac != null)
                    {
                        var c = this;
                        ac.ProcessPkt(c, p);
                    }


                    base.ProcessSocket(this, p);
                    //m_inv.ProcessSocket(p);
                    //m_setting.ProcessSocket(p);


                }
                catch (Exception f) { DebugSystem.Write(new ExceptionData(f)); m_socket.Disconnect(); }
            }

            public void Disconnect()
            {
                if (!isDisconnected())
                    m_socket.Disconnect();
            }

            public TimeSpan TimeIdle { get { return m_socket.Elapsed(); } }
            public bool isDisconnected() { return m_socket.isDisconnected(); }
            public String SockAddress() { return m_socket.SockAddress(); }
            public String LocalPort() { return m_socket.LocalPort(); }
            #endregion

            #region Inotify Property
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            #endregion

            #region User Methods/Properties
            public User UserAcc { get { return m_useracc; } }
            public int UserID { get { return (m_useracc == null) ? 0 : m_useracc.DataBaseID; } }
            //public clientSettings Settings { get { return m_setting; } }
            #endregion

            #region Game.Mail

            #endregion


            public bool Load_CharacterInfo(Character data)
            {
                if (data == null) return false;
                CharID = data.CharID;
                CharName = data.CharName;
                Slot = data.Slot;
                Head = data.Head;
                Body = data.Body;
                TotalExp = 95000478;//data.TotalEXP
                CharName = data.CharName;
                NickName = data.NickName;
                LoginMap = data.LoginMap;
                CurSP = data.CurSP;
                CurHP = data.CurHP;
                CurX = data.CurX;
                CurY = data.CurY;
                HairColor = data.HairColor;
                SkinColor = data.SkinColor;
                ClothingColor = data.ClothingColor;
                EyeColor = data.EyeColor;
                SetGold((int)data.Gold);
                Element = data.Element;
                Job = data.Job;
                Potential = data.Potential;
                foreach (var stat in data.GetStatArray())
                    SetBaseStat(stat[0], stat[1]);

                for (byte a = 1; a < 7; a++)
                    this[a].CopyFrom(data[a]);

                //remove
                FillHP();
                FillSP();

                return true;
            }
            public bool ContinueInteraction()
            {
                if (QueueData.Count == 1)
                {
                    m_socket.SendPacket(QueueData.Dequeue());
                    return false;// (object_interactingwith != null);
                }
                else if (QueueData.Count > 1)
                {
                    m_socket.SendPacket(QueueData.Dequeue()); return true;
                }
                return false;
            }
        }
    }
