using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    public class SkillAttackPattern
    {
        AttackPattern APattern;
        int GradeStartRange;
        int GradeEndRange;


        public bool isInRange(byte currentrange)
        {
            if (currentrange >= GradeStartRange && currentrange <= GradeEndRange)
                return true;
            else
                return false;
        }
        public void Set(byte ptr, byte data, byte Max)
        {
            APattern = (AttackPattern)data;
            int w = 0;
            int bon = 0;
            int m = 0;
            switch (ptr)
            {
                case 1: { w = 0; bon = 1; m = 25; } break;
                case 2: { w = 25; bon = 1; m = 50; } break;
                case 3: { bon = 1; w = 50; m = 75; } break;
                case 4: { w = 75; m = 100; } break;
            }

            GradeStartRange = (int)Math.Floor((double)((w / 100.0) * Max) + bon);
            if (Max == 0) GradeEndRange = 1;
            else
                GradeEndRange = (int)Math.Floor((double)((m / 100.0) * Max));
        }

        public List<byte[]> GetTargets(byte[] tgrid, BattleSide loc)
        {

            List<byte[]> trglist = new List<byte[]>();
            switch (loc)
            {
                case BattleSide.Defending:
                    {
                        switch (APattern)
                        {
                            case AttackPattern.Single: trglist.Add(tgrid); break;
                            case AttackPattern.HorizontalLine:
                                {
                                    trglist.Add(new byte[] { tgrid[0], (byte)(tgrid[1] - 1) });
                                    trglist.Add(new byte[] { tgrid[0], (byte)(tgrid[1] + 1) });
                                } break;
                            case AttackPattern.VerticalLine:
                                {
                                    trglist.Add(new byte[] { (byte)(tgrid[0] - 1), (byte)(tgrid[1]) });
                                    trglist.Add(new byte[] { (byte)(tgrid[0] + 1), (byte)(tgrid[1]) });
                                } break;
                        } break;
                    }
                case BattleSide.Attacking:
                    {
                        switch (APattern)
                        {
                            case AttackPattern.Single: trglist.Add(tgrid); break;
                            case AttackPattern.HorizontalLine:
                                {
                                    trglist.Add(new byte[] { tgrid[0], (byte)(tgrid[1] - 1) });
                                    trglist.Add(new byte[] { tgrid[0], (byte)(tgrid[1] + 1) });
                                } break;
                            case AttackPattern.VerticalLine:
                                {
                                    trglist.Add(new byte[] { (byte)(tgrid[0] - 1), (byte)(tgrid[1]) });
                                    trglist.Add(new byte[] { (byte)(tgrid[0] + 1), (byte)(tgrid[1]) });
                                } break;
                        } break;
                    }
            }
            return trglist;
        }
    }

    public class Skill
    {
        SkillInfo Data;
        public byte jobReq;
        public byte levelReq;
        public ushort StrReq;
        public ushort ConReq;
        public ushort WisReq;
        public ushort IntReq;
        public ushort AgiReq;
        public bool npconly;
        public bool givenatquest;
        public byte Grade;
        public uint Exp;

        int tyrns_in_effect;

        public void DecreaseTurns()
        {
        }

        public SkillType Type { get { if (Data != null)return (SkillType)Data.Type; else return SkillType.None; } }
        public string Name { get { return ASCIIEncoding.ASCII.GetString(GetData().SkillName).Replace("'", string.Empty).Replace("\0", string.Empty); } }
        public double AnimtionTime { get { return Data.Decimal2; } }
        public ushort AttackPower() { if (Data.MaxSkillLevel == 0)Data.MaxSkillLevel = 1; return (ushort)Math.Floor((double)(((Grade * 100.0) / (double)Data.MaxSkillLevel) / 100.0) * Data.AdditinalHarm); }
        public ElementType ElementType { get { return (ElementType)Data.ElementType; } set { Data.ElementType = (byte)value; } }
        public ushort SkillID { get { if (Data != null) return Data.SkillID; else return 0; } }
        public ushort SPUsage { get { return Data.SP; } }
        public ushort PrecedingSkill { get { return Data.SkillID2; } }
        public ushort SkillTableOrder { get { return Data.SkillTableOrder; } }

        internal SkillInfo GetData()
        {
            return Data;
        }
        public EffectLayer EffectLayer { get { return (EffectLayer)Data.EffectLayer; } }
        public EffectType TargetEffect { get { return (EffectType)Data.Effect; } }
        SkillAttackPattern[] GetPatternList()
        {
            List<SkillAttackPattern> tmp = new List<SkillAttackPattern>();
            SkillAttackPattern o = new SkillAttackPattern();
            o.Set(1, Data.SkillPattern1, Data.MaxSkillLevel);
            tmp.Add(o);
            o = new SkillAttackPattern();
            o.Set(2, Data.SkillPattern2, Data.MaxSkillLevel);
            tmp.Add(o);
            o = new SkillAttackPattern();
            o.Set(3, Data.SkillPattern3, Data.MaxSkillLevel);
            tmp.Add(o);
            o = new SkillAttackPattern();
            o.Set(4, Data.SkillPattern4, Data.MaxSkillLevel);
            tmp.Add(o);
            return tmp.ToArray();
        }
        public SkillAttackPattern PatternofAttack()
        {
            foreach (SkillAttackPattern o in GetPatternList())
            {
                if (o.isInRange(Grade))
                    return o;
            }
            return null;
        }

        public Skill()
        {
            levelReq = 0;
            StrReq = 0;
            ConReq = 0;
            WisReq = 0;
            IntReq = 0;
            AgiReq = 0;
            jobReq = 0;
            Grade = 1;
        }
        public Skill(SkillInfo src)
        {
            Data = src;
            Grade = 1;
            Exp = 1;
            /* levelReq = byte.Parse((string)row["levelreq"]);
             jobReq = byte.Parse((string)row["jobreq"]);
             StrReq = byte.Parse((string)row["strreq"]);
             IntReq = byte.Parse((string)row["intreq"]);
             ConReq = byte.Parse((string)row["conreq"]);
             AgiReq = byte.Parse((string)row["agireq"]);
             WisReq = byte.Parse((string)row["wisreq"]);
             ElementType = (ElementType)byte.Parse((string)row["elementreq"]);  */
        }
        public Skill(Skill src)
        {
            levelReq = src.levelReq;
            StrReq = src.StrReq;
            ConReq = src.ConReq;
            WisReq = src.WisReq;
            IntReq = src.IntReq;
            AgiReq = src.AgiReq;
            jobReq = src.jobReq;
            Data = src.Data;
            Grade = src.Grade;
            Exp = src.Exp;
        }
        //public bool hasRequirements(CharacterTemplate src)
        //{
        //    if (src.MyStats.Str >= StrReq && src.MyStats.Con >= ConReq && src.MyStats.Wis >= WisReq &&
        //        src.MyStats.Int >= IntReq && src.MyStats.Agi >= AgiReq && src.MyStats.Level >= levelReq
        //        && src.MyStats.element == ElementType && !givenatquest && !npconly)
        //    {
        //        if (Data.SkillID2 != 0)
        //        {
        //            var preskil = src.MySkills.GetSkillByID(Data.SkillID2);
        //            if (preskil != null)
        //            {
        //                if (preskil.Grade == preskil.Data.MaxSkillLevel)
        //                {
        //                    return true;
        //                }
        //                else
        //                    return false;
        //            }
        //            else
        //                return false;
        //        }
        //        else
        //            return true;
        //    }
        //    else
        //        return false;
        //}
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class SkillInfo
    {
        public byte SkillNameLength;     //Length of the Skill's Name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] SkillName;         //Skill's Name
        public byte Type;                //0==Battle Type, 1==Earth Type, 2==Water Type, 3==Fire Type, 4==Wind Type, 7==Char Special Skill, 8==Item, 9==Life Skill, 10==Pet Skill
        public UInt16 SkillID;           //Skill's ID
        public UInt16 SP;  //Amount of SP needed to use skill
        public byte ElementType;             //0==Normal, 1==Earth, 2==Water, 3==Fire, 4==Wind, 7==Undefined
        public UInt16 Attack;            //0==Near, 1==Distance1, 2==Distance2, 4==Outside of Battle
        public byte EffectLayer;         //0==Pet Express, 1==Physical Skill, 2==Magical Skill, 3==Seals1, 4==Buffs1, 5==Debuffs1, 6==Mana, 7==Healing1, 8==Revival, 10==Defend, 11==Capture, 12==Flee, 13==No Effects, 14==Healing2, 15==Seals2, 16==Debuffs2, 17==Static Damage (not used in Damage Formula), 19==Buffs2, 20==Equiped Item, 21==Rest, 22==Prolonged Item
        public byte UnknownByte1;        //
        public byte UnknownByte2;        //
        public double Decimal1;          //
        public double Decimal2;          //
        public byte UnknownByte3;        //
        public UInt16 AdditinalHarm;     //The skill's Attack Power
        public byte NumberOfTurns;       //Number of additional turns a skill will be in effect (Old Definition=UnknownByte4)
        public byte Effect;        //
        public byte UnknownByte6;        //
        public UInt16 SkillID2;          //The preceding skill that is needed to be maxed in order to unlock that skill
        public UInt16 UnknownWord1;      //No Known Value
        public UInt16 UnknownWord2;      //No Known Value
        public UInt16 UnknownWord3;      //No Known Value
        public UInt16 UnknownWord4;      //No Known Value
        public UInt16 UnknownWord5;      //No Known Value
        public byte DescriptionLength;   //Length of the skill's Description
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public byte[] Description;       //Description of the skill's attack
        public UInt16 SkillTableOrder;   //Order of placement within the skill table
        public byte Target;              //0==Any Enemy, 1==Any Ally, 3==Self, 4==Pet, 5==Any Ally But Self
        public UInt16 ImageNumSmall;     //The Image Number
        public UInt16 UnknownWord6;      //No Known Value
        public byte UnknownByte7;        //
        public byte MaxSkillLevel;       //Max Grade of skill
        public byte AdditionalEffect;    //
        public UInt16 UnknownWord7; //No Known Value
        public byte SkillPattern1;       //Skill Pattern1
        public byte SkillPattern2;       //Skill Pattern2
        public byte SkillPattern3;       //Skill Pattern3
        public byte SkillPattern4;       //Skill Pattern4
        public byte TypeOfInjury;        //0==Normal, 1==Arrow, 2==Gun
        public byte UnknownByte8;        //No Known Value
        public byte UnknownByte9;        //No Known Value
        public byte UnknownByte10;       //No Known Value
        public byte UnknownByte11;       //No Known Value
        public UInt16 VoiceWav;          //The VoiceWav Number
        public UInt16 UnknownWord8;      //No Known Value
        public UInt16 UnknownWord9;      //No Known Value
        public UInt16 UnknownWord10;     //No Known Value
        public UInt16 UnknownWord11;     //No Known Value
        public UInt32 UnknownDword1;     //No Known Value
        public UInt32 UnknownDword2;     //No Known Value
        public UInt32 UnknownDword3;     //No Known Value
        public UInt32 UnknownDword4;     //No Known Value
        public UInt32 UnknownDword5;     //No Known Value
    }

    public class SkillDataFile
    {

        private List<Skill> m_List;
        public bool Loaded = false;
        private readonly object m_Lock;

        public SkillDataFile()
        {
            m_List = new List<Skill>();
            m_Lock = new object();
        }

        protected void DecodeSkill32(ref UInt32 val)
        {
            val = Convert.ToUInt32((val ^ 0x0BDEDEBF) - 4);
        }

        protected void DecodeSkill16(ref UInt16 val)
        {
            val = Convert.ToUInt16((val ^ 0x6EA0) - 4);
        }

        protected void DecodeSkill8(ref byte val)
        {
            val = Convert.ToByte(((val) ^ 0xFD) - 4);
        }

        protected T ReadFromSkills<T>(Stream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }

        public bool LoadSkills(string file)
        {
            DebugSystem.Write("Loading Skill.Dat");
                using (Stream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        long FileLen = fs.Length;
                        long total = FileLen;
                        while (FileLen >= Marshal.SizeOf(typeof(SkillInfo)))
                        {
                            SkillInfo NewSkill = ReadFromSkills<SkillInfo>(fs);

                            DecodeSkill8(ref NewSkill.Type);
                            DecodeSkill16(ref NewSkill.SkillID);
                            DecodeSkill16(ref NewSkill.SP);
                            DecodeSkill8(ref NewSkill.ElementType);
                            DecodeSkill16(ref NewSkill.Attack);
                            DecodeSkill8(ref NewSkill.EffectLayer);
                            DecodeSkill8(ref NewSkill.UnknownByte1);
                            DecodeSkill8(ref NewSkill.UnknownByte2);
                            DecodeSkill8(ref NewSkill.UnknownByte3);
                            DecodeSkill16(ref NewSkill.AdditinalHarm);
                            DecodeSkill8(ref NewSkill.NumberOfTurns);
                            DecodeSkill8(ref NewSkill.Effect);
                            DecodeSkill8(ref NewSkill.UnknownByte6);
                            DecodeSkill16(ref NewSkill.SkillID2);
                            DecodeSkill16(ref NewSkill.UnknownWord1);
                            DecodeSkill16(ref NewSkill.UnknownWord2);
                            DecodeSkill16(ref NewSkill.UnknownWord3);
                            DecodeSkill16(ref NewSkill.UnknownWord4);
                            DecodeSkill16(ref NewSkill.UnknownWord5);
                            DecodeSkill16(ref NewSkill.SkillTableOrder);
                            DecodeSkill8(ref NewSkill.Target);
                            DecodeSkill16(ref NewSkill.ImageNumSmall);
                            DecodeSkill16(ref NewSkill.UnknownWord6);
                            DecodeSkill8(ref NewSkill.UnknownByte7);
                            DecodeSkill8(ref NewSkill.MaxSkillLevel);
                            DecodeSkill8(ref NewSkill.AdditionalEffect);
                            DecodeSkill16(ref NewSkill.UnknownWord7);
                            DecodeSkill8(ref NewSkill.SkillPattern1);
                            DecodeSkill8(ref NewSkill.SkillPattern2);
                            DecodeSkill8(ref NewSkill.SkillPattern3);
                            DecodeSkill8(ref NewSkill.SkillPattern4);
                            DecodeSkill8(ref NewSkill.TypeOfInjury);
                            DecodeSkill8(ref NewSkill.UnknownByte8);
                            DecodeSkill8(ref NewSkill.UnknownByte9);
                            DecodeSkill8(ref NewSkill.UnknownByte10);
                            DecodeSkill8(ref NewSkill.UnknownByte11);
                            DecodeSkill16(ref NewSkill.VoiceWav);
                            DecodeSkill16(ref NewSkill.UnknownWord8);
                            DecodeSkill16(ref NewSkill.UnknownWord9);
                            DecodeSkill16(ref NewSkill.UnknownWord10);
                            DecodeSkill16(ref NewSkill.UnknownWord11);
                            DecodeSkill32(ref NewSkill.UnknownDword1);
                            DecodeSkill32(ref NewSkill.UnknownDword2);
                            DecodeSkill32(ref NewSkill.UnknownDword3);
                            DecodeSkill32(ref NewSkill.UnknownDword4);
                            DecodeSkill32(ref NewSkill.UnknownDword5);

                            for (int i = 0; i < 10; i++)
                            {
                                byte tmp = NewSkill.SkillName[19 - i];
                                NewSkill.SkillName[19 - i] = NewSkill.SkillName[i];
                                NewSkill.SkillName[i] = tmp;
                            }
                            for (int i = 0; i < 15; i++)
                            {
                                byte tmp = NewSkill.Description[29 - i];
                                NewSkill.Description[29 - i] = NewSkill.Description[i];
                                NewSkill.Description[i] = tmp;
                            }
                            FileLen -= Marshal.SizeOf(typeof(SkillInfo));
                            Skill tmpskill = new Skill(NewSkill);
                            m_List.Add(tmpskill);
                        }
                        fs.Close();
                        DebugSystem.Write("Skill.Dat Loaded ( " + m_List.Count + " Skill)");
                    }
                    catch (Exception ex) { DebugSystem.Write(ex); }
                }
                return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byType"></param>
        /// <returns></returns>
        public IEnumerable<Skill> Get_Acquirable_Skills(ElementType byType)
        {
            //switch (byType)
            //{
            //    case ElementType.Earth: return (from skill in m_List where skill.Type == SkillType.EarthType select skill).DefaultIfEmpty(null);
            //    case ElementType.Fire: return (from skill in m_List where skill.Type == SkillType.FireType select skill).DefaultIfEmpty(null);
            //    case ElementType.Water: return (from skill in m_List where skill.Type == SkillType.WaterType select skill).DefaultIfEmpty(null);
            //    case ElementType.Wind: return (from skill in m_List where skill.Type == SkillType.WindType select skill).DefaultIfEmpty(null);
            //}
            return null;
        }

        public Skill Get_Skill(UInt16 SkillID)
        {
            foreach (var t in m_List)
            {
                if(t.SkillID == SkillID)
                {
                    return t;
                }
            }
            return null;
        }
        public Skill Get_Skill(string SkillName)
        {
            foreach (var t in m_List)
            {
                if (t.Name == SkillName)
                {
                    return t;
                }
            }
            return null;
        }

        public void UnloadSkills() { lock (m_Lock) { m_List.Clear(); } }
    }
}
