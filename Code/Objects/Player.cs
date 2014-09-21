using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Wonderland_Private_Server.Code.Interface;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.GM;


namespace Wonderland_Private_Server.Code.Objects
{
    public class Player : Character, Fighter, Network.cSocket
    {
        readonly object mylock = new object();
        Thread wrk;

        #region Acc Def
        uint m_dbuserid;
        string m_user, m_deletekey;
        public int m_im, m_gmlvl;
        public Dictionary<byte, uint> m_charids;
        int slot;
        #endregion

        #region Client Def
        Queue<SendPacket> DatatoSend;
        SendPacket QueuePkt, MultiPkt;
        SendType dataout, prevdataout;
        Socket socket;
        int m_nTimer;
        IPAddress ip;
        int port;
        bool killFlag;
        int holdSize;
        byte[] holdData;
        byte[] buffer;
        #endregion

        #region FighterDef
        DateTime rndend;
        #endregion

        #region Player Def
        PlayerState m_state;
        TradeManager m_trade;
        //BattleArea m_battle;
        Guild m_guild;
        cRiceBall m_riceball;
        cPetList m_pets;
        List<Player> m_teammembers;
        inGameSettings m_settings;
        InventoryManager m_Inv;
        List<Character> m_friends;
        List<Quest> m_started_Quests;
        public WarpData PrevMap { get; set; }
        WarpData lastMap;
        WarpData recordMap;
        WarpData gpsMap;
        Tent m_tent;
        #endregion

        public Maps.MapObject object_interactingwith;
        Dictionary<int, ActionCodes.AC> aclist;

        public Player(ref Socket sock)
        {
            wrk = new Thread(new ThreadStart(Recv_Send));
            mylock = new object();
            buffer = new byte[2048];
            this.host = this;
            this.socket = sock;
            this.socket.Blocking = true;
            socket.LingerState = new LingerOption(false, 0);
            //socket.ReceiveTimeout = 5000;
            socket.SendTimeout = 5000;
            m_nTimer = Environment.TickCount;
            ip = ((IPEndPoint)sock.RemoteEndPoint).Address;
            port = ((IPEndPoint)sock.RemoteEndPoint).Port;
            wrk.Start();
            cGlobal.ThreadManager.Add(wrk);
            aclist = new Dictionary<int, ActionCodes.AC>();
            base.Clear();
            BlockSave = true;
            m_im = 0;
            m_gmlvl = 0;
            m_charids = new Dictionary<byte, uint>();
            m_charids.Add(1, 0);
            m_charids.Add(2, 0);
            m_started_Quests = new List<Quest>();
            DatatoSend = new Queue<SendPacket>();
            m_settings = new inGameSettings();
            m_friends = new List<Character>(50);
            m_Inv = new InventoryManager(this);
            m_pets = new cPetList(this);
            m_tent = new Tent(this);
            m_riceball = new cRiceBall(this);
        }
        ~Player()
        {
            wrk.Abort();
        }

        #region ThreadSafe  Properties

        #region ClientSocket
        public IPAddress ClientIP { get { return ip; } }
        public int ClientPort { get { return port; } }
        public bool isAlive
        {
            get
            {
                return !killFlag;
            }
        }
        public TimeSpan Elapsed
        {
            get
            {
                return new TimeSpan(0, 0, 0, 0, Environment.TickCount - m_nTimer);
            }
        }
        #endregion

        #region Account
        public string Cipher { get { return m_deletekey; } set { m_deletekey = value; } }
        public uint UserID
        {
            get
            {
                return (uint)(m_dbuserid + 10000);
            }
        }
        public uint DataBaseID
        {
            get
            {
                return m_dbuserid;
            }
            set
            {
                m_dbuserid = value;
            }
        }
        public string UserName { get { return m_user; } set { m_user = value; } }
        public byte Slot { get { return (byte)slot; } }
        public bool GM { get { return m_gmlvl > 0; } }
        public bool Busy { get; set; }

        #endregion

        #region Player

        public bool BlockSave { get; set; }
        public bool inGame { get; set; }
        public PlayerState State
        {
            get
            {
                //if (m_battle != null)
                //{
                //    if (CurHP != 0)
                //        return PlayerState.InGame_Battling_Alive;
                //}

                return m_state;
            }
            set
            {
                m_state = value;
            }
        }
        public IReadOnlyList<Quest> Completed_Quest { get { return m_started_Quests.Where(c => c.progress == c.total).ToList(); } }
        public inGameSettings Settings
        {
            get
            {
                return m_settings;
            }
        }
        public List<Character> Friends
        {
            get
            {
                return m_friends;
            }
        }
        public List<Mail> MailBox
        {
            get
            {
                return mailBox;
            }
        }
        public InventoryManager Inv { get { return m_Inv ?? null; } }
        public EquipementManager Eqs { get { return ((EquipementManager)this) ?? null; } }
        public cPetList Pets { get { return m_pets; } }
        public Tent Tent { get { return m_tent; } }
       // public BattleArea BattleScene { get { return m_battle; } set { m_battle = value; } }
        public cRiceBall RiceBall { get { return m_riceball; } }
        public SendType DataOut
        {
            get { return dataout; }
            set
            {
                prevdataout = dataout; dataout = value;
                if (prevdataout == SendType.Multi && value == SendType.Normal) Send(MultiPkt);
                else if (value == SendType.Multi) MultiPkt = new SendPacket(false, true);

            }
        }
        public override uint ID
        {
            get
            {
                return (uint)((int)UserID + ((slot == 2) ? 4500000 : 0));
            }
        }
        public override string CharacterName
        {
            get
            {
                return (this.GM) ?  GMStuff.Name + " " + base.CharacterName : base.CharacterName;
            }
            set
            {
                base.CharacterName = value;
            }
        }

        #endregion

        #region Fighter
        public BattleSide BattlePosition { get; set; }
        public eFighterType TypeofFighter { get; set; }
        //public BattleAction myAction { get; set; }
        public UInt16 ClickID { get { return 0; } set { } }
        public UInt16 OwnerID { get { return 0; } set { } }
        public byte GridX { get; set; }
        public byte GridY { get; set; }
        public bool ActionDone { get { return false; /*(myAction != null || DateTime.Now > rndend);*/ } }
        public DateTime RdEndTime { set { rndend = value; } }
        public Int32 MaxHP { get { return (Eqs != null) ? Eqs.FullHP : 0; } }
        public Int16 MaxSP { get { return (Eqs != null) ? (short)Eqs.FullSP : (short)0; } }
        public override int CurHP
        {
            get
            {
                return base.CurHP;
            }
            set
            {
                base.CurHP = value;
            }
        }
        public override int CurSP
        {
            get
            {
                return base.CurSP;
            }
            set
            {
                base.CurSP = value;
            }
        }

        #endregion

        #region Team
        public bool PartyLeader { get { return (m_teammembers[0] == this); } }
        public List<Player> TeamMembers { get { return m_teammembers.Skip(1).ToList(); } }
        public bool hasParty { get { return (m_teammembers.Count > 0); } }
        public SendPacket _13_6Data
        {
            get
            {
                SendPacket f = new SendPacket();
                f.PackArray(new byte[] { 13, 6 });
                f.Pack32(ID);
                f.Pack8((byte)m_teammembers.Count(c => c.ID != ID));
                foreach (Player y in m_teammembers.Where(c => c.ID != ID))
                    f.Pack32(y.ID);
                return f;
            }
        }
        #endregion

        #endregion

        #region ThreadSafe  Methods
        #region ClientSock
        void PacketProcess(RecvPacket p)
        {
           //ut DLogger.NetworkLog(this.UserName, p.A, p.B, p.Data.ToArray());
            Player me = this;
            //recieved a packet
            if (aclist.ContainsKey(p.A))
                aclist[p.A].ProcessPkt(ref me, p);
            else
            {

                var pkt = cGlobal.GetActionCode(p.A);
                if (pkt != null)
                {
                    aclist.Add(p.A, pkt);
                    aclist[p.A].ProcessPkt(ref me, p);
                }
                else
                   Utilities.LogServices.Log("AC "+p.A+","+p.B+" has not been coded");
            }
        }
        public void QueueStart()
        {
            QueuePkt = new SendPacket(false, true);
        }
        public void QueueEnd()
        {
            DatatoSend.Enqueue(QueuePkt);
            QueuePkt = null;
        }
        void Recv_Send()
        {
            do
            {
                #region Read
                try
                {
                    int nRef = -1;
                    nRef = socket.Receive(buffer, 0, 2048, SocketFlags.None);
                    if (nRef == 0) break;
                    m_nTimer = Environment.TickCount;
                    byte[] tmp2 = buffer.Take(nRef).ToArray();
                    byte[] data;
                    int len = tmp2.Length;
                    int origLen = len;

                    Encode(ref tmp2);
                    //check for leftover data
                    if (holdSize > 0)
                    {
                        len += holdSize;
                        data = new byte[len];
                        holdData.CopyTo(data, 0);
                        tmp2.CopyTo(data, holdSize);
                        holdSize = 0;
                        holdData = null;
                    }
                    else
                    {
                        data = new byte[len]; tmp2.CopyTo(data, 0);
                    }

                    int at = 0;

                    while (at < (len - 4))
                    {
                        if ((data[at] == 244) && (data[at + 1] == 68))
                        {
                            int size = data[at + 2] + (data[at + 3] << 8);
                            if ((size + 4) > (len - at)) //not all packet present, so store remainder
                            {
                                holdSize = len - at;
                                holdData = new byte[holdSize];
                                Array.Copy(data, at, holdData, 0, holdSize);
                                at = len;
                            }
                            else //full packet should be in this block
                            {
                                RecvPacket tmp = new RecvPacket();
                                tmp.PackArray(data.Skip(4).ToArray(), size);
                                PacketProcess(tmp);
                                at += size + 4;
                            }
                        }
                        else
                            at++;
                    }

                    if (at < len) //we have leftover data too small to be a header, put on hold
                    {
                        holdSize = len - at;
                        holdData = new byte[holdSize];
                        Array.Copy(data, at, holdData, 0, holdSize);
                        at = len;
                    }

                    len = 0;
                }
                catch (ObjectDisposedException t) { Disconnect(); }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10057)
                        Disconnect();
                    else if (ex.ErrorCode != (int)SocketError.WouldBlock)
                    {
                       Utilities.LogServices.Log(ex);
                        Disconnect();
                    }
                }
                catch (Exception r)
                {
                     Utilities.LogServices.Log(r);
                }

                #endregion
                Thread.Sleep(135);
            }
            while (!killFlag);
            killFlag = true;
        }
        public void Send(SendPacket pkt, bool queue = false)//TODO finished for multi packs
        {
            if (pkt == null) return;

            if (!killFlag)
            {
                SendPacket o = pkt;
                if (QueuePkt != null)
                    QueuePkt.PackArray(pkt.Data.ToArray());
                else if (queue)
                    DatatoSend.Enqueue(pkt);
                else
                {
                    switch (DataOut)
                    {
                        case SendType.Multi: MultiPkt.PackArray(o.Data.ToArray()); break;
                        case SendType.Normal:
                            {
                                //Utilities.LogServices.Log(UserName, pkt.Data.ToArray());
                                killFlag = pkt.DisconnectAfter();
                                var data = pkt.Data.ToArray();
                                int offset = 0;
                                Encode(ref data);
                                int nret = 0;
                            retry:
                                if (nret < data.Length)
                                {
                                    nret = (UInt16)socket.Send(data.Skip(offset).ToArray(), SocketFlags.None);
                                    offset += nret; goto retry;
                                }
                            } break;
                    }
                }
            }
        }
        public void Send(SendPacket pkt, uint ignore)//TODO finished for multi packs
        {
            if (ignore == ID) return;
            Send(pkt);
        }
        public void Disconnect()
        {
            if (killFlag) return;
            killFlag = true;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        void Encode(ref byte[] data)
        {
            for (int n = 0; n < data.Length; n++)
            {
                data[n] = (byte)(173 ^ data[n]);
            }
        }
        #endregion

        #region Account

        public void SetCharSlot(byte slot)
        {
            this.slot = slot;
        }



        #endregion

        #region Player
        public void onPlayerLogin(uint id)
        {
            if (Friends.Exists(c => c.ID == id))
            {
                SendPacket p = new SendPacket();
                p.PackArray(new byte[] { 14, 9 });
                p.Pack32(id);
                p.Pack8(0);
                Send(p);
            }
            for (int a = 0; a < MailBox.Count; a++)
            {
                if (MailBox[a].targetid == id && MailBox[a].type == "Send" && MailBox[a].isSent)
                {
                    MailBox[a].isSent = true;
                    SendMailTo(MailBox[a].targetid, MailBox[a].message);
                }
            }

        }
        public bool ContinueInteraction()
        {
            if (DatatoSend.Count == 1)
            {
                var f = DatatoSend.Dequeue();
                Send(f);
                return (object_interactingwith != null);
            }
            else if (DatatoSend.Count > 1)
            {
                Send(DatatoSend.Dequeue()); return true;
            }
            return false;
        }
        public void Send_3_Me()
        {
            SendPacket p = new SendPacket();
            p.Pack8(3);
            p.Pack32(ID);
            p.Pack8((byte)Eqs.Body);
            p.Pack16(LoginMap);
            p.Pack16(X);
            p.Pack16(Y);
            p.Pack8(0); p.Pack8(Eqs.Head); p.Pack8(0);
            p.Pack16(HairColor);
            p.Pack16(SkinColor);
            p.Pack16(ClothingColor);
            p.Pack16(EyeColor);
            p.Pack8(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            p.PackArray(Eqs.Worn_Equips);
            p.Pack32(0);
            p.PackString(CharacterName);
            p.PackString(Nickname);
            p.Pack32(0);
            Send(p);
        }
        public SendPacket _3Data()
        {
            SendPacket p = new SendPacket();
            p.Pack8(3);
            p.Pack32(ID);
            p.Pack8((byte)Eqs.Body);
            p.Pack8((byte)Eqs.Element);
            p.Pack8((byte)Eqs.Level);
            p.Pack16(CurrentMap.MapID);
            p.Pack16(X);
            p.Pack16(Y);
            p.Pack8(0); p.Pack8(Eqs.Head); p.Pack8(0);
            p.Pack16(HairColor);
            p.Pack16(SkinColor);
            p.Pack16(ClothingColor);
            p.Pack16(EyeColor);
            p.Pack8(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            p.PackArray(Eqs.Worn_Equips);
            p.Pack32(0); p.Pack8(0);
            p.PackBoolean(Eqs.Reborn);
            p.Pack8((byte)Eqs.Job);
            p.PackString(CharacterName);
            p.PackString(Nickname);
            p.Pack8(255);
            return p;
        }
        public void Send_5_3() //logging in player info
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 5, 3 });
            p.Pack8((byte)Eqs.Element);
            p.Pack32((uint)CurHP);
            p.Pack16((ushort)CurSP);
            p.Pack16(Eqs.Str); //base str
            p.Pack16(Eqs.Con); //base con
            p.Pack16(Eqs.Int); //base int
            p.Pack16(Eqs.Wis); //base wis
            p.Pack16(Eqs.Agi); //base agi
            p.Pack8((byte)Eqs.Level); //lvl
            p.Pack64((ulong)Eqs.TotalExp); //exp ???
            p.Pack32((uint)Eqs.FullHP); //max hp
            p.Pack16((ushort)Eqs.FullSP); //max sp

            //-------------- 7 DWords
            p.Pack32(0);
            p.Pack32(0);
            p.Pack32(0);
            p.Pack32(0);
            p.Pack32(0);
            p.Pack32(0);
            p.Pack32(0);

            //--------------- Skills
            p.Pack16(0/*(ushort)MySkills.Count*/);
            //if (MySkills.Count > 0)
            //    p.PackArray(MySkills.GetSkillData());
            //p.Pack16(1); //ammt of skills
            //p.Pack16(188); p.Pack16(1); p.Pack16(0); p.Pack8(0); //skill data
            //--------------- table with rebirth and job
            p.Pack16(0); p.Pack16(0);
            p.Pack8(BitConverter.GetBytes(Eqs.Reborn)[0]); p.Pack8((byte)Eqs.Job); p.Pack8((byte)Eqs.Potential);

            Send(p);
        }

        #endregion

        #region Friends
        public void SendFriendList()
        {
            SendPacket y = new SendPacket();
            y.PackArray(new byte[] { 14, 5 });
            y.PackArray(new byte[]{100, 0, 0, 0, 6, 71, 77, 164, 164, 164, 223, 200, 0,      
        0, 0, 0, 0, 28, 175, 125, 26, 28, 175, 125, 26, 0, 0});
            foreach (Character h in Friends)
            {
                y.Pack32(h.ID);
                y.PackString(h.CharacterName);
                y.Pack8((byte)h.Level);
                y.Pack8(BitConverter.GetBytes(h.Reborn)[0]);
                y.Pack8((byte)h.Job);
                y.Pack8((byte)h.Element);
                y.Pack8((byte)h.Body);
                y.Pack8(h.Head);
                y.Pack16(h.HairColor);
                y.Pack16(h.SkinColor);
                y.Pack16(h.ClothingColor);
                y.Pack16(h.EyeColor);
                y.PackString(h.Nickname);
                y.Pack8(0);
            }
            Send(y);
        }
        public void AddFriend(Player t)
        {
            if (Friends.Count == 50) return;
            if (!m_friends.Exists(c => c.ID == t.ID))
                m_friends.Add(t);
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 14, 9 });
            s.Pack32(t.ID);
            s.Pack8(0);
            Send(s);
            s = new SendPacket();
            s.PackArray(new byte[] { 14, 7 });
            s.Pack32(t.ID);
            s.PackString("Test");
            Send(s);
        }
        public void DelFriend(uint t)
        {
            if (m_friends.Exists(c => c.ID == t))
                m_friends.Remove(m_friends.Single(c => c.ID == t));
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 14, 4 });
            s.Pack32(t);
            Send(s);
        }
        public bool LoadFriends(string str)
        {
            foreach (string y in str.Split('&'))
            {
                if (y.Length > 0 && y != "none")
                {
                    string[] f = y.Split(' ');
                    //m_friends.Add(cGlobal.gCharacterDataBase.GetCharacterData(uint.Parse(f[0])));
                }
            }
            return true;
        }
        public string GetFriends_Flag
        {
            get
            {
                string query = "";
                for (int a = 0; a < m_friends.Count; a++)
                {
                    query += m_friends[a].ID.ToString() + " " + m_friends[a].CharacterName;
                    if (a < m_friends.Count)
                        query += "&";
                }
                if (query == "")
                    query += "none";
                return query;
            }
        }
        #endregion

        #region Mail
        public void SendMailTo(Player t, string msg)
        {
            Mail a = new Mail();
            a.message = msg;
            a.id = ID;
            a.targetid = t.ID;
            a.type = "Send";
            a.isSent = true;
            MailBox.Add(a);
            t.RecvMailfrom(this, a.message);
        }
        public void SendMailTo(uint t, string msg)
        {
            Mail a = new Mail();
            a.message = msg;
            a.id = ID;
            a.targetid = t;
            a.type = "Send";
            a.isSent = false;
            MailBox.Add(a);
        }
        public override void RecvMailfrom(Player t, string msg, double Date = 0)
        {
            base.RecvMailfrom(t, msg, Date);
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 14, 1 });
            p.Pack32(t.ID);
            p.PackArray(((Date == 0) ? BitConverter.GetBytes(DateTime.Now.ToOADate()) : BitConverter.GetBytes(Date)));
            for (int n = 0; n < msg.Length; n++)
                p.Pack8((byte)msg[n]);
            Send(p);
        }
        public string GetMailboxFlags()
        {
            string str = "none";
            if (MailBox.Count > 0)
            {
                for (int a = 0; a < MailBox.Count; a++)
                {
                    str += MailBox[a].id + " " +
                        MailBox[a].targetid + " " +
                        MailBox[a].when + " " +
                        MailBox[a].message + " " +
                        MailBox[a].type + " " +
                        BitConverter.GetBytes(MailBox[a].isSent)[0].ToString() + " ";

                    if (a < MailBox.Count)
                        str += "&";
                }
            }
            return str;
        }
        #endregion

        #region equips

        public bool WearEQ(byte index)
        {

            bool ret = false;
            InvItemCell i = new InvItemCell();

            i.CopyFrom(Inv[index]);
            if (i.ItemID > 0)
            {
                Inv[index].Clear();
                if (Eqs.Level >= i.Data.Level)
                {
                    DataOut = SendType.Multi;
                    var retrem = Eqs.SetEQ((byte)i.Data.EquipPos, i);
                    if (retrem != null && retrem.ItemID > 0)
                        Inv.AddItem(retrem, index, false);
                    Eqs.Send8_1();//send ac8
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 5, 2 });
                    tmp.Pack32(ID);
                    tmp.Pack16(i.ItemID);
                    CurrentMap.Broadcast(tmp, ID);
                    tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 23, 17 });
                    tmp.Pack8(index);
                    tmp.Pack8(index);
                    Send(tmp);
                    ret = true;
                    DataOut = SendType.Normal;

                }
                else
                {
                }
            }
            return ret;

        }

        public bool unWearEQ(byte src, byte dst)
        {
            bool ret = false;
            InvItemCell i = new InvItemCell();

            if (Eqs[src].ItemID > 0)
            {
                i.CopyFrom(Eqs[src]);//copy from clothes
                if (i != null && Inv.AddItem(i, dst, false) > 0)
                {
                    Eqs.RemoveEQ(src);
                    DataOut = SendType.Multi;
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 23, 16 });
                    p.Pack8(src);
                    p.Pack8(dst);
                    Send(p);
                    Eqs.Send8_1();
                    p = new SendPacket();
                    p.PackArray(new byte[] { 5, 1 });
                    p.Pack32(ID);
                    p.Pack16(i.ItemID);
                    CurrentMap.Broadcast(p, ID);
                    ret = true;
                    DataOut = SendType.Normal;
                }
                else if (i != null)
                    Eqs.SetEQ(src, i);
            }
            return ret;

        }

        #endregion

        #region Team
        //public void KickMember(Player t)
        //{
        //    MemberLeave(t);
        //}
        //public void MakeLeader()
        //{
        //    SendPacket f = new SendPacket();
        //    f.Header(13, 15);
        //    f.Pack8(3);
        //    f.Pack32(own.CharacterTemplateID);
        //    foreach (Player u in myTeamMembers.ToArray())
        //        if (u.character.MyTeam.PartyLeader)
        //        {
        //            f.Pack32(u.CharacterTemplateID);
        //        }
        //    f.SetSize();
        //    leader = true;
        //    own.currentMap.Broadcast(f);

        //}
        //public void MemberLeave(Player l)
        //{
        //    if (l.character.MyTeam.leader)
        //        EndTeam();
        //    else if (myTeamMembers.Contains(l))
        //    {
        //        Rem(l);
        //    }
        //    Rem(l);
        //    l.character.MyTeam.Leave();
        //}
        //public void Leave()
        //{
        //    Send_5(53);
        //    Send_5(54);
        //    Send_5(183);
        //    SendPacket p = new SendPacket();
        //    p.Header(13, 4);
        //    p.Pack32(own.CharacterTemplateID);
        //    p.SetSize();
        //    own.currentMap.Broadcast(p);
        //    leader = false;
        //    myTeamMembers.Clear();
        //}
        //public void EndTeam()
        //{
        //    //end party
        //    foreach (Player s in myTeamMembers.ToArray())
        //        s.character.MyTeam.Leave();
        //}

        public void onPartyjoined(Player partyowner)
        {

        }
        #endregion
        #endregion
    }
}
