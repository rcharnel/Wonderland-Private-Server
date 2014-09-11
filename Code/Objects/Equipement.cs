using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    /// <summary>
    /// This will contain the Threadsafe stats for the character
    /// </summary>
    public class EquipementManager
    {
        readonly object m_Lock = new object();

        protected cSocket host;
        Dictionary<byte, EquipmentCell> clothes;

        byte head;
        int m_currexp, m_curhp, m_cursp, gold, m_skillpoint, m_potential;
        long m_totalexp = 0;
        BodyStyle body;
        ElementType element;
        RebornJob job;

        /// <summary>
        /// accesses the Equipment List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public EquipmentCell this[byte key]
        {
            get
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, return its value.
                    if (clothes.ContainsKey(key))
                    {
                        // The key was found; return its value. 
                        return clothes[key];
                    }
                    else
                    {
                        // The key was not found; return null. 
                        throw new Exception("No key in Dictionary");
                    }
                }
            }

            set
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, change its value. 
                    if (clothes.ContainsKey(key))
                    {
                        // The key was found; change its value.
                        clothes[key] = value;
                    }
                    else
                    {
                        throw new Exception("No key in Dictionary");
                    }
                }
            }
        }

        public EquipementManager()
        {
            clothes = new Dictionary<byte, EquipmentCell>();
            for (byte a = 1; a < 7; a++)
                clothes.Add(a, new EquipmentCell());
        }

        #region Definitions

        /// <summary>
        /// Gets/Sets the SkillPoints for the Character
        /// </summary>
        public UInt16 SkillPoints {
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
                    m_totalexp = value - (Level * 6);
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
            set
            {
                lock (m_Lock)
                {
                    gold = (int)value;
                }
            }
        }
        /// <summary>
        /// a Character's level
        /// </summary>
        public byte Level
        {
            get
            {
                lock (m_Lock)
                {
                    int level = 1;

                    long tmp = m_totalexp;
                    uint exp = (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], level);

                    while (tmp >= exp )
                    {
                        if (!Reborn && level + 1 > 199) break;
                        tmp -= exp;
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
                lock (m_Lock) { return (UInt32)(((Math.Round(Math.Pow((double)Level, 0.35) * Con) * 2) + (Level) + (Con * 2) + 180 + 0)); }
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
                { return (UInt16)(((Math.Round(Math.Pow((double)Level, 0.3) * Wis) * 3.2) + Level + (Wis * 2) + 94 + 0)); }
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
                        var remainexp = exptolvl-m_currexp;
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
        public ElementType Element
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
        UInt16 baseInt;
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
        UInt16 baseWis;
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
        UInt16 baseCon;
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
        UInt16 baseAgi;
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
                    case ElementType.Fire:
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
                    case ElementType.Fire:
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
                    case ElementType.Fire:
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
                    case ElementType.Earth:
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
                    case ElementType.Wind:
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
                lock(m_Lock)
                { 
                Int32 nRet = 0;
                for (byte i = 1; i < 7; ++i)
                    nRet += clothes[i].HP;
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
                        nRet += clothes[i].SP;
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
                        nRet += (Int16)clothes[i].ATK;
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
                        nRet += (Int16)clothes[i].DEF;
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
                        nRet += (Int16)clothes[i].MAT;
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
                        nRet += (Int16)clothes[i].MDF;
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
                        nRet += (Int16)clothes[i].SPD;
                    return (UInt16)nRet;
                }
            }

        }

        #endregion

        

        /// <summary>
        /// Returns a byte[] of Equips that are worn
        /// </summary>
        public byte[] Worn_Equips
        {
            get
            {
                lock (m_Lock)
                {
                    Packet tmp = new Packet();
                    if (WornCount > 0)
                    {

                        for (byte a = 1; a < 7; a++)
                            if (clothes[a].ItemID > 0)
                                tmp.Pack16(clothes[a].ItemID);
                    }
                    return tmp.Data.ToArray();
                }
            }
        }
        /// <summary>
        /// The Equipment list in Byte[] form (includes empty slots)
        /// </summary>
        public byte[] FullEqData
        {
            get
            {
                lock (m_Lock)
                {
                    Packet tmp = new Packet();
                    for (byte n = 1; n < 7; n++)
                        if (clothes[n].ItemID != 0)
                            tmp.Pack16(clothes[n].ItemID);
                        else
                            tmp.Pack16(0);

                    return tmp.Data.ToArray();
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
                    Packet tmp = new Packet();
                    if (WornCount > 0)
                    {
                        for (byte n = 1; n < 7; n++)
                        {
                            if (clothes[n].ItemID != 0)
                            {
                                tmp.Pack16(clothes[n].ItemID);
                                tmp.Pack8(clothes[n].Damage);
                                tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                            }
                        }
                    }
                    return tmp.Data.ToArray();
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
                return (byte)clothes.Values.Count(c => c.ItemID > 0);
            }
        }


        #region Saving/Loading

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

        public Dictionary<byte, uint[]> EqData
        {
            get
            {
                lock (m_Lock)
                {
                    Dictionary<byte, uint[]> tmp = new Dictionary<byte, uint[]>();
                    for (byte a = 1; a < 7; a++)
                        tmp.Add(a, new uint[] { clothes[a].ItemID, clothes[a].Damage, clothes[a].Ammt, (uint)clothes[a].Data.EquipPos, 0, 0, 0, 0 });
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

        #endregion


        #region Methods
        public void AddStat(byte statType, byte ammt)
        {
            if (SkillPoints == 0) return;
            SkillPoints -= ammt;

            switch (statType)
            {
                case 27://int
                    {
                        Int += ammt;
                        Send_1(27, Int, 0);
                        Send_1(43, (uint)FullMatk, 0);
                        Send_1(38, SkillPoints, 0);
                    } break;
                case 28://str
                    {
                        Str += ammt;
                        Send_1(28, Str, 0);
                        Send_1(41, (uint)FullAtk, 0);
                        Send_1(38, SkillPoints, 0);
                    } break;
                case 29://con
                    {
                        Con += ammt;
                        Send_1(29, Con, 0);
                        Send_1(42, (uint)FullDef, 0);
                        Send_1(205, (uint)FullHP, 0);
                        Send_1(38, SkillPoints, 0);
                    } break;
                case 30://agi
                    {
                        Agi += ammt;
                        Send_1(30, Agi, 0);
                        Send_1(45, (uint)FullSpd, 0);
                        Send_1(38, SkillPoints, 0);
                    } break;
                case 33://wis
                    {
                        Wis += ammt;
                        Send_1(33, Wis, 0);
                        Send_1(44, (uint)FullMdef, 0);
                        Send_1(206, MaxSp, 0);
                        Send_1(38, SkillPoints, 0);
                    } break;
            }
        }
        public void SendGold()
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 26, 4 });
            p.Pack32(Gold);
            host.Send(p);
        }
        
        /// <summary>
        /// Removes a Character's Equipment
        /// </summary>
        /// <param name="clothesSlot">place to remove Equip</param>
        /// <returns> Invenotry Item Type to place in Inventory</returns>
        public InvItemCell RemoveEQ(byte clothesSlot)
        {
            lock (m_Lock)
            {
                InvItemCell i = new InvItemCell();
                i.CopyFrom(clothes[clothesSlot]);
                clothes[clothesSlot].Clear();
                return i;
            }
        }
        /// <summary>
        /// Sets a Characters Equipment
        /// </summary>
        /// <param name="clothesSlot">Slot/place to put item</param>
        /// <param name="eq">the Item to Place </param>
        /// <returns>the InvItem object to be placed in Inv</returns>
        public InvItemCell SetEQ(byte clothesSlot, cItem eq)
        {
            lock (m_Lock)
            {
                InvItemCell i = null;

                if (clothes[clothesSlot].ItemID != 0)
                    i = RemoveEQ(clothesSlot);
                clothes[clothesSlot].CopyFrom(eq);
                return i;
            }
        }
        
        public void Send8_1(bool levelup = false) //this function sets the stas according to pts
        {
            lock (m_Lock)
            {
                if (levelup)
                {
                    SendExp();
                    Send_1(35, Level, 0);
                    Send_1(37, (uint)(Level - 1), 0);
                    Send_1(38, SkillPoints, 0);
                    CurHP = FullHP;
                    CurSP = FullSP;
                }

                Send_1(207, (uint)EquippedMaxHP, 0); //a plus to hp
                Send_1(25, (uint)((CurHP > FullHP) ? FullHP : CurHP), 0);
                Send_1(208, (uint)EquippedMaxSP, 0); //a plus to sp
                Send_1(26, (uint)((CurSP > FullSP) ? FullSP : CurSP), 0);
                Send_1(210, (uint)EquippedATK, 0);
                Send_1(41, (uint)FullAtk, 0);
                Send_1(211, (uint)EquippedDEF, 0);
                Send_1(42, (uint)FullDef, 0);
                Send_1(214, (uint)EquippedSPD, 0);
                Send_1(45, (uint)FullSpd, 0);
                Send_1(215, (uint)EquippedMAT, 0);
                Send_1(43, (uint)FullMatk, 0);
                Send_1(216, (uint)EquippedMDF, 0);
                Send_1(44, (uint)FullMdef, 0);
            }
        }
        void SendExp()
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 1 });
            p.Pack8(36);
            p.Pack8(1);
            p.Pack64((ulong)TotalExp);
            host.Send(p);
        }
        void Send_1(byte stat, UInt32 ammt, UInt32 skill)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 1 });
            p.Pack8(stat);
            p.Pack8(1);
            p.Pack32(ammt);
            p.Pack32(skill);
            host.Send(p);
        }
        void Send_16(byte src, byte dst) //deequipping
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 16 });
            p.Pack8(src);
            p.Pack8(dst);
            host.Send(p);
        }
        void Send_17(byte src, byte dst) //equipping
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 17 });
            p.Pack8(src);
            p.Pack8(dst);
            host.Send(p);
        }
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
        public void RemfromStat(byte statType, ushort ammt)
        {
        }
        public void FillHP()
        {
            CurHP = (int)MaxHp;
        }
        public void FillSP()
        {
            CurSP = MaxSp;
        }
        #endregion
    }

    public class PetEquipementManager
    {
        readonly object m_Lock = new object();
        protected Npc myData;
        protected Player owner; 
        Dictionary<byte, EquipmentCell> clothes;

        byte head;
        protected byte slot;
        int m_currexp, m_curhp, m_cursp, gold, m_skillpoint, m_potential;
        long m_totalexp = 6;
        BodyStyle body;
        ElementType element;
        RebornJob job;

        /// <summary>
        /// accesses the Equipment List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public EquipmentCell this[byte key]
        {
            get
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, return its value.
                    if (clothes.ContainsKey(key))
                    {
                        // The key was found; return its value. 
                        return clothes[key];
                    }
                    else
                    {
                        // The key was not found; return null. 
                        throw new Exception("No key in Dictionary");
                    }
                }
            }

            set
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, change its value. 
                    if (clothes.ContainsKey(key))
                    {
                        // The key was found; change its value.
                        clothes[key] = value;
                    }
                    else
                    {
                        throw new Exception("No key in Dictionary");
                    }
                }
            }
        }

        public PetEquipementManager(int slot = 0,Player own = null)
        {
            this.slot = (byte)slot;
            owner = own;
            clothes = new Dictionary<byte, EquipmentCell>();
            for (byte a = 1; a < 7; a++)
                clothes.Add(a, new EquipmentCell());
        }

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
                    return m_totalexp;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_totalexp = value;

                    long tmp = m_totalexp;
                    int level = 1;
                    uint exp = (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], level);

                    while (tmp > exp)
                    {
                        tmp -= exp;
                        exp = (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], ++level);
                    }
                    m_currexp = (int)tmp;
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
        /// a Character's level
        /// </summary>
        public byte Level
        {
            get
            {
                lock (m_Lock)
                {
                    int level = 1;

                    long tmp = m_totalexp;
                    uint exp = (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], level);

                    while (tmp > exp)
                    {
                        if (!Reborn && ++level > 199) break;
                        tmp -= exp;
                        exp = (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], ++level);
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
        /// Exp needed to Level up
        /// </summary>
        public UInt32 ExptoLevel
        {
            get
            {
                lock (m_Lock)
                {
                    return (uint)CalcMaxExp(BitConverter.GetBytes(Reborn)[0], Level);
                }
            }
        }
        /// <summary>
        /// A Character's Current EXP
        /// </summary>
        public virtual Int32 CurExp
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
                    TotalExp += (UInt32)(value - CurExp);
                    CurExp = value;

                    if (CurExp >= ExptoLevel)
                    {
                        m_currexp -= (int)ExptoLevel;
                        SkillPoints += 5;
                        //Send8_1(true);
                    }
                    //else
                    //    SendExp();
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
        /// Element Type
        /// </summary>
        public ElementType Element
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

        //Base Calculated Stats of the character set by 9,1
        #region Base Stats
        UInt16 baseStr;
        public UInt16 Str
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    return (ushort)(baseStr + ((myData != null) ? myData.STR : 0) + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                { baseStr = value; }
            }
        }
        UInt16 baseInt;
        public UInt16 Int
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;

                    return (ushort)(baseInt + ((myData != null) ? myData.INT : 0) + (Level * add));
                }

            }
            set
            {
                lock (m_Lock)
                { baseInt = value; }
            }
        }
        UInt16 baseWis;
        public UInt16 Wis
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    return (ushort)(baseWis + ((myData != null) ? myData.WIS : 0) + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                { baseWis = value; }
            }
        }
        UInt16 baseCon;
        public UInt16 Con
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    return (ushort)(baseCon + ((myData != null) ? myData.CON : 0) + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                { baseCon = value; }
            }
        }
        UInt16 baseAgi;
        public UInt16 Agi
        {
            get
            {
                lock (m_Lock)
                {
                    int add = 0;
                    return (ushort)(baseAgi + ((myData != null) ? myData.AGI : 0) + (Level * add));
                }
            }
            set
            {
                lock (m_Lock)
                { baseAgi = value; }
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
                    case ElementType.Fire:
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
                    case ElementType.Fire:
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
            set { baseInt = value; }
        }
        UInt16 Mdef
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case ElementType.Fire:
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
            set { baseWis = value; }
        }
        UInt16 Def
        {
            get
            {
                int add = 0;
                switch (Element)
                {
                    case ElementType.Earth:
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
                    case ElementType.Wind:
                        add = (UInt16)(Math.Round(((double)Level * 2.1) + ((double)Agi * 2.2)));
                        break;
                    default:
                        add = (UInt16)(Math.Round(((double)Level * 1.6) + ((double)Agi * 2.2)));
                        break;
                }

                if (Job == RebornJob.Knight || Job == RebornJob.Seer)
                    add = (UInt16)Math.Round((double)add * 1.1);

                return (ushort)(add + ((myData != null) ? myData.SPD : 0));
            }
        }
        #endregion

        #region Full Stats
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

        /// <summary>
        /// Base Calculated Max HP
        /// </summary>
        UInt32 MaxHp
        {
            get
            {
                lock (m_Lock) { return (UInt32)(((Math.Round(Math.Pow((double)Level, 0.35) * Con) * 2) + (Level) + (Con * 2) + 180 + 0)); }
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
                { return (UInt16)(((Math.Round(Math.Pow((double)Level, 0.3) * Wis) * 3.2) + Level + (Wis * 2) + 94 + 0)); }
            }
        }


        #region Equipped Bonuses
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
                        nRet += clothes[i].HP;
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
                        nRet += clothes[i].SP;
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
                        nRet += (Int16)clothes[i].ATK;
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
                        nRet += (Int16)clothes[i].DEF;
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
                        nRet += (Int16)clothes[i].MAT;
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
                        nRet += (Int16)clothes[i].MDF;
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
                        nRet += (Int16)clothes[i].SPD;
                    return (UInt16)nRet;
                }
            }

        }

        #endregion

        /// <summary>
        /// Returns a byte[] of Equips that are worn
        /// </summary>
        public byte[] Worn_Equips
        {
            get
            {
                lock (m_Lock)
                {
                    Packet tmp = new Packet();
                    if (WornCount > 0)
                    {

                        for (byte a = 1; a < 7; a++)
                            if (clothes[a].ItemID > 0)
                                tmp.Pack16(clothes[a].ItemID);
                    }
                    return tmp.Data.ToArray();
                }
            }
        }
        /// <summary>
        /// The Equipment list in Byte[] form (includes empty slots)
        /// </summary>
        public byte[] FullEqData
        {
            get
            {
                lock (m_Lock)
                {
                    Packet tmp = new Packet();
                    for (byte n = 1; n < 7; n++)
                        if (clothes[n].ItemID != 0)
                        {
                            tmp.Pack16(clothes[n].ItemID);
                            tmp.Pack8(0);//dmg
                            tmp.Pack16(0);//unknown
                            tmp.Pack16(0);//socketitem
                            tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                        }
                        else
                            tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                    return tmp.Data.ToArray();
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
                    Packet tmp = new Packet();
                    if (WornCount > 0)
                    {
                        for (byte n = 1; n < 7; n++)
                        {
                            if (clothes[n].ItemID != 0)
                            {
                                tmp.Pack16(clothes[n].ItemID);
                                tmp.Pack8(clothes[n].Damage);
                                tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                            }
                        }
                    }
                    return tmp.Data.ToArray();
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
                return (byte)clothes.Values.Count(c => c.ItemID > 0);
            }
        }


        #region Saving/Loading

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

        public Dictionary<byte, uint[]> EqData
        {
            get
            {
                lock (m_Lock)
                {
                    Dictionary<byte, uint[]> tmp = new Dictionary<byte, uint[]>();
                    for (byte a = 1; a < 7; a++)
                        tmp.Add(a, new uint[] { clothes[a].ItemID, clothes[a].Damage, clothes[a].Ammt, (uint)clothes[a].Data.EquipPos, 0, 0, 0, 0 });
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

        #endregion


        //void AddtoStat(byte statType, byte ammt)
        //{
        //    if (SkillPoints == 0) return;
        //    SkillPoints -= ammt;

        //    switch (statType)
        //    {
        //        case 27://int
        //            {
        //                Int += ammt;
        //                Send_1(27, Int, 0);
        //                Send_1(43, Matk, 0);
        //                Send_1(38, SkillPoints, 0);
        //            } break;
        //        case 28://str
        //            {
        //                Str += ammt;
        //                Send_1(28, Str, 0);
        //                Send_1(41, Atk, 0);
        //                Send_1(38, SkillPoints, 0);
        //            } break;
        //        case 29://con
        //            {
        //                Con += ammt;
        //                Send_1(29, Con, 0);
        //                Send_1(42, Def, 0);
        //                Send_1(205, (uint)FullHP, 0);
        //                Send_1(38, SkillPoints, 0);
        //            } break;
        //        case 30://agi
        //            {
        //                Agi += ammt;
        //                Send_1(30, Agi, 0);
        //                Send_1(45, Spd, 0);
        //                Send_1(38, SkillPoints, 0);
        //            } break;
        //        case 33://wis
        //            {
        //                Wis += ammt;
        //                Send_1(33, Wis, 0);
        //                Send_1(44, Mdef, 0);
        //                Send_1(206, MaxSp, 0);
        //                Send_1(38, SkillPoints, 0);
        //            } break;
        //    }
        //}

        //void RemfromStat(byte statType, byte ammt)
        //{
        //}

        /// <summary>
        /// Removes a Character's Equipment
        /// </summary>
        /// <param name="clothesSlot">place to remove Equip</param>
        /// <returns> Invenotry Item Type to place in Inventory</returns>
        public InvItemCell RemoveEQ(byte clothesSlot)
        {
            lock (m_Lock)
            {
                InvItemCell i = new InvItemCell();
                i.CopyFrom(clothes[clothesSlot]);
                clothes[clothesSlot].Clear();
                return i;
            }
        }
        /// <summary>
        /// Sets a Characters Equipment
        /// </summary>
        /// <param name="clothesSlot">Slot/place to put item</param>
        /// <param name="eq">the Item to Place </param>
        /// <returns>the InvItem object to be placed in Inv</returns>
        public InvItemCell SetEQ(byte clothesSlot, cItem eq)
        {
            lock (m_Lock)
            {
                InvItemCell i = null;

                if (clothes[clothesSlot].ItemID != 0)
                    i = RemoveEQ(clothesSlot);
                clothes[clothesSlot].CopyFrom(eq);
                return i;
            }
        }

        public void Send8_2(bool levelup = false) //this function sets the stas according to pts
        {
            lock (m_Lock)
            {
                if (levelup)
                {
                    SendExp();
                    Send_2(35, Level, 0);
                    Send_2(37, (uint)(Level - 1), 0);
                    Send_2(38, SkillPoints, 0);
                    CurHP = FullHP;
                    CurSP = FullSP;
                }

                Send_2(207, (uint)EquippedMaxHP, 0); //a plus to hp
                Send_2(25, (uint)((CurHP > FullHP) ? FullHP : CurHP), 0);
                Send_2(208, (uint)EquippedMaxSP, 0); //a plus to sp
                Send_2(26, (uint)((CurSP > FullSP) ? FullSP : CurSP), 0);
                Send_2(210, (uint)EquippedATK, 0);
                Send_2(41, (uint)FullAtk, 0);
                Send_2(211, (uint)EquippedDEF, 0);
                Send_2(42, (uint)FullDef, 0);
                Send_2(214, (uint)EquippedSPD, 0);
                Send_2(45, (uint)FullSpd, 0);
                Send_2(215, (uint)EquippedMAT, 0);
                Send_2(43, (uint)FullMatk, 0);
                Send_2(216, (uint)EquippedMDF, 0);
                Send_2(44, (uint)FullMdef, 0);
            }
        }
        void SendExp()
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 2, 4 });
            p.Pack16(slot);
            p.Pack8(36);
            p.Pack8(1);
            p.Pack64((ulong)TotalExp);
            owner.Send(p);
        }
        void Send_2(byte stat, UInt32 ammt, UInt32 skill)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 8, 2, 4 });
            p.Pack16(slot);
            p.Pack8(stat);
            p.Pack8(2);
            p.Pack32(ammt);
            p.Pack32(skill);
            owner.Send(p);
        }
        void Send_16(byte src, byte dst) //deequipping
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 16 });
            p.Pack8(src);
            p.Pack8(dst);
            //socket.Send(p);
        }
        void Send_17(byte src, byte dst) //equipping
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 17 });
            p.Pack8(src);
            p.Pack8(dst);
            //socket.Send(p);
        }
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

        }
        public void FillSP()
        {

        }

    }
}
