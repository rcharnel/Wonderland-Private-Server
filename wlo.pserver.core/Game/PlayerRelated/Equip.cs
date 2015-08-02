using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFiles;
using RCLibrary.Core.Networking;
using PhoenixData;
using Network;


namespace Game.Code
{
    public class Equip : Item
    {

        #region Properties
        public Int32 HP
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 207) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 207) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 SP
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 208) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 208) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 ATK
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 210) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 210) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 DEF
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 211) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 211) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 MAT
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 215) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 215) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 MDF
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 216) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 216) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 SPD
        {
            get
            {
                int val = 0;
                val += (Data.StatusType[0] == 214) ? (Int32)Data.StatusUp[0] : 0;
                val += (Data.StatusType[1] == 214) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 Resist
        {
            get
            {
                int val = 0;
                return val;
            }
        }

        public Int32 Crit
        {
            get
            {
                return /*(Data.SpecialStatus == eSpecialStatus.Critical_Increase) ? Data.ItemRank * 2 + 10 :*/ 0;
            }
        }

        #endregion

        public IEnumerable<byte> ToArray()
        {
            if (ItemID == 0) return null;

            SendPacket tmp = new SendPacket();
            tmp.Pack16(ItemID);
            tmp.Pack8(Damage);
            tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            return tmp.Buffer;
        }
    }

    public class EquipManager
    {
        readonly object m_Lock = new object();
        PhxItemDat _ItemManager;
        Equip[] equippedItems;

        byte head;
        int m_currexp, m_curhp, m_cursp, gold, m_skillpoint, m_potential;
        long m_totalexp = 0;
        BodyStyle body;
        Affinity element;
        RebornJob job;

        Action<IPacket> Send;

        public EquipManager(Action<IPacket> src, PhxItemDat itemsrc)
        {
            _ItemManager = itemsrc;
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
        /// Gets/Sets the SkillPoints for the Character
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
        /// Adjusts the Player's total accumalted Exp
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
        /// Returns whether a player is reborn or not
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
        /// a Players Gold Amount
        /// </summary>
        public uint Gold
        {
            get
            {
                lock (m_Lock)
                {
                    return (uint)gold;
                }
            }
        }

        /// <summary>
        /// a Character's level
        /// </summary>
        public virtual byte Level
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
        /// A Character's Current Hp
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
        /// A Character's Current Sp
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
        /// A Character's Current EXP
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
        /// A Character Potential Ammt
        /// </summary>
        public UInt16 Potential
        {
            get { lock (m_Lock) { return (ushort)m_potential; } }
            set { lock (m_Lock) { m_potential = (int)value; } }
        }

        /// <summary>
        /// SubType of Character body Chosen
        /// </summary>
        public byte Head
        {
            get { lock (m_Lock) { return head; } }
            set { lock (m_Lock) { head = value; } }
        }

        /// <summary>
        /// BodyType
        /// </summary>
        public BodyStyle Body
        {
            get { lock (m_Lock) { return body; } }
            set { lock (m_Lock) { body = value; } }
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
        protected UInt16 baseStr;
        public UInt16 Str
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Iris)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Lique)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Daniel)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Sid)
                        add = 2;

                    return (ushort)(baseStr + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                {
                    int add = 0;
                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Iris)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Lique)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Daniel)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Sid)
                        add = 2;

                    baseStr = (ushort)(value - (Level * add));
                }
            }
        }
        protected UInt16 baseInt;
        public UInt16 Int
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Vanessa)
                        add = 3;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Jessica)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Betty)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Maria)
                        add = 2;
                    else if (body == BodyStyle.Small_Male && (HairStyle_SmallM)Head == HairStyle_SmallM.Rocco)
                        add = 2;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Konnotsuroko)
                        add = 2;
                    return (ushort)(baseInt + (Level * add));
                }

            }
            set
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Vanessa)
                        add = 3;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Jessica)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Betty)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Maria)
                        add = 2;
                    else if (body == BodyStyle.Small_Male && (HairStyle_SmallM)Head == HairStyle_SmallM.Rocco)
                        add = 2;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Konnotsuroko)
                        add = 2;

                    baseInt = (ushort)(value - (Level * add));
                }
            }
        }
        protected UInt16 baseWis;
        public UInt16 Wis
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Jessica)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.More)
                        add = 2;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Konnotsuroko)
                        add = 1;
                    return (ushort)(baseWis + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Jessica)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.More)
                        add = 2;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Konnotsuroko)
                        add = 1;

                    baseWis = (ushort)(value - (Level * add));
                }
            }
        }
        protected UInt16 baseCon;
        public UInt16 Con
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Iris)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Daniel)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Sid)
                        add = 1;
                    return (ushort)(baseCon + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                {
                    int add = 0;
                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Iris)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Daniel)
                        add = 2;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Kurogane)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.Sid)
                        add = 1;

                    baseCon = (ushort)(value - (Level * add));
                }
            }
        }
        protected UInt16 baseAgi;
        public UInt16 Agi
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Lique)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Betty)
                        add = 2;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Maria)
                        add = 1;
                    else if (body == BodyStyle.Small_Male && (HairStyle_SmallM)Head == HairStyle_SmallM.Rocco)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.More)
                        add = 1;
                    return (ushort)(baseAgi + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                {
                    int add = 0;

                    if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Karin)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Lique)
                        add = 1;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Betty)
                        add = 2;
                    else if (body == BodyStyle.Small_Female && (HairStyle_SmallF)Head == HairStyle_SmallF.Nina)
                        add = 1;
                    else if (body == BodyStyle.Big_Female && (HairStyle_BigF)Head == HairStyle_BigF.Maria)
                        add = 1;
                    else if (body == BodyStyle.Small_Male && (HairStyle_SmallM)Head == HairStyle_SmallM.Rocco)
                        add = 1;
                    else if (body == BodyStyle.Big_Male && (HairStyle_BigM)Head == HairStyle_BigM.More)
                        add = 1;

                    baseAgi = (ushort)(value - (Level * add));
                }
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
                    PacketBuilder tmp = new PacketBuilder();
                    tmp.Begin(null);
                    if (WornCount > 0)
                    {

                        for (byte a = 1; a < 7; a++)
                            if (this[a].ItemID > 0)
                                tmp.Add(this[a].ItemID);
                    }
                    return tmp.End();
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
                    PacketBuilder tmp = new PacketBuilder();
                    tmp.Begin(null);
                    for (byte n = 1; n < 7; n++)
                        if (this[n].ItemID != 0)
                            tmp.Add(this[n].ItemID);
                        else
                            tmp.Add((ushort)0);

                    return tmp.End();
                }
            }
        }
        /// <summary>
        /// 23 11 Equip packet
        /// </summary>
        public byte[] _23_11Data
        {
            get
            {
                lock (m_Lock)
                {
                    PacketBuilder tmp = new PacketBuilder();
                    tmp.Begin();
                    tmp.Add((byte)23);
                    tmp.Add((byte)11);
                    if (WornCount > 0)
                    {
                        for (byte n = 1; n < 7; n++)
                        {
                            if (this[n].ItemID != 0)
                            {
                                tmp.Add(this[n].ItemID);
                                tmp.Add(this[n].Damage);
                                tmp.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                            }
                        }
                    }
                    return tmp.End();
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

        #region Data Storage
        public List<long[]> Stat_toSave
        {
            get
            {
                lock (m_Lock)
                {
                    List<long[]> tmp = new List<long[]>();
                    tmp.Add(new long[] { 36, TotalExp, 0 });
                    tmp.Add(new long[] { 38, SkillPoints, 0 });
                    tmp.Add(new long[] { 25, CurHP, 0 });
                    tmp.Add(new long[] { 26, CurSP, 0 });
                    tmp.Add(new long[] { 28, baseStr, Potential });
                    tmp.Add(new long[] { 29, baseCon, Potential });
                    tmp.Add(new long[] { 30, baseAgi, Potential });
                    tmp.Add(new long[] { 27, baseInt, Potential });
                    tmp.Add(new long[] { 33, baseWis, Potential });
                    return tmp;
                }
            }
        }
        public void LoadStats(List<long[]> src)
        {
            lock (m_Lock)
            {
                foreach (var t in src)
                {
                    switch (t[0])
                    {
                        case 25: CurHP = (int)t[1]; break;
                        case 26: CurSP = (ushort)t[1]; break;
                        case 38: SkillPoints = (ushort)t[1]; break;
                        case 36: TotalExp = (long)t[1]; break;
                        case 28: Str = (ushort)t[1]; break;
                        case 29: Con = (ushort)t[1]; break;
                        case 30: Agi = (ushort)t[1]; break;
                        case 27: Int = (ushort)t[1]; break;
                        case 33: Wis = (ushort)t[1]; break;
                    }
                    Potential = (byte)t[2];
                }
            }
        }

        public void LoadStats(string s)
        {
            var stats = s.Split(' ');

            CurHP = int.Parse(stats[0]);
            CurSP = int.Parse(stats[1]);
            job = (RebornJob)int.Parse(stats[2]);
            element = (Affinity)int.Parse(stats[3]);
            m_skillpoint = int.Parse(stats[4]);
            Str = ushort.Parse(stats[5]);
            Int = ushort.Parse(stats[6]);
            Wis = ushort.Parse(stats[7]);
            Con = ushort.Parse(stats[8]);
            Agi = ushort.Parse(stats[9]);
            TotalExp = long.Parse(stats[10]);
            m_potential = int.Parse(stats[11]);
            
        }

        public string StatString
            {
                get
                {
                    string s = CurHP + " " + CurSP + " " + (byte)Job + " " + (byte)Element +
                        " " + SkillPoints + " " + Str + " " + Int + " " + Wis + " " + Con + " " + Agi + " "
                        + TotalExp + " " + Potential;
                    return s;
                }
            }

        public Dictionary<byte, uint[]> EqData
        {
            get
            {
                lock (m_Lock)
                {
                    Dictionary<byte, uint[]> tmp = new Dictionary<byte, uint[]>();
                    for (byte a = 1; a < 7; a++)
                        tmp.Add(a, new uint[] { this[a].ItemID, this[a].Damage, this[a].Ammt, (uint)this[a].Wear_At, 0, 0, 0, 0 });
                    return tmp;
                }
            }
        }
        #endregion

        /// <summary>
        /// Processes Network Related Packets
        /// This class will also send queue packets that have been generated 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="client"></param>
        /// 
        public virtual void ProcessSocket(Player src, RecievePacket p)
        {
            p.SetPtr();

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
                                        tmp2.Pack8(5);
                                        tmp2.Pack8(2);
                                        tmp2.Pack32(src.CharID);
                                        tmp2.Pack16(item.ItemID);
                                        //src.CurMap.Broadcast(tmp2, "Ex", src.CharID);

                                        tmp2 = new SendPacket();
                                        tmp2.Pack8(23);
                                        tmp2.Pack8(17);
                                        tmp2.Pack8(loc);
                                        tmp2.Pack8(loc);
                                        Send(tmp2);
                                    }
                                } break;
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
                                                tmp2.Pack8(23);
                                                tmp2.Pack8(16);
                                                tmp2.Pack8(loc);
                                                tmp2.Pack8(dst);
                                                Send(tmp2);

                                                tmp2 = new SendPacket();
                                                tmp2.Pack8(5);
                                                tmp2.Pack8(1);
                                                tmp2.Pack32(src.CharID);
                                                tmp2.Pack16(eq.ItemID);
                                                //src.CurMap.Broadcast(tmp2, "Ex", src.CharID);
                                            }
                                        }
                                    }
                                } break;
                            #endregion
                            #region item being used
                            case 15:
                                {
                                } break;
                            #endregion
                            #region destroy an item
                            case 124:
                                {
                                } break;
                            #endregion
                        }
                    } break;
                #endregion
            }
        }

        public void Send8_1(bool levelup = false) //this function sets the stas according to pts
        {
            lock (m_Lock)
            {
                PacketBuilder tmp = new PacketBuilder();
                tmp.Begin(null);

                if (levelup)
                {
                    tmp.Add(SendPacket.FromFormat("bbbbl", 8, 1, 36, 1, TotalExp));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 35, 1, Level, 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 37, 1, (Level - 1), 0));
                    tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 38, 1, SkillPoints, 0));
                    CurHP = FullHP;
                    CurSP = FullSP;
                }

                //hp
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 207, 1, EquippedMaxHP, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 25, 1, (CurHP > FullHP) ? FullHP : CurHP, 0));
                //sp
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 208, 1, EquippedMaxSP, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 26, 1, (CurSP > FullSP) ? FullSP : CurSP, 0));
                //str
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 210, 1, EquippedATK, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 41, 1, FullAtk, 0));
                if (levelup) tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 28, 1, Str, 0));
                //con
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 211, 1, EquippedDEF, 0));
                tmp.Add(SendPacket.FromFormat("bbbbdd", 8, 1, 42, 1, FullDef, 0));
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
                
                Send(new SendPacket(tmp.End()));
            }
        }

        void SendExp()
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 1 });
            p.Pack8(36);
            p.Pack8(1);
            p.Pack64((ulong)TotalExp);
            Send(p);
        }

        public void SetBeginnerOutfit()
        {
            switch (Body)
            {
                #region Big Female
                case BodyStyle.Big_Female:
                    {
                        HairStyle_BigF g = new HairStyle_BigF();
                        switch (Enum.GetName(g.GetType(), (HairStyle_BigF)Head))
                        {
                            case "Iris":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22005)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21006)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(23001)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24006)));
                                } break;
                            case "Lique":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21007)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(23002)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24007)));
                                } break;
                            case "Maria":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22006)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21011)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(10004)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24011)));
                                } break;
                            case "Vanessa":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21008)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24008)));
                                } break;
                            case "Breillat":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22007)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21009)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(10002)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24009)));
                                } break;
                            case "Karin":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21015)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22008)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24015)));
                                } break;
                            case "Konnotsuroko":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24013)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21013)));
                                } break;
                            case "Jessica":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22002)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21010)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(10003)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24010)));
                                } break;
                        }
                    } break;
                #endregion

                #region Big Male
                case BodyStyle.Big_Male:
                    {
                        HairStyle_BigM g = new HairStyle_BigM();
                        switch (Enum.GetName(g.GetType(), (HairStyle_BigM)Head))
                        {
                            case "Daniel":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21004)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24004)));
                                } break;
                            case "Sid":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21005)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24005)));
                                } break;
                            case "More":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21012)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24012)));
                                } break;
                            case "Kurogane":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22009)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21014)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(18002)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24014)));
                                } break;
                        }
                    } break;
                #endregion

                #region Sml Female
                case BodyStyle.Small_Female:
                    {
                        HairStyle_SmallF g = new HairStyle_SmallF();
                        switch (Enum.GetName(g.GetType(), (HairStyle_SmallF)Head))
                        {
                            case "Nina":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22003)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21002)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24002)));
                                } break;
                            case "Betty":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(22001)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21003)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24003)));
                                } break;
                        }
                    } break;
                #endregion

                #region Sml Male
                case BodyStyle.Small_Male:
                    {
                        HairStyle_SmallM g = new HairStyle_SmallM();
                        switch (Enum.GetName(g.GetType(), (HairStyle_SmallM)Head))
                        {
                            case "Rocco":
                                {
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(21001)));
                                    Wear(new Item((PhxItemInfo)_ItemManager.GetObject(24001)));
                                } break;
                        }

                    } break;
                #endregion
            }
        }

        #region Gold Methods
        public void SendGold()
        {
            Send(new SendPacket(SendPacket.FromFormat("bbd", 26, 4, Gold)));
        }
        public bool AddGold(int g)
        {
            lock (m_Lock)
            {
                if (gold >= 999999) return false;
                gold += g;
                return true;
            }
        }
        public bool TakeGold(int g)
        {
            lock (m_Lock)
            {
                if (gold < g) return false;
                Send(new SendPacket(SendPacket.FromFormat("bbd", 26, 2, g)));
                gold -= g;
                return true;
            }

        }
        public void SetGold(int g)
        {
            gold = g;
        }
        #endregion

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
        public bool IsEquipped(ushort src)
        {
            return (equippedItems.Count(c => c.ItemID == src) > 0);
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
