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
using Game.Maps;

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
            #region Definitions

            readonly object mlock = new object();

            SocketClient m_socket;
            Thread net;

            PlayerFlagManager m_Flags;

            Queue<SendPacket> QueueData;
            SendMode m_sendMode;

            WarpData prevMap;
            WarpData returnSpawnMap;
            WarpData recordMap;
            WarpData gpsMap;

            int slot;
            byte emote;

            User m_useracc;
            Inventory m_inv;
            ClientSettings m_settings;
            //MailManager m_Mail;
            //Friendlist m_friendlist;
            //RiceBall m_riceball;
            //PetList m_petlist;
            //Tent m_tent;
            #endregion


            public Player(SocketClient src,DataFiles.PhxItemDat itemdat)
                : base(src.SendPacket,itemdat)
            {
                m_socket = src;
                m_socket.onConnectionLost += m_socket_onConnectionLost;
                m_socket.onPacketRecved = ProcessSocket;
                QueueData = new Queue<SendPacket>(25);

                
                m_inv = new Inventory(this);
                onWearEquip = m_inv.onWearEquip;
                onEquip_Remove = m_inv.onUnEquip;

                m_useracc = new User();
                Flags = new PlayerFlagManager();
                m_settings = new ClientSettings();
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

            #region ThreadSafe  Properties

            #region Socket
            public TimeSpan TimeIdle { get { return m_socket.Elapsed(); } }
            public bool isDisconnected() { return m_socket.isDisconnected(); }
            public String SockAddress() { return m_socket.SockAddress(); }
            public String LocalPort() { return m_socket.LocalPort(); }
            #endregion

            #region User Account
            public User UserAcc { get { return m_useracc; } }
            //public bool GM { get { return m_gmlvl > 0; } }
            //public bool Busy { get; set; }
            #endregion

            #region Player
            
            
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
            public override uint CharID
            {
                get
                {
                    return (Slot == 1) ? UserAcc.Character1ID : UserAcc.Character2ID;
                }
                set
                {
                    base.CharID = value;
                }
            }
            //public bool BlockSave { get; set; }
            //public bool inGame { get; set; }
            //public PlayerState State
            //{
            //    get
            //    {
            //        if (m_battle != null)
            //        {
            //            if (CurHP != 0)
            //                return PlayerState.InGame_Battling_Alive;
            //        }

            //        return m_state;
            //    }
            //    set
            //    {
            //        m_state = value;
            //    }
            //}
            //public IReadOnlyList<Quest> Completed_Quest { get { return m_started_Quests.Where(c => c.progress == c.total).ToList(); } }
            public ClientSettings Settings
            {
                get
                {
                    return m_settings;
                }
            }
            //public List<Character> Friends
            //{
            //    get
            //    {
            //        return m_friends;
            //    }
            //}
            //public List<Mail> MailBox
            //{
            //    get
            //    {
            //        return mailBox;
            //    }
            //}
            public Inventory Inv { get { return m_inv ?? null; } }
            public EquipManager Eqs { get { return ((EquipManager)this) ?? null; } }
            //public cPetList Pets { get { return m_pets; } }
            //public Tent Tent { get { return m_tent; } }
            //public BattleArea BattleScene { get { return m_battle; } set { m_battle = value; } }
            //public cRiceBall RiceBall { get { return m_riceball; } }
            //public SendType DataOut
            //{
            //    get { return dataout; }
            //    set
            //    {
            //        prevdataout = dataout; dataout = value;
            //        if (prevdataout == SendType.Multi && value == SendType.Normal) Send(MultiPkt);
            //        else if (value == SendType.Multi) MultiPkt = new SendPacket(false, true);

            //    }
            //}
            //public override string CharacterName
            //{
            //    get
            //    {
            //        return (this.GM) ? GMStuff.Name + " " + base.CharacterName : base.CharacterName;
            //    }
            //    set
            //    {
            //        base.CharacterName = value;
            //    }
            //}
            public override IMap CurMap
            {
                get
                {
                    return base.CurMap;
                }
                set
                {
                    if (base.CurMap != null)
                    {
                        if (base.CurMap is GameMap)
                        {
                            prevMap = new WarpData();
                            prevMap.DstMap = (ushort)base.CurMap.MapID;
                            prevMap.DstX_Axis = CurX;
                            prevMap.DstY_Axis = CurY;

                            (base.CurMap as GameMap).onItemDropped_fromMap = null;
                            (base.CurMap as GameMap).onItemPickup_fromMap = null;
                        }
                    }
                    base.CurMap = value;
                    if (base.CurMap is GameMap)
                    {
                        (base.CurMap as GameMap).onItemDropped_fromMap = m_inv.onItemDropped_fromMap;
                        (base.CurMap as GameMap).onItemPickup_fromMap = m_inv.onItemPickedUp_fromMap;
                    }
                }
            }

            public WarpData PrevMap
            {
                get { lock (mlock) return prevMap; }
                set { lock (mlock)prevMap = value; }
            }

            #endregion

            #region Fighter
            //public BattleSide BattlePosition { get; set; }
            //public eFighterType TypeofFighter { get; set; }
            //public BattleAction myAction { get; set; }
            //public UInt16 ClickID { get { return 0; } set { } }
            //public UInt16 OwnerID { get { return 0; } set { } }
            //public byte GridX { get; set; }
            //public byte GridY { get; set; }
            //public bool ActionDone { get { return (myAction != null || DateTime.Now > rndend); } }
            //public DateTime RdEndTime { set { rndend = value; } }
            //public Int32 MaxHP { get { return (Eqs != null) ? Eqs.FullHP : 0; } }
            //public Int16 MaxSP { get { return (Eqs != null) ? (short)Eqs.FullSP : (short)0; } }
            //public override int CurHP
            //{
            //    get
            //    {
            //        return base.CurHP;
            //    }
            //    set
            //    {
            //        base.CurHP = value;
            //    }
            //}
            //public override int CurSP
            //{
            //    get
            //    {
            //        return base.CurSP;
            //    }
            //    set
            //    {
            //        base.CurSP = value;
            //    }
            //}

            #endregion

            #region Team
            //public bool PartyLeader { get { return (m_teammembers[0] == this); } }
            //public List<Player> TeamMembers { get { return m_teammembers.Skip(1).ToList(); } }
            //public bool hasParty { get { return (m_teammembers.Count > 0); } }
            //public SendPacket _13_6Data
            //{
            //    get
            //    {
            //        SendPacket f = new SendPacket();
            //        f.PackArray(new byte[] { 13, 6 });
            //        f.Pack32(ID);
            //        f.Pack8((byte)m_teammembers.Count(c => c.ID != ID));
            //        foreach (Player y in m_teammembers.Where(c => c.ID != ID))
            //            f.Pack32(y.ID);
            //        return f;
            //    }
            //}
            #endregion

            #endregion

            #region ThreadSafe  Methods

            #region ClientSock
            
            public Action<Player> OnDisconnect;

            public void Send(IPacket p)
            {                
                m_socket.SendPacket(p);
            }

            public void Send(IPacket p, RCLibrary.Core.Networking.PacketFlags pFlags)
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
                    p.m_nUnpackIndex = 4;
                    var b = p.Unpack8();
                    Network.ActionCodes.AC ac = Network.ActionCodes.AC.GetAction(b);
                    if (ac != null)
                    {
                        var c = this;
                        ac.ProcessPkt(c, p);
                    }


                    base.ProcessSocket(this, p);
                    m_inv.ProcessSocket(p);
                    //m_setting.ProcessSocket(p);


                }
                catch (Exception f) { DebugSystem.Write(new ExceptionData(f)); m_socket.Disconnect(); }
            }

            public void Disconnect()
            {
                if (!isDisconnected())
                    m_socket.Disconnect();
            }

            //public void Send(SendPacket pkt, bool queue = false)//TODO finished for multi packs
            //{
            //    if (!killFlag)
            //    {
            //        SendPacket o = pkt;
            //        if (QueuePkt != null)
            //            QueuePkt.PackArray(pkt.Data.ToArray());
            //        else if (queue)
            //            DatatoSend.Enqueue(pkt);
            //        else
            //        {
            //            switch (DataOut)
            //            {
            //                case SendType.Multi: MultiPkt.PackArray(o.Data.ToArray()); break;
            //                case SendType.Normal:
            //                    {
            //                        DLogger.NetworkLog(UserName, pkt.Data.ToArray());
            //                        killFlag = pkt.DisconnectAfter();
            //                        var data = pkt.Data.ToArray();
            //                        int offset = 0;
            //                        Encode(ref data);
            //                        int nret = 0;
            //                    retry:
            //                        if (nret < data.Length)
            //                        {
            //                            nret = (UInt16)socket.Send(data.Skip(offset).ToArray(), SocketFlags.None);
            //                            offset += nret; goto retry;
            //                        }
            //                    } break;
            //            }
            //        }
            //    }
            //}

            #endregion

            #region Player
            //public void onPlayerLogin(uint id)
            //{
            //    if (Friends.Exists(c => c.ID == id))
            //    {
            //        SendPacket p = new SendPacket();
            //        p.PackArray(new byte[] { 14, 9 });
            //        p.Pack32(id);
            //        p.Pack8(0);
            //        Send(p);
            //    }
            //    for (int a = 0; a < MailBox.Count; a++)
            //    {
            //        if (MailBox[a].targetid == id && MailBox[a].type == "Send" && MailBox[a].isSent)
            //        {
            //            MailBox[a].isSent = true;
            //            SendMailTo(MailBox[a].targetid, MailBox[a].message);
            //        }
            //    }

            //}
            //public bool ContinueInteraction()
            //{
            //    if (DatatoSend.Count == 1)
            //    {
            //        var f = DatatoSend.Dequeue();
            //        Send(f);
            //        return (obj_interacting != null);
            //    }
            //    else if (DatatoSend.Count > 1)
            //    {
            //        Send(DatatoSend.Dequeue()); return true;
            //    }
            //    return false;
            //}
            //public void Send_3_Me()
            //{
            //    SendPacket p = new SendPacket();
            //    p.Pack8(3);
            //    p.Pack32(ID);
            //    p.Pack8((byte)Eqs.Body);
            //    p.Pack16(LoginMap);
            //    p.Pack16(X);
            //    p.Pack16(Y);
            //    p.Pack8(0); p.Pack8(Eqs.Head); p.Pack8(0);
            //    p.Pack16(HairColor);
            //    p.Pack16(SkinColor);
            //    p.Pack16(ClothingColor);
            //    p.Pack16(EyeColor);
            //    p.Pack8(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            //    p.PackArray(Eqs.Worn_Equips);
            //    p.Pack32(0);
            //    p.PackString(CharacterName);
            //    p.PackString(Nickname);
            //    p.Pack32(0);
            //    Send(p);
            //}
            //public SendPacket _3Data()
            //{
            //    SendPacket p = new SendPacket();
            //    p.Pack8(3);
            //    p.Pack32(ID);
            //    p.Pack8((byte)Eqs.Body);
            //    p.Pack8((byte)Eqs.Element);
            //    p.Pack8((byte)Eqs.Level);
            //    p.Pack16(CurrentMap.MapID);
            //    p.Pack16(X);
            //    p.Pack16(Y);
            //    p.Pack8(0); p.Pack8(Eqs.Head); p.Pack8(0);
            //    p.Pack16(HairColor);
            //    p.Pack16(SkinColor);
            //    p.Pack16(ClothingColor);
            //    p.Pack16(EyeColor);
            //    p.Pack8(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            //    p.PackArray(Eqs.Worn_Equips);
            //    p.Pack32(0); p.Pack8(0);
            //    p.PackBoolean(Eqs.Reborn);
            //    p.Pack8((byte)Eqs.Job);
            //    p.PackString(CharacterName);
            //    p.PackString(Nickname);
            //    p.Pack8(255);
            //    return p;
            //}
            //public void Send_5_3() //logging in player info
            //{
            //    SendPacket p = new SendPacket();
            //    p.PackArray(new byte[] { 5, 3 });
            //    p.Pack8((byte)Eqs.Element);
            //    p.Pack32((uint)CurHP);
            //    p.Pack16((ushort)CurSP);
            //    p.Pack16(Eqs.Str); //base str
            //    p.Pack16(Eqs.Con); //base con
            //    p.Pack16(Eqs.Int); //base int
            //    p.Pack16(Eqs.Wis); //base wis
            //    p.Pack16(Eqs.Agi); //base agi
            //    p.Pack8((byte)Eqs.Level); //lvl
            //    p.Pack64((ulong)Eqs.TotalExp); //exp ???
            //    p.Pack32((uint)Eqs.FullHP); //max hp
            //    p.Pack16((ushort)Eqs.FullSP); //max sp

            //    //-------------- 7 DWords
            //    p.Pack32(0);
            //    p.Pack32(0);
            //    p.Pack32(0);
            //    p.Pack32(0);
            //    p.Pack32(0);
            //    p.Pack32(0);
            //    p.Pack32(0);

            //    //--------------- Skills
            //    p.Pack16(0/*(ushort)MySkills.Count*/);
            //    //if (MySkills.Count > 0)
            //    //    p.PackArray(MySkills.GetSkillData());
            //    //p.Pack16(1); //ammt of skills
            //    //p.Pack16(188); p.Pack16(1); p.Pack16(0); p.Pack8(0); //skill data
            //    //--------------- table with rebirth and job
            //    p.Pack16(0); p.Pack16(0);
            //    p.Pack8(BitConverter.GetBytes(Eqs.Reborn)[0]); p.Pack8((byte)Eqs.Job); p.Pack8((byte)Eqs.Potential);

            //    Send(p);
            //}

            #endregion

        //    #region Friends
        //    public void SendFriendList()
        //    {
        //        SendPacket y = new SendPacket();
        //        y.PackArray(new byte[] { 14, 5 });
        //        y.PackArray(new byte[]{100, 0, 0, 0, 6, 71, 77, 164, 164, 164, 223, 200, 0,      
        //0, 0, 0, 0, 28, 175, 125, 26, 28, 175, 125, 26, 0, 0});
        //        foreach (Character h in Friends)
        //        {
        //            y.Pack32(h.ID);
        //            y.PackString(h.CharacterName);
        //            y.Pack8((byte)h.Level);
        //            y.Pack8(BitConverter.GetBytes(h.Reborn)[0]);
        //            y.Pack8((byte)h.Job);
        //            y.Pack8((byte)h.Element);
        //            y.Pack8((byte)h.Body);
        //            y.Pack8(h.Head);
        //            y.Pack16(h.HairColor);
        //            y.Pack16(h.SkinColor);
        //            y.Pack16(h.ClothingColor);
        //            y.Pack16(h.EyeColor);
        //            y.PackString(h.Nickname);
        //            y.Pack8(0);
        //        }
        //        Send(y);
        //    }
        //    public void AddFriend(Player t)
        //    {
        //        if (Friends.Count == 50) return;
        //        if (!m_friends.Exists(c => c.ID == t.ID))
        //            m_friends.Add(t);
        //        SendPacket s = new SendPacket();
        //        s.PackArray(new byte[] { 14, 9 });
        //        s.Pack32(t.ID);
        //        s.Pack8(0);
        //        Send(s);
        //        s = new SendPacket();
        //        s.PackArray(new byte[] { 14, 7 });
        //        s.Pack32(t.ID);
        //        s.PackString("Test");
        //        Send(s);
        //    }
        //    public void DelFriend(uint t)
        //    {
        //        if (m_friends.Exists(c => c.ID == t))
        //            m_friends.Remove(m_friends.Single(c => c.ID == t));
        //        SendPacket s = new SendPacket();
        //        s.PackArray(new byte[] { 14, 4 });
        //        s.Pack32(t);
        //        Send(s);
        //    }
        //    public bool LoadFriends(string str)
        //    {
        //        foreach (string y in str.Split('&'))
        //        {
        //            if (y.Length > 0 && y != "none")
        //            {
        //                string[] f = y.Split(' ');
        //                m_friends.Add(myhost.CharDataBase.GetCharacterData(uint.Parse(f[0])));
        //            }
        //        }
        //        return true;
        //    }
        //    public string GetFriends_Flag
        //    {
        //        get
        //        {
        //            string query = "";
        //            for (int a = 0; a < m_friends.Count; a++)
        //            {
        //                query += m_friends[a].ID.ToString() + " " + m_friends[a].CharacterName;
        //                if (a < m_friends.Count)
        //                    query += "&";
        //            }
        //            if (query == "")
        //                query += "none";
        //            return query;
        //        }
        //    }
        //    #endregion

        //    #region Mail
        //    public void SendMailTo(Player t, string msg)
        //    {
        //        Mail a = new Mail();
        //        a.message = msg;
        //        a.id = ID;
        //        a.targetid = t.ID;
        //        a.type = "Send";
        //        a.isSent = true;
        //        MailBox.Add(a);
        //        t.RecvMailfrom(this, a.message);
        //    }
        //    public void SendMailTo(uint t, string msg)
        //    {
        //        Mail a = new Mail();
        //        a.message = msg;
        //        a.id = ID;
        //        a.targetid = t;
        //        a.type = "Send";
        //        a.isSent = false;
        //        MailBox.Add(a);
        //    }
        //    public override void RecvMailfrom(Player t, string msg, double Date = 0)
        //    {
        //        base.RecvMailfrom(t, msg, Date);
        //        SendPacket p = new SendPacket();
        //        p.PackArray(new byte[] { 14, 1 });
        //        p.Pack32(t.ID);
        //        p.PackArray(((Date == 0) ? BitConverter.GetBytes(DateTime.Now.ToOADate()) : BitConverter.GetBytes(Date)));
        //        for (int n = 0; n < msg.Length; n++)
        //            p.Pack8((byte)msg[n]);
        //        Send(p);
        //    }
        //    public string GetMailboxFlags()
        //    {
        //        string str = "none";
        //        if (MailBox.Count > 0)
        //        {
        //            for (int a = 0; a < MailBox.Count; a++)
        //            {
        //                str += MailBox[a].id + " " +
        //                    MailBox[a].targetid + " " +
        //                    MailBox[a].when + " " +
        //                    MailBox[a].message + " " +
        //                    MailBox[a].type + " " +
        //                    BitConverter.GetBytes(MailBox[a].isSent)[0].ToString() + " ";

        //                if (a < MailBox.Count)
        //                    str += "&";
        //            }
        //        }
        //        return str;
        //    }
        //    #endregion

        //    #region equips

        //    public bool WearEQ(byte index)
        //    {

        //        bool ret = false;
        //        InvItemCell i = new InvItemCell();

        //        i.CopyFrom(Inv[index]);
        //        if (i.ItemID > 0)
        //        {
        //            Inv[index].Clear();
        //            if (Eqs.Level >= i.Data.Level)
        //            {
        //                DataOut = SendType.Multi;
        //                var retrem = Eqs.SetEQ((byte)i.Data.EquipPos, i);
        //                if (retrem != null && retrem.ItemID > 0)
        //                    Inv.AddItem(retrem, index, false);
        //                Eqs.Send8_1();//send ac8
        //                SendPacket tmp = new SendPacket();
        //                tmp.PackArray(new byte[] { 5, 2 });
        //                tmp.Pack32(ID);
        //                tmp.Pack16(i.ItemID);
        //                CurrentMap.Broadcast(tmp, ID);
        //                tmp = new SendPacket();
        //                tmp.PackArray(new byte[] { 23, 17 });
        //                tmp.Pack8(index);
        //                tmp.Pack8(index);
        //                Send(tmp);
        //                ret = true;
        //                DataOut = SendType.Normal;

        //            }
        //            else
        //            {
        //            }
        //        }
        //        return ret;

        //    }

        //    public bool unWearEQ(byte src, byte dst)
        //    {
        //        bool ret = false;
        //        InvItemCell i = new InvItemCell();

        //        if (Eqs[src].ItemID > 0)
        //        {
        //            i.CopyFrom(Eqs[src]);//copy from clothes
        //            if (i != null && Inv.AddItem(i, dst, false) > 0)
        //            {
        //                Eqs.RemoveEQ(src);
        //                DataOut = SendType.Multi;
        //                SendPacket p = new SendPacket();
        //                p.PackArray(new byte[] { 23, 16 });
        //                p.Pack8(src);
        //                p.Pack8(dst);
        //                Send(p);
        //                Eqs.Send8_1();
        //                p = new SendPacket();
        //                p.PackArray(new byte[] { 5, 1 });
        //                p.Pack32(ID);
        //                p.Pack16(i.ItemID);
        //                CurrentMap.Broadcast(p, ID);
        //                ret = true;
        //                DataOut = SendType.Normal;
        //            }
        //            else if (i != null)
        //                Eqs.SetEQ(src, i);
        //        }
        //        return ret;

        //    }

        //    #endregion

        //    #region Team
        //    //public void KickMember(Player t)
        //    //{
        //    //    MemberLeave(t);
        //    //}
        //    //public void MakeLeader()
        //    //{
        //    //    SendPacket f = new SendPacket();
        //    //    f.Header(13, 15);
        //    //    f.Pack8(3);
        //    //    f.Pack32(own.CharacterTemplateID);
        //    //    foreach (Player u in myTeamMembers.ToArray())
        //    //        if (u.character.MyTeam.PartyLeader)
        //    //        {
        //    //            f.Pack32(u.CharacterTemplateID);
        //    //        }
        //    //    f.SetSize();
        //    //    leader = true;
        //    //    own.currentMap.Broadcast(f);

        //    //}
        //    //public void MemberLeave(Player l)
        //    //{
        //    //    if (l.character.MyTeam.leader)
        //    //        EndTeam();
        //    //    else if (myTeamMembers.Contains(l))
        //    //    {
        //    //        Rem(l);
        //    //    }
        //    //    Rem(l);
        //    //    l.character.MyTeam.Leave();
        //    //}
        //    //public void Leave()
        //    //{
        //    //    Send_5(53);
        //    //    Send_5(54);
        //    //    Send_5(183);
        //    //    SendPacket p = new SendPacket();
        //    //    p.Header(13, 4);
        //    //    p.Pack32(own.CharacterTemplateID);
        //    //    p.SetSize();
        //    //    own.currentMap.Broadcast(p);
        //    //    leader = false;
        //    //    myTeamMembers.Clear();
        //    //}
        //    //public void EndTeam()
        //    //{
        //    //    //end party
        //    //    foreach (Player s in myTeamMembers.ToArray())
        //    //        s.character.MyTeam.Leave();
        //    //}

        //    public void onPartyjoined(Player partyowner)
        //    {

        //    }
        //    #endregion
            #endregion

            #region Properties
            

            //public Inventory Inv { get { return m_inv; } }
            
           
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
            public string DisplayName { get { return SockAddress() + " ID: " + UserAcc.UserID + " User: " + UserAcc.UserName + " Char: " + CharName; } }

            #endregion

            #region IEventRequester
            public object WhoIam { get { return this; } }
            public bool isThere { get { return !isDisconnected(); } }
            #endregion

            #region ISocket
                        
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
