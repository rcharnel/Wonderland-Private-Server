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
using Wonderland_Private_Server.DataManagement.DataFiles;
using Wlo.Core;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Player : Character, Fighter,cSocket
    {
        readonly object mylock = new object();
        bool blockupdt;

       
        public int CurInstance;//tmp
        public UInt16 GuildID; //tmp      

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
        WloClient socket;
        int m_nTimer;
        string addr;
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
        Battle m_battle;
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

        public Player(ref Wlo.Core.WloClient sock)
        {
            mylock = new object();
            buffer = new byte[2048];
            //this.host = this;
            this.socket = sock;
            m_nTimer = Environment.TickCount;
            addr = sock.SockAddress();
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
            m_teammembers = new List<Player>();
            
        }
        ~Player()
        {
        }

        #region ThreadSafe  Properties

        #region ClientSocket
        public IPAddress ClientIP { get { return IPAddress.Parse(addr.Split(':')[0]); } }
        public int ClientPort { get { return int.Parse(addr.Split(':')[1]); } }
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
        public WloClient Socket { get { return socket; } }
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

        #region Player.Game
        public bool BlockSave { get; set; }
        public bool inGame { get; set; }
        public PlayerState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }
        public IReadOnlyList<Quest> Completed_Quest { get { return m_started_Quests.Where(c => c.progress == c.total).ToList(); } }
        public SendType DataOut
        {
            get { return dataout; }
            set
            {
                prevdataout = dataout; dataout = value;
                if (prevdataout == SendType.Multi && value == SendType.Normal) Send(MultiPkt);
                else if (value == SendType.Multi) MultiPkt = new SendPacket(false);

            }
        }
        #endregion
        #region Player.Character
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
        #region Player.Guild
        public bool inGuild { get { return (m_guild == null); } }
        public string GuildName { get { return (m_guild != null) ? m_guild.GuildName : ""; } }
        public Guild CurGuild { get { return m_guild; } set { m_guild = value; } }
        public byte[] GuildInsigne { get { return new byte[1]; } }

        #endregion


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

        public Battle BattleScene { get { return m_battle; } set { m_battle = value; } }
        public cRiceBall RiceBall { get { return m_riceball; } }
                
        #endregion

        #region Fighter
        public FighterState BattleState
        {
            get
            {
                if (m_battle != null)
                {
                    if (CurHP != 0)
                        return FighterState.Alive;
                    else
                        return FighterState.Dead;
                }
                return FighterState.Unknown;
            }
        }
        public Skill SkillEffect { get; set; }
        public BattleSide BattlePosition { get; set; }
        public eFighterType TypeofFighter { get { return ( BattlePosition == BattleSide.Watching)? eFighterType.Watcher:eFighterType.player; } }
        public BattleAction myAction { get; set; }
        public UInt16 ClickID { get { return 0; } set { } }
        public UInt32 OwnerID { get { return 0; } set { } }
        public byte GridX { get; set; }
        public byte GridY { get; set; }
        public bool ActionDone { get { return (myAction != null || DateTime.Now > rndend); } }
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
                f.Pack(new byte[] { 13, 6 });
                f.Pack(ID);
                f.Pack((byte)m_teammembers.Count(c => c.ID != ID));
                foreach (Player y in m_teammembers.Where(c => c.ID != ID))
                    f.Pack(y.ID);
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
            QueuePkt = new SendPacket(false);
        }
        public void QueueEnd()
        {
            DatatoSend.Enqueue(QueuePkt);
            QueuePkt = null;
        }

        public void Send(SendPacket pkt, bool queue = false)//TODO finished for multi packs
        {
            if (pkt == null) return;

            if (!killFlag)
            {
                SendPacket o = pkt;
                if (QueuePkt != null)
                    QueuePkt.Pack(pkt.Buffer.ToArray());
                else if (queue)
                    DatatoSend.Enqueue(pkt);
                else
                {
                    switch (DataOut)
                    {
                        case SendType.Multi: MultiPkt.Pack(o.Buffer.ToArray()); break;
                        case SendType.Normal:
                            {
                                //Utilities.LogServices.Log(UserName, pkt.Data.ToArray());
                                killFlag = pkt.KillConnection;
                                socket.SendPacket(pkt);
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
            socket.Disconnect();
        }
        #endregion

        #region Account

        public void SetCharSlot(byte slot)
        {
            this.slot = slot;
        }



        #endregion

        #region Player
        public void Process()
        {
            if (blockupdt) return;
            lock (mylock)
            {
                blockupdt = true;

                blockupdt = false;
            }
        }
        public void onPlayerLogin(uint id)
        {
            if (Friends.Exists(c => c.ID == id))
            {
                SendPacket p = new SendPacket();
                p.Pack(new byte[] { 14, 9 });
                p.Pack(id);
                p.Pack(0);
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
            p.Pack(3);
            p.Pack(ID);
            p.Pack((byte)Eqs.Body);
            p.Pack(LoginMap);
            p.Pack(X);
            p.Pack(Y);
            p.Pack(0); p.Pack(Eqs.Head); p.Pack(0);
            p.Pack(HairColor);
            p.Pack(SkinColor);
            p.Pack(ClothingColor);
            p.Pack(EyeColor);
            p.Pack(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            p.Pack(Eqs.Worn_Equips);
            p.Pack(0);
            p.Pack(CharacterName);
            p.Pack(Nickname);
            p.Pack(0);
            Send(p);
        }
        public SendPacket _3Data()
        {
            SendPacket p = new SendPacket();
            p.Pack(3);
            p.Pack(ID);
            p.Pack((byte)Eqs.Body);
            p.Pack((byte)Eqs.Element);
            p.Pack((byte)Eqs.Level);
            p.Pack(CurrentMap.MapID);
            p.Pack(X);
            p.Pack(Y);
            p.Pack(0); p.Pack(Eqs.Head); p.Pack(0);
            p.Pack(HairColor);
            p.Pack(SkinColor);
            p.Pack(ClothingColor);
            p.Pack(EyeColor);
            p.Pack(Eqs.WornCount);//clothesAmmt); // ammt of clothes
            p.Pack(Eqs.Worn_Equips);
            p.Pack(0); p.Pack(0);
            p.Pack(Eqs.Reborn);
            p.Pack((byte)Eqs.Job);
            p.Pack(CharacterName);
            p.Pack(Nickname);
            p.Pack(255);
            return p;
        }

        public void Send_5_3() //logging in player info
        {
            SendPacket p = new SendPacket();
            p.Pack(new byte[] { 5, 3 });
            p.Pack((byte)Eqs.Element);
            p.Pack((uint)CurHP);
            p.Pack((ushort)CurSP);
            p.Pack(Eqs.Str); //base str
            p.Pack(Eqs.Con); //base con
            p.Pack(Eqs.Int); //base int
            p.Pack(Eqs.Wis); //base wis
            p.Pack(Eqs.Agi); //base agi
            p.Pack((byte)Eqs.Level); //lvl
            p.Pack((ulong)Eqs.TotalExp); //exp ???
            p.Pack((uint)Eqs.FullHP); //max hp
            p.Pack((ushort)Eqs.FullSP); //max sp

            //-------------- 7 DWords
            p.Pack(0);
            p.Pack(0);
            p.Pack(0);
            p.Pack(0);
            p.Pack(0);
            p.Pack(0);
            p.Pack(0);

            //--------------- Skills
            p.Pack(0/*(ushort)MySkills.Count*/);
            //if (MySkills.Count > 0)
            //    p.Pack(MySkills.GetSkillData());
            //p.Pack(1); //ammt of skills
            //p.Pack(188); p.Pack(1); p.Pack(0); p.Pack(0); //skill data
            //--------------- table with rebirth and job
            p.Pack(0); p.Pack(0);
            p.Pack(BitConverter.GetBytes(Eqs.Reborn)[0]); p.Pack((byte)Eqs.Job); p.Pack((byte)Eqs.Potential);

            Send(p);
        }

        #endregion

        #region Friends
        public void SendFriendList()
        {
            SendPacket y = new SendPacket();
            y.Pack(new byte[] { 14, 5 });
            y.Pack(new byte[]{100, 0, 0, 0, 6, 71, 77, 164, 164, 164, 223, 200, 0,      
        0, 0, 0, 0, 28, 175, 125, 26, 28, 175, 125, 26, 0, 0});
            foreach (Character h in Friends)
            {
                y.Pack(h.ID);
                y.Pack(h.CharacterName);
                y.Pack((byte)h.Level);
                y.Pack(BitConverter.GetBytes(h.Reborn)[0]);
                y.Pack((byte)h.Job);
                y.Pack((byte)h.Element);
                y.Pack((byte)h.Body);
                y.Pack(h.Head);
                y.Pack(h.HairColor);
                y.Pack(h.SkinColor);
                y.Pack(h.ClothingColor);
                y.Pack(h.EyeColor);
                y.Pack(h.Nickname);
                y.Pack(0);
            }
            Send(y);
        }
        public void AddFriend(Player t)
        {
            if (Friends.Count == 50) return;
            if (!m_friends.Exists(c => c.ID == t.ID))
                m_friends.Add(t);
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 14, 9 });
            s.Pack(t.ID);
            s.Pack(0);
            Send(s);
            s = new SendPacket();
            s.Pack(new byte[] { 14, 7 });
            s.Pack(t.ID);
            s.Pack("Test");
            Send(s);
        }
        public void DelFriend(uint t)
        {
            if (m_friends.Exists(c => c.ID == t))
                m_friends.Remove(m_friends.Single(c => c.ID == t));
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 14, 4 });
            s.Pack(t);
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
            p.Pack(new byte[] { 14, 1 });
            p.Pack(t.ID);
            p.Pack(((Date == 0) ? BitConverter.GetBytes(DateTime.Now.ToOADate()) : BitConverter.GetBytes(Date)));
            for (int n = 0; n < msg.Length; n++)
                p.Pack((byte)msg[n]);
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
                if (Eqs.Level >= i.Level)
                {
                    DataOut = SendType.Multi;
                    var retrem = Eqs.SetEQ((byte)i.Equippped_At, i);
                    if (retrem != null && retrem.ItemID > 0)
                        Inv.AddItem(retrem, index, false);
                    Eqs.Send8_1();//send ac8
                    SendPacket tmp = new SendPacket();
                    tmp.Pack(new byte[] { 5, 2 });
                    tmp.Pack(ID);
                    tmp.Pack(i.ItemID);
                    CurrentMap.Broadcast(tmp, ID);
                    tmp = new SendPacket();
                    tmp.Pack(new byte[] { 23, 17 });
                    tmp.Pack(index);
                    tmp.Pack(index);
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
                    p.Pack(new byte[] { 23, 16 });
                    p.Pack(src);
                    p.Pack(dst);
                    Send(p);
                    Eqs.Send8_1();
                    p = new SendPacket();
                    p.Pack(new byte[] { 5, 1 });
                    p.Pack(ID);
                    p.Pack(i.ItemID);
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
        //    f.Pack(3);
        //    f.Pack(own.CharacterTemplateID);
        //    foreach (Player u in myTeamMembers.ToArray())
        //        if (u.character.MyTeam.PartyLeader)
        //        {
        //            f.Pack(u.CharacterTemplateID);
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
        //    p.Pack(own.CharacterTemplateID);
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
