using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;

namespace DataFiles
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class Npc
    {
        public byte NpcNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] NpcName;
        public byte Type; // [代碼不太明白] (val ^ 0xC8) - 1
        public UInt16 NpcID; // (val ^ 0x5209) - 1
        public UInt16 ImageNum; // jma\001.jma  (val ^ 5209) - 1
        public UInt16 ImageNumSmall;//(Small); // jma\007.jma (val ^ 0x5209) - 1
        public UInt32 ColorCode1; // (val ^ 0x0BAEB716) - 1
        public UInt32 ColorCode2; // (val ^ 0x0BAEB716) - 1
        public UInt32 ColorCode3; // (val ^ 0x0BAEB716) - 1
        public UInt32 ColorCode4; // (val ^ 0x0BAEB716) - 1
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
        public byte ImageNumEnlarge;//(Enlarge); //00=正常 01=放大 (val ^ 0xC8) - 1
        public byte element; // 00=無 01=地 02=水 03=火 04=風 (val ^ 0xC8) - 1
        public UInt16 SkillID1; // Pet stunt1 (val ^ 0x5209) - 1
        public UInt16 SkillID2; // Pet stunt2 (val ^ 0x5209) - 1
        public UInt16 SkillID3; // Pet stunt3 (val ^ 0x5209) - 1
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
        public UInt16 TalkImage; // jma\008.jma (val ^ 0x5209) - 1
        public UInt16 UnknownWord20; // 未知參數[] (val ^ 0x5209) - 1
        public UInt16 UnknownWord21; // 未知參數[] (val ^ 0x5209) - 1
        public UInt16 SPD; // 魔物spd (參數＞0 不符合spd公式 　　Ex： Mahabrahman spd=4000 ) (val ^ 0x5209) - 1
        public UInt16 GeneralAttack3; // 參數＞0 普通攻擊以這裡SkillID為主要 (val ^ 0x5209) - 1
        public byte UnknownByte9; // 未知參數[] 00,01 (val ^ 0xC8) - 1
        public byte Transferrable; // 參數＞0 不得轉讓 (val ^ 0xC8) - 1
        public byte PK_NPC; // 00=YES 01=NO (val ^ 0xC8) - 1
        public UInt16 UnknownWord24; // ??? (val ^ 0x5209) - 1
        public byte UnknownByte12; // 未知參數[] (val ^ 0xC8) - 1
        public byte NPCQuestID; //  (val ^ 0xC8) - 1
        public byte HumanNPC; // 00=NO 01=YES (val ^ 0xC8) - 1
        public byte UnknownByte15; // ??? (val ^ 0xC8) - 1
        public byte HP_times2; // 00=NO 01=YES 02=未知  (val ^ 0xC8) - 1
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

    }

    public class PhxNpcDat : IDataManager
    {
        bool Loaded = true;
        public BindingList<Npc> NpcList = new BindingList<Npc>();
        private readonly object m_Lock = new object();

        public event ProgressChangedEventHandler onLoadProgressChanged;
        public Action<int, int, float> onWritePercentChange;
        public Action<object> onDebug;

        public PhxNpcDat()
        {
        }

        public Task<bool> Load(string file)
        {
            return Task.Factory.StartNew<bool>(new Func<bool>(() =>
            {

                try
                {
                    NpcList.Clear();
                    if (onDebug != null) onDebug("Beginning to Load Npc Dat");
                    if (!File.Exists(file)) { if (onDebug != null)onDebug(file + " has not been found"); return false; }
                    onDebug("Loading from " + file);

                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        long max = fs.Length;

                        while (max >= Marshal.SizeOf(typeof(Npc)))
                        {
                            Npc i = ReadFromItems<Npc>(fs);


                            lock (m_Lock) NpcList.Add(i);
                            max -= Marshal.SizeOf(typeof(Npc));
                            if (onLoadProgressChanged != null) onLoadProgressChanged(this,
                                  new ProgressChangedEventArgs((int)Math.Round((decimal)((fs.Length - max) / fs.Length * 100)), null));
                        }
                        fs.Close();
                        fs.Dispose();
                        if (onDebug != null) onDebug("Loaded " + NpcList.Count + " Npc");
                        if (onDebug != null) onDebug("Loading of NpcDat has completed");
                        return true;
                    }

                }
                catch (Exception e) { if (onDebug != null)onDebug(e); return false; }
            }));
        }


        public object GetObject(object a)
        {
            if (a.GetType().IsValueType)
                return GetNpcbyID(ushort.Parse(a.ToString()));
            else
                return GetNpcbyName(a.ToString());
        }
        public Npc GetNpcbyID(UInt16 ID)
        {
            foreach (Npc e in NpcList)
                if (e.NpcID == ID)
                    return e;
            return null;
        }
        public Npc GetNpcbyName(string Name)
        {
            foreach (Npc e in NpcList)
                if (ASCIIEncoding.ASCII.GetString(e.NpcName) == Name)
                    return e;
            return null;
        }

        #region Decode

        private void DecodeItem32(ref UInt32 val)
        {
            val = Convert.ToUInt32((val ^ 0xBAEB716) - 9);
        }

        private void DecodeItem32(ref Int32 val)
        {
            val = Convert.ToInt32((val ^ 0xBAEB716) - 9);
        }

        private void DecodeItem16(ref UInt16 val)
        {
            val = Convert.ToUInt16((val ^ 0x5209) - 9);
        }

        private void DecodeItem8(ref byte val)
        {
            val = Convert.ToByte((val ^ 0xC8) - 9);
        }

        private T ReadFromItems<T>(Stream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }

        #endregion


        public bool bit_set(UInt16 value, UInt16 index)
        {
            return (value & (1 << index)) != 0;
        }

        private byte[] ItemtoBytes<T>(T file)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                IntPtr rawDataPtr = Handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(file, rawDataPtr, false);
            }
            catch (Exception) { }
            finally
            {
                Handle.Free();
            }
            return buffer;
        }

        public bool WriteItems(string file)
        {
            int ptr = 0;
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    for (int a = 0; a < NpcList.Count; a++)
                    {
                        //Encode data first

                        fs.Write(ItemtoBytes(NpcList[a]), 0, Marshal.SizeOf(NpcList[a]));
                        ptr += Marshal.SizeOf(NpcList[a]);
                        if (onWritePercentChange != null) onWritePercentChange(a + 1, NpcList.Count, (((a + 1) / NpcList.Count) * 100));
                    }
                }


            }
            catch { return false; }
            return true;
        }
    }
}
