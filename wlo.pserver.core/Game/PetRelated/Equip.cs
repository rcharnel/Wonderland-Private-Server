using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFiles;
using Network;


namespace Game.Code.PetRelated
{
    public class PetEquipManager
    {
        readonly object m_Lock = new object();

        Equip[] equippedItems;
        protected PhoneixNpc npcData;
        protected Player owner;
        protected byte Slot;
        protected byte m_amity;

        int m_currexp, m_curhp, m_cursp, m_skillpoint, m_potential, m_spd;
        long m_totalexp = 0;
        Affinity element;
        RebornJob job;

        Action<SendPacket> Send;

        public PetEquipManager(Action<SendPacket> src)
        {
            Send = src;
            equippedItems = new Equip[6];
            for (int a = 0; a < 6; a++)
                equippedItems[a] = new Equip();

        }

        /// <summary>
        /// Access the list of equipped Items
        /// </summary>
        /// <param name="index"></param>
        /// <returns>returns the equippedItem</returns>
        public Equip this[byte index]
        {
            get
            {
                lock (m_Lock)
                {
                    return equippedItems[index - 1];
                }
            }
        }

        public virtual void Clear()
        {
        }

        public Func<byte, Item> onWearEquip;
        public Func<Item, byte, bool, bool> onEquip_Remove;

        #region Properties

        #region Definitions

        /// <summary>
        /// Gets/Sets the SkillPoints for the Pet
        /// </summary>
        public UInt16 SkillPoints
        {
            get
            {
                lock (m_Lock)
                {
                    return (ushort)m_skillpoint;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_skillpoint = value;
                }
            }
        }
        /// <summary>
        /// Adjusts the Pet's total accumalted Exp
        /// </summary>
        public long TotalExp
        {
            get
            {
                lock (m_Lock)
                {
                    return (Level * 6) + m_totalexp;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_totalexp = value;
                    m_totalexp -= (Level * 6);
                }
            }
        }
        /// <summary>
        /// Returns whether a Pet is reborn or not
        /// </summary>
        public bool Reborn
        {
            get
            {
                lock (m_Lock)
                { return (Job != RebornJob.none); }
            }
        }

        /// <summary>
        /// a Pet's level
        /// </summary>
        public byte Level
        {
            get
            {
                lock (m_Lock)
                {
                    int level = 1;

                    long tmp = m_totalexp;

                    while (tmp >= (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], level))
                    {
                        if (!Reborn && level + 1 > 199) break;
                        tmp -= (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], level);
                        level++;
                    }

                    if (Reborn) return (byte)(level - 100);
                    else
                        return (byte)level;
                }
            }
        }
        /// <summary>
        /// A Pets's Current Hp
        /// </summary>
        public virtual Int32 CurHP
        {
            get
            {
                lock (m_Lock)
                {
                    return m_curhp;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_curhp = value;
                }
            }
        }
        /// <summary>
        /// A Pets's Current Sp
        /// </summary>
        public virtual Int32 CurSP
        {
            get
            {
                lock (m_Lock)
                {
                    return m_cursp;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_cursp = value;
                }
            }
        }

        /// <summary>
        /// Base Calculated Max HP
        /// </summary>
        UInt32 MaxHp
        {
            get
            {
                lock (m_Lock) { return (UInt32)(Math.Round(((Math.Pow(Level, 0.35) * Con * 2) + (Level * 1) + (Con * 2) + 180 + 0))); }
            }
        }

        /// <summary>
        /// Base Calculated Max SP
        /// </summary>
        UInt16 MaxSp
        {
            get
            {
                lock (m_Lock)
                { return (UInt16)(Math.Round(((Math.Pow((double)Level, 0.3) * Wis * 3.2) + (Level * 1) + (Wis * 2) + 94 + 0))); }
            }
        }

        /// <summary>
        /// A Pet's Current EXP
        /// </summary>
        public Int32 CurExp
        {
            get
            {
                lock (m_Lock)
                {
                    return m_currexp;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    long expgain = value;

                    while (expgain > 0)
                    {
                        var exptolvl = CalcMaxExp(BitConverter.GetBytes(Reborn)[0], Level);
                        var remainexp = exptolvl - m_currexp;
                        if (m_currexp + expgain >= exptolvl)
                        {
                            SkillPoints += 5;
                            m_currexp = 0;
                            TotalExp += remainexp;
                            expgain -= remainexp;
                            Send8_1(true);
                        }
                        else
                        {
                            TotalExp += expgain;
                            m_currexp += (int)expgain;
                            expgain -= expgain;
                            SendExp();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A Pet's Potential Ammt
        /// </summary>
        public UInt16 Potential
        {
            get { lock (m_Lock) { return (ushort)m_potential; } }
            set { lock (m_Lock) { m_potential = (int)value; } }
        }


        /// <summary>
        /// Element Type
        /// </summary>
        public Affinity Element
        {
            get { lock (m_Lock) { return element; } }
            set { lock (m_Lock) { element = value; } }
        }

        /// <summary>
        /// Rebirth Job Type
        /// </summary>
        public RebornJob Job
        {
            get { lock (m_Lock) { return job; } }
            set { lock (m_Lock) { job = value; } }
        }
        #endregion

        #region Base Stats Calculated by Character Type
        UInt16 baseStr;
        public UInt16 Str
        {
            get
            {
                lock (m_Lock) return (ushort)baseStr;
            }
            set
            {
                lock (m_Lock) baseStr = (ushort)value;
            }
        }

        UInt16 baseInt;
        public UInt16 Int
        {
            get
            {
                lock (m_Lock) return (ushort)baseInt;
            }
            set
            {
                lock (m_Lock) baseInt = (ushort)value;
            }
        }
        UInt16 baseWis;
        public UInt16 Wis
        {
            get
            {
                lock (m_Lock) return (ushort)baseWis;
            }
            set
            {
                lock (m_Lock) baseWis = (ushort)value;
            }
        }
        UInt16 baseCon;
        public UInt16 Con
        {
            get
            {
                lock (m_Lock) return (ushort)baseCon;
            }
            set
            {
                lock (m_Lock) baseCon = (ushort)value;
            }
        }
        UInt16 baseAgi;
        public UInt16 Agi
        {
            get
            {
                lock (m_Lock) return (ushort)baseAgi;
            }
            set
            {
                lock (m_Lock) baseAgi = (ushort)value;
            }
        }
        #endregion

        #region Calculated Stats based on element and job variables
        UInt16 Atk
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case Affinity.Fire:
                        add = (UInt16)(Math.Round(((double)Level * 2.0) + ((double)Str * 2.0)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 1.4) + ((double)Str * 2.0)));
                        break;
                }

                if (Job == RebornJob.Killer)
                    add = (UInt16)Math.Round((double)add * 1.1);

                return (ushort)add;
            }
        }
        UInt16 Matk
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case Affinity.Fire:
                        add = (UInt16)(Math.Round(((double)Level * 1.6) + ((double)Int * 2.0)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 1.4) + ((double)Int * 2.0)));
                        break;
                }

                if (Job == RebornJob.Wit)
                    add = (UInt16)Math.Round((double)add * 1.1);

                return (ushort)add;

            }
        }
        UInt16 Mdef
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case Affinity.Fire:
                        add = (UInt16)(Math.Round(((double)Level * 2.2) + ((double)Wis * 2.2)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 2.0) + ((double)Wis * 2.2)));
                        break;
                }

                if (Job == RebornJob.Priest)
                    add = (UInt16)Math.Round((double)add * 1.1);

                return (ushort)add;
            }
        }
        UInt16 Def
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case Affinity.Earth:
                        add = (UInt16)(Math.Round(((double)Level * 8.0) + ((double)Con * 1.75)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 2.0) + ((double)Con * 1.75)));
                        break;
                }

                if (Job == RebornJob.Warrior)
                {
                    add = (UInt16)Math.Round((double)add * 1.1);
                }
                return (ushort)add;
            }
        }
        UInt16 Spd
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case Affinity.Wind:
                        add = (UInt16)(Math.Round(((double)Level * 2.1) + ((double)Agi * 2.2)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 1.6) + ((double)Agi * 2.2)));
                        break;
                }

                if (Job == RebornJob.Knight || Job == RebornJob.Seer)
                    add = (UInt16)Math.Round((double)add * 1.1);

                return (ushort)add;
            }
        }
        #endregion

        #region Full Stats combines Calculated and Equipped Stats
        /// <summary>
        /// Full HP Stat
        /// </summary>
        public Int32 FullHP { get { lock (m_Lock) { return (int)(MaxHp + EquippedMaxHP); } } }
        /// <summary>
        /// Full SP Stat
        /// </summary>
        public Int32 FullSP { get { lock (m_Lock) { return (int)(MaxSp + EquippedMaxSP); } } }
        /// <summary>
        /// Full ATK Stat
        /// </summary>
        public Int32 FullAtk { get { lock (m_Lock) { return (int)(Atk + EquippedATK); } } }
        /// <summary>
        /// Full DEF Stat
        /// </summary>
        public Int32 FullDef { get { lock (m_Lock) { return (int)(Def + EquippedDEF); } } }
        /// <summary>
        /// Full MAT Stat
        /// </summary>
        public Int32 FullMatk { get { lock (m_Lock) { return (int)(Matk + EquippedMAT); } } }
        /// <summary>
        /// Full MDF Stat
        /// </summary>
        public Int32 FullMdef { get { lock (m_Lock) { return (int)(Mdef + EquippedMDF); } } }
        /// <summary>
        /// Full SPD Stat
        /// </summary>
        public Int32 FullSpd { get { lock (m_Lock) { return (int)(Spd + EquippedSPD); } } }
        #endregion

        #region Equipment Bonuses
        /// <summary>
        /// Total Equipped MAXHP
        /// </summary>
        protected Int32 EquippedMaxHP
        {
            get
            {
                lock (m_Lock)
                {
                    Int32 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += this[i].HP;
                    return nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped MAXSP
        /// </summary>
        protected Int32 EquippedMaxSP
        {
            get
            {
                lock (m_Lock)
                {
                    Int32 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += this[i].SP;
                    return nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped ATK
        /// </summary>
        protected UInt16 EquippedATK
        {
            get
            {
                lock (m_Lock)
                {
                    Int16 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += (Int16)this[i].ATK;
                    return (UInt16)nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped DEF
        /// </summary>
        protected UInt16 EquippedDEF
        {
            get
            {
                lock (m_Lock)
                {
                    Int16 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += (Int16)this[i].DEF;
                    return (UInt16)nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped MAT
        /// </summary>
        protected UInt16 EquippedMAT
        {
            get
            {
                lock (m_Lock)
                {
                    Int16 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += (Int16)this[i].MAT;
                    return (UInt16)nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped MDF
        /// </summary>
        protected UInt16 EquippedMDF
        {
            get
            {
                lock (m_Lock)
                {
                    Int16 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += (Int16)this[i].MDF;
                    return (UInt16)nRet;
                }
            }
        }
        /// <summary>
        /// Total Equipped SPD
        /// </summary>
        protected UInt16 EquippedSPD
        {
            get
            {
                lock (m_Lock)
                {
                    Int16 nRet = 0;
                    for (byte i = 1; i < 7; ++i)
                        nRet += (Int16)this[i].SPD;
                    return (UInt16)nRet;
                }
            }

        }

        #endregion

        #endregion


        /// <summary>
        /// unWears a Character's Equipment
        /// </summary>
        /// <param name="clothesSlot">place to remove Equip</param>
        /// <returns> Invenotry Item Type to place in Inventory</returns>
        public Item unWear(byte clothesSlot)
        {
            lock (m_Lock)
            {
                try
                {
                    if (equippedItems[clothesSlot - 1].ItemID > 0)
                    {
                        Item i = new Item(equippedItems[clothesSlot - 1]);
                        equippedItems[clothesSlot - 1].Clear();
                        return i;
                    }
                }
                catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); }
                return null;
            }
        }

        /// <summary>
        /// Wears a Characters Item
        /// </summary>
        /// <param name="eq">the Item to Place </param>
        /// <returns>the InvItem object to be placed in Inv</returns>
        public Item Wear(Item eq)
        {
            return Wear((byte)eq.Wear_At, eq);
        }
        /// <summary>
        /// Wears a Characters Item
        /// </summary>
        /// <param name="clothesSlot">Slot/place to put item</param>
        /// <param name="eq">the Item to Place </param>
        /// <returns>the InvItem object to be placed in Inv</returns>
        public Item Wear(byte clothesSlot, Item eq)
        {
            lock (m_Lock)
            {
                Item i = null;
                try
                {
                    if (equippedItems[clothesSlot - 1].ItemID != 0)
                        i = unWear(clothesSlot);
                    equippedItems[clothesSlot - 1].CopyFrom(eq);
                }
                catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); i = null; }
                return i;
            }
        }

        /// <summary>
        /// Returns a byte[] of Equips that are worn
        /// </summary>
        public IEnumerable<byte> Worn_Equips
        {
            get
            {
                lock (m_Lock)
                {
                    SendPacket tmp = new SendPacket(false);
                    if (WornCount > 0)
                    {

                        for (byte a = 1; a < 7; a++)
                            if (this[a].ItemID > 0)
                                tmp.Pack16(this[a].ItemID);
                    }
                    return tmp.Buffer;
                }
            }
        }
        /// <summary>
        /// The Equipment list in Byte[] form (includes empty slots)
        /// </summary>
        public IEnumerable<byte> FullEqData
        {
            get
            {
                lock (m_Lock)
                {
                    SendPacket tmp = new SendPacket(false);
                    for (byte n = 1; n < 7; n++)
                        if (this[n].ItemID != 0)
                            tmp.Pack16(this[n].ItemID);
                        else
                            tmp.Pack16(0);

                    return tmp.Buffer;
                }
            }
        }
        /// <summary>
        /// 23 11 Equip packet
        /// </summary>
        public IEnumerable<byte> _23_11Data
        {
            get
            {
                lock (m_Lock)
                {
                    SendPacket tmp = new SendPacket(false);
                    tmp.Pack((byte)23);
                    tmp.Pack((byte)11);
                    if (WornCount > 0)
                    {
                        for (byte n = 1; n < 7; n++)
                        {
                            if (this[n].ItemID != 0)
                            {
                                tmp.Pack16(this[n].ItemID);
                                tmp.Pack((byte)this[n].Damage);
                                tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                            }
                        }
                    }
                    return tmp.Buffer;
                }
            }
        }
        /// <summary>
        /// Total Equips Worn on Character
        /// </summary>
        public byte WornCount
        {
            get
            {
                return (byte)equippedItems.Count(c => c.ItemID > 0);
            }
        }

        /// <summary>
        /// Processes Network Related Packets
        /// This class will also send queue packets that have been generated 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="client"></param>
        public virtual void ProcessSocket(Player src, SendPacket p)
        {
            p.m_nUnpackIndex = 4;

            var a = p.Unpack8();
            var b = p.Unpack8();

            switch (a)
            {
                #region Ac 23
                case 23:
                    {
                        switch (b)
                        {
                            #region item selected to equip
                            case 11:
                                {
                                    byte loc = p.Unpack8();

                                    var item = onWearEquip(loc);
                                    if (item != null)
                                    {
                                        onEquip_Remove(Wear(item), loc, false);
                                        Send8_1();

                                        SendPacket tmp2 = new SendPacket();
                                        tmp2.Pack((byte)5);
                                        tmp2.Pack((byte)2);
                                        tmp2.Pack(src.CharID);
                                        tmp2.Pack16(item.ItemID);
                                        tmp2.SetHeader();
                                        src.CurMap.Broadcast(tmp2, "Ex", src.CharID);

                                        tmp2 = new SendPacket();
                                        tmp2.Pack((byte)23);
                                        tmp2.Pack((byte)17);
                                        tmp2.Pack((byte)loc);
                                        tmp2.Pack((byte)loc);
                                        tmp2.SetHeader();
                                        SendPacket(tmp2);
                                    }
                                }
                                break;
                            #endregion
                            #region item selected to unequip
                            case 12:
                                {
                                    byte loc = p.Unpack8();
                                    byte dst = p.Unpack8();
                                    if ((loc > 0) && (loc < 7) && (dst > 0) && (dst < 51))
                                    {
                                        var eq = unWear(loc);
                                        if (eq != null && eq.ItemID > 0)
                                        {
                                            bool ret = onEquip_Remove(eq, dst, false);

                                            if (!ret)
                                                Wear(eq);
                                            else
                                            {
                                                Send8_1();
                                                SendPacket tmp2 = new SendPacket();
                                                tmp2.Pack((byte)23);
                                                tmp2.Pack((byte)16);
                                                tmp2.Pack((byte)loc);
                                                tmp2.Pack((byte)dst);
                                                tmp2.SetHeader();
                                                SendPacket(tmp2);

                                                tmp2 = new SendPacket();
                                                tmp2.Pack((byte)5);
                                                tmp2.Pack((byte)1);
                                                tmp2.Pack(src.CharID);
                                                tmp2.Pack16(eq.ItemID);
                                                tmp2.SetHeader();
                                                src.CurMap.Broadcast(tmp2, "Ex", src.CharID);
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region item being used
                            case 15:
                                {
                                }
                                break;
                            #endregion
                            #region destroy an item
                            case 124:
                                {
                                }
                                break;
                            #endregion
                        }
                    }
                    break;
                #endregion
            }
        }

        public void Send8_1(bool levelup = false) //this function sets the stas according to pts
        {
            lock (m_Lock)
            {
                PacketBuilder tmp = new PacketBuilder();
                tmp.Begin(false);

                if (levelup)
                {
                    tmp.Add(SendPacket.FromFormat("bbbbl", 8, 2, 36, 1, TotalExp));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 35, 1, Level, 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 37, 1, (Level - 1), 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 38, 1, SkillPoints, 0));
                    CurHP = FullHP;
                    CurSP = FullSP;
                }

                //hp
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 207, 1, EquippedMaxHP, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 25, 1, (CurHP > FullHP) ? FullHP : CurHP, 0));
                //sp
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 208, 1, EquippedMaxSP, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 26, 1, (CurSP > FullSP) ? FullSP : CurSP, 0));
                //str
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 210, 1, EquippedATK, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 41, 1, FullAtk, 0));
                if (levelup) tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 28, 1, Str, 0));
                //con
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 211, 1, EquippedDEF, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 2, 42, 1, FullDef, 0));
                if (levelup)
                {
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 205, 1, FullHP, 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 29, 1, Con, 0));
                }
                //spd
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 214, 1, EquippedSPD, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 45, 1, FullSpd, 0));
                if (levelup) tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 30, 1, Agi, 0));
                //int
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 215, 1, EquippedMAT, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 43, 1, FullMatk, 0));
                if (levelup) tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 27, 1, Int, 0));
                //wis
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 216, 1, EquippedMDF, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 44, 1, FullMdef, 0));
                if (levelup)
                {
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 206, 1, FullSP, 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 33, 1, Wis, 0));
                }

                SendPacket(tmp.End());
            }
        }

        void SendExp()
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 1 });
            p.Pack((byte)36);
            p.Pack((byte)1);
            p.Pack64((ulong)TotalExp);
            p.SetHeader();
            SendPacket(p);
        }

        #region Helper Methods
        int CalcMaxExp(byte rebirth, int Level)
        {
            if (rebirth == 0)
            {
                return (int)Math.Round(Math.Pow((Level + 1), 3.1) + 5);
            }
            else
            {
                if (Level < 150)
                {
                    return (int)Math.Pow((double)(Level + 1), (3.3)) + 50;
                }
                else
                {
                    return (int)Math.Pow((Level + 1), (3.3)) + (int)Math.Pow((Level + 1 - 150), 4.9);
                }
            }
        }
        public void FillHP()
        {
            m_curhp = (int)FullHP;
        }
        public void FillSP()
        {
            m_cursp = FullSP;
        }
        public void SetBaseStat(object stat, object value)
        {
            switch (byte.Parse(stat.ToString()))
            {
                case 28: baseStr = (ushort)value; break;
                case 29: baseCon = (ushort)value; break;
                case 30: baseAgi = (ushort)value; break;
                case 27: baseInt = (ushort)value; break;
                case 33: baseWis = (ushort)value; break;
            }
        }
        public IEnumerable<object[]> GetStatArray()
        {
            List<object[]> tmp = new List<object[]>();
            tmp.Add(new object[] { 36, TotalExp });
            tmp.Add(new object[] { 38, SkillPoints });
            tmp.Add(new object[] { 25, CurHP });
            tmp.Add(new object[] { 26, CurSP });
            tmp.Add(new object[] { 28, baseStr });
            tmp.Add(new object[] { 29, baseCon });
            tmp.Add(new object[] { 30, baseAgi });
            tmp.Add(new object[] { 27, baseInt });
            tmp.Add(new object[] { 33, baseWis });
            return tmp;
        }
        #endregion
    }
}
