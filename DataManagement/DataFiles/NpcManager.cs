using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    public class Npc
    {

        byte NpcNameLength;
        public string NpcName;
        public byte Type; // [代碼不太明白] (val ^ 0xC8) - 1
        public UInt16 NpcID; // (val ^ 0x5209) - 1
        UInt16 ImageNum; // jma\001.jma  (val ^ 5209) - 1
        UInt16 ImageNumSmall;//(Small); // jma\007.jma (val ^ 0x5209) - 1
        UInt32 ColorCode1; // (val ^ 0x0BAEB716) - 1
        UInt32 ColorCode2; // (val ^ 0x0BAEB716) - 1
        UInt32 ColorCode3; // (val ^ 0x0BAEB716) - 1
        UInt32 ColorCode4; // (val ^ 0x0BAEB716) - 1
        public bool Can_Catch { get { if (Catchable == 1)return true; else return false; } }
        public byte Catchable; // 01=yes 02=no (val ^ 0xC8) - 1
        public byte UnknownByte2; // 未知參數[] (val ^ 0xC8) - 1
        public byte UnknownByte3; // 未知參數[] (val ^ 0xC8) - 1
        public byte Level; //  (val ^ 0xC8) - 1
        public UInt32 HP; //  (val ^ 0xBAEB716) - 1
        public UInt32 SP; //  (val ^ 0xBAEB716) - 1
        public UInt16 STR; //  (val ^ 0x5209) - 1
        public UInt16 CON; //  (val ^ 0x5209) - 1
        public UInt16 INT; //  (val ^ 0x5209) - 1
        public UInt16 WIS; //  (val ^ 0x5209) - 1
        public UInt16 AGI; //  (val ^ 0x5209) - 1
        byte ImageNumEnlarge;//(Enlarge); //00=正常 01=放大 (val ^ 0xC8) - 1
        public ElementType element; // 00=無 01=地 02=水 03=火 04=風 (val ^ 0xC8) - 1
        public Skill SkillID1; // Pet stunt1 (val ^ 0x5209) - 1
        public Skill SkillID2; // Pet stunt2 (val ^ 0x5209) - 1
        public Skill SkillID3; // Pet stunt3 (val ^ 0x5209) - 1
        public UInt16 ItemID1; // Drop Items 1 (val ^ 0x5209) - 1
        public UInt16 ItemID2; // Drop Items 2 (val ^ 0x5209) - 1
        public UInt16 ItemID3; // Drop Items 3 (val ^ 0x5209) - 1
        public UInt16 ItemID4; // Drop Items 4 (val ^ 0x5209) - 1
        public UInt16 ItemID5; // Drop Items 5 (val ^ 0x5209) - 1
        public byte UnknownByte5; // 未知參數[] 00,01,02 (val ^ 0xC8) - 1
        public UInt16 UnknownWord14; //未知參數[] 00,01 (val ^ 0x5209) - 1
        public UInt16 UnknownWord15; //未知參數[] 只有NpcID 18045 = 06  (val ^ 0x5209) - 1
        public UInt16 UnknownWord16; //補00 00？ (val ^ 0x5209) - 1
        public UInt16 UnknownWord17; //補00 00？ (val ^ 0x5209) - 1
        public byte GeneralAttack1; // 01=獸寵  00,02=10001(檢查武器類型)  03≧10001(不檢查武器類型) (val ^ 0xC8) - 1
        public UInt16 UnknownWord18; // 未知參數[] (val ^ 0x5209) - 1
        public byte GeneralAttack2; // GeneralAttack1=01 執行這裡上面遮蔽 00=10023  01,02=10010  03≧10001(不檢查武器類型) (SkillID) (val ^ 0xC8) - 1
        public byte UnknownByte8; // 未知參數[] 00,01  (val ^ 0xC8) - 1
        UInt16 TalkImage; // jma\008.jma (val ^ 0x5209) - 1
        public UInt16 UnknownWord20; // 未知參數[] (val ^ 0x5209) - 1
        public UInt16 UnknownWord21; // 未知參數[] (val ^ 0x5209) - 1
        public UInt16 SPD; // 魔物spd (參數＞0 不符合spd公式 　　Ex： Mahabrahman spd=4000 ) (val ^ 0x5209) - 1
        public UInt16 GeneralAttack3; // 參數＞0 普通攻擊以這裡SkillID為主要 (val ^ 0x5209) - 1
        public byte UnknownByte9; // 未知參數[] 00,01 (val ^ 0xC8) - 1
        public bool Can_Transfer { get { if (Transferrable == 1)return true; else return false; } }
        byte Transferrable; // 參數＞0 不得轉讓 (val ^ 0xC8) - 1
        public bool Can_PK { get { if (PK_NPC == 1)return true; else return false; } }
        byte PK_NPC; // 00=YES 01=NO (val ^ 0xC8) - 1
        public UInt16 UnknownWord24; // ??? (val ^ 0x5209) - 1
        public byte UnknownByte12; // 未知參數[] (val ^ 0xC8) - 1
        public byte NPCQuestID; //  (val ^ 0xC8) - 1
        public bool HumanNpc { get { if (HumanNPC == 1)return true; else return false; } }
        byte HumanNPC; // 00=NO 01=YES (val ^ 0xC8) - 1
        public byte UnknownByte15; // ??? (val ^ 0xC8) - 1
        public bool HPDouble { get { if (HP_times2 == 1)return true; else return false; } }
        byte HP_times2; // 00=NO 01=YES 02=未知  (val ^ 0xC8) - 1
        public UInt16 UnknownWord25; // ??? (val ^ 0x5209) - 1
        public UInt16 Tradeable; //  不可交易 (val ^ 0x5209) - 1
        public UInt16 UnknownWord27; // ??? (val ^ 0x5209) - 1
        public UInt16 UnknownWord28; // ??? (val ^ 0x5209) - 1
        public UInt16 UnknownWord29; // ??? (val ^ 0x5209) - 1
        public UInt32 UnknownDword2; // ??? (val ^ 0xBAEB716) - 1
        public UInt32 UnknownDword3; // ??? (val ^ 0xBAEB716) - 1
        public UInt32 UnknownDword4; // ??? (val ^ 0xBAEB716) - 1
        public UInt32 UnknownDword5; // ??? (val ^ 0xBAEB716) - 1
        public UInt32 UnknownDword6; // ??? (val ^ 0xBAEB716) - 1

        #region LoadHelpers
        private UInt16 getWord(byte[] data, int ptr)
        {
            return (UInt16)((data[ptr + 1] << 8) + data[ptr]);
        }
        private UInt32 getDWord(byte[] data, int ptr)
        {
            return (UInt32)((data[ptr + 3] << 24) + (data[ptr + 2] << 16) + (data[ptr + 1] << 8) + data[ptr]);
        }
        private byte byteXor(byte v) { return (byte)((v ^ 0xC8) - 1); }
        private UInt16 wordXor(UInt16 v) { return (UInt16)((v ^ 0x5209) - 1); }
        private UInt32 dwordXor(UInt32 v) { return (UInt32)((v ^ 0xBAEB716) - 1); }
        #endregion
        public void Load(byte[] data, int ptr)
        {
            byte len = data[ptr];
            NpcName = "";
            for (int n = 0; n < len; n++) { NpcName += (char)data[ptr + (20 - n)]; } ptr += 21;
            Type = byteXor(data[ptr]); ptr++;
            NpcID = wordXor(getWord(data, ptr)); ptr += 2;
            ImageNum = wordXor(getWord(data, ptr)); ptr += 2;
            ImageNumSmall = wordXor(getWord(data, ptr)); ptr += 2;
            ColorCode1 = dwordXor(getDWord(data, ptr)); ptr += 4;
            ColorCode2 = dwordXor(getDWord(data, ptr)); ptr += 4;
            ColorCode3 = dwordXor(getDWord(data, ptr)); ptr += 4;
            ColorCode4 = dwordXor(getDWord(data, ptr)); ptr += 4;
            Catchable = byteXor(data[ptr]); ptr++;
            UnknownByte2 = byteXor(data[ptr]); ptr++;
            UnknownByte3 = byteXor(data[ptr]); ptr++;
            Level = byteXor(data[ptr]); ptr++;
            HP = dwordXor(getDWord(data, ptr)); ptr += 4;
            SP = (ushort)dwordXor(getDWord(data, ptr)); ptr += 4;
            STR = wordXor(getWord(data, ptr)); ptr += 2;
            CON = wordXor(getWord(data, ptr)); ptr += 2;
            INT = wordXor(getWord(data, ptr)); ptr += 2;
            WIS = wordXor(getWord(data, ptr)); ptr += 2;
            AGI = wordXor(getWord(data, ptr)); ptr += 2;
            ImageNumEnlarge = byteXor(data[ptr]); ptr++;
            element = (ElementType)byteXor(data[ptr]); ptr++;
            SkillID1 = cGlobal.gSkillManager.Get_Skill(wordXor(getWord(data, ptr))); ptr += 2;
            SkillID2 = cGlobal.gSkillManager.Get_Skill(wordXor(getWord(data, ptr))); ptr += 2;
            SkillID3 = cGlobal.gSkillManager.Get_Skill(wordXor(getWord(data, ptr))); ptr += 2;
            ItemID1 = wordXor(getWord(data, ptr)); ptr += 2;
            ItemID2 = wordXor(getWord(data, ptr)); ptr += 2;
            ItemID3 = wordXor(getWord(data, ptr)); ptr += 2;
            ItemID4 = wordXor(getWord(data, ptr)); ptr += 2;
            ItemID5 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownByte5 = byteXor(data[ptr]); ptr++;
            UnknownWord14 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord15 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord16 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord17 = wordXor(getWord(data, ptr)); ptr += 2;
            GeneralAttack1 = byteXor(data[ptr]); ptr++;
            UnknownWord18 = wordXor(getWord(data, ptr)); ptr += 2;
            GeneralAttack2 = byteXor(data[ptr]); ptr++;
            UnknownByte8 = byteXor(data[ptr]); ptr++;
            TalkImage = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord20 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord21 = wordXor(getWord(data, ptr)); ptr += 2;
            SPD = wordXor(getWord(data, ptr)); ptr += 2;
            GeneralAttack3 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownByte9 = byteXor(data[ptr]); ptr++;
            Transferrable = byteXor(data[ptr]); ptr++;
            PK_NPC = byteXor(data[ptr]); ptr++;
            UnknownWord24 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownByte12 = byteXor(data[ptr]); ptr++;
            NPCQuestID = byteXor(data[ptr]); ptr++;
            HumanNPC = byteXor(data[ptr]); ptr++;
            UnknownByte15 = byteXor(data[ptr]); ptr++;
            HP_times2 = byteXor(data[ptr]); ptr++;
            UnknownWord25 = wordXor(getWord(data, ptr)); ptr += 2;
            Tradeable = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord27 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord28 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownWord29 = wordXor(getWord(data, ptr)); ptr += 2;
            UnknownDword2 = dwordXor(getDWord(data, ptr)); ptr += 4;
            UnknownDword3 = dwordXor(getDWord(data, ptr)); ptr += 4;
            UnknownDword4 = dwordXor(getDWord(data, ptr)); ptr += 4;
            UnknownDword5 = dwordXor(getDWord(data, ptr)); ptr += 4;
            UnknownDword6 = dwordXor(getDWord(data, ptr)); ptr += 4;
        }
    }

    public class NpcDat
    {
        bool Loaded = true;
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        public List<Npc> NpcList = new List<Npc>();

        public NpcDat()
        {
        }

        public bool LoadNpc(string path)
        {
            Utilities.LogServices.Log("Loading Npc.Dat");
            try
            {
                NpcList.Clear();
                if (!File.Exists(path)) return false;
                byte[] data = File.ReadAllBytes(path);
                int max = data.Length / 148;
                for (int n = 0; n < max; n++)
                {
                    Npc i = new Npc();
                    i.Load(data, n * 148);
                    NpcList.Add(i);
                }
                Utilities.LogServices.Log("Npc.Dat Loaded ( " + NpcList.Count + " Npc)");
                return true;
            }
            catch (Exception e) { Utilities.LogServices.Log(e); return false; }
        }
        public Npc GetNpc(UInt16 ID)
        {
            foreach (Npc e in NpcList)
                if (e.NpcID == ID)
                    return e;
            return null;
        }
        public Npc GetNpc(string Name)
        {
            foreach (Npc e in NpcList)
                if (e.NpcName == Name)
                    return e;
            return null;
        }
    }
}
