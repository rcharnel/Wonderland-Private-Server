using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PhoenixData;

namespace DataFiles
{
    //public class ItemData
    //{
    //    public byte[] mydata;
    //    public string Name;// byte[] ItemName;
    //    public eItemType itemType;
    //    public UInt16 ItemID;
    //    public UInt16 IconNum;
    //    public UInt16 LargeIconNum;
    //    public UInt16[] EquipImageNum = new UInt16[4];
    //    public UInt16[] statType = new UInt16[2];
    //    public byte UnknownByte1;
    //    public byte UnknownByte2;
    //    public UInt16[] statVal = new UInt16[2];
    //    public UInt16 unknonVal;
    //    public UInt16 unknonVal1;
    //    public byte UnknownByte3;
    //    public byte ItemRank;
    //    public eWearSlot EquipPos;
    //    public eSpecialStatus SpecialStatus;
    //    public UInt32[] ColorDef = new UInt32[16];
    //    public byte UnknownByte4;
    //    public byte Unused;
    //    public byte Level;
    //    public UInt32 BuyingPrice;
    //    public UInt32 SellingPrice;
    //    public byte EquipLimit;
    //    public UInt16 Control;
    //    public UInt32 UnknownDWord1;
    //    public byte SetID;
    //    public UInt32 AntiSeal;
    //    public UInt16 SkillID;
    //    public UInt16[] MaterialTypes = new UInt16[5];
    //    public string Desccription;
    //    public byte TentWidth;
    //    public byte TentHeight;
    //    public byte TentDepth;
    //    public UInt16 UnknownWord2;
    //    public byte InvWidth;
    //    public byte InvHeight;
    //    public byte UnknownByte5;
    //    public UInt16[] InTentImages = new UInt16[2];
    //    public UInt16 NpcID;
    //    public byte UnknownByte6;
    //    public byte UnknownByte7;
    //    public byte UnknownByte8;
    //    public byte UnknownByte9;
    //    public byte UnknownByte10;
    //    public byte UnknownByte11;
    //    public UInt16 Duration;
    //    public UInt16 UnknownWord4;
    //    public UInt16 CapsuleForm;
    //    public UInt16 UnknownWord6;
    //    public UInt16 UnknownWord7;
    //    public UInt32 UnknownDWord2;
    //    public UInt32 UnknownDWord3;
    //    public UInt32 UnknownDWord4;
    //    public UInt32 UnknownDWord5;
    //    public UInt32 UnknownDWord6;

    //    #region Data Load Helpers
    //    private UInt16 Unpack16(byte[] data, int ptr)
    //    {
    //        return (UInt16)((data[ptr + 1] << 8) + data[ptr]);
    //    }
    //    private UInt32 getDWord(byte[] data, int ptr)
    //    {
    //        return (UInt32)((data[ptr + 3] << 24) + (data[ptr + 2] << 16) + (data[ptr + 1] << 8) + data[ptr]);
    //    }
    //    private byte byteXor(byte v) { return (byte)((v ^ 0x9A) - 9); }
    //    private UInt16 wordXor(UInt16 v) { return (UInt16)((v ^ 0xEFC3) - 9); }
    //    private UInt32 dwordXor(UInt32 v) { return (UInt32)((v ^ 0x0B80F4B4) - 9); }
    //    #endregion
    //    #region Item Attributes
    //    public bool able_to_trade
    //    {
    //        get
    //        {
    //            switch ((byte)itemType)
    //            {
    //                case 29: return false;
    //                case 28: return true;
    //                default: return false;
    //            }
    //        }
    //    }
    //    public bool able_to_drop
    //    {
    //        get
    //        {
    //            switch ((byte)itemType)
    //            {
    //                case 23:
    //                case 3:
    //                case 31:
    //                case 32:
    //                case 1:
    //                case 10:
    //                case 12:
    //                case 14:
    //                case 15:
    //                case 5:
    //                case 6:
    //                case 8:
    //                case 9:
    //                case 4:
    //                case 37:
    //                case 36:
    //                case 35:
    //                case 34:
    //                case 33:
    //                case 2:
    //                case 13: return true;
    //                default: return false;
    //            }
    //        }
    //    }
    //    public bool able_to_stack
    //    {
    //        get
    //        {
    //            switch ((byte)itemType)
    //            {
    //                case 10: return true;
    //                case 15: return false;
    //                case 17:
    //                case 20:
    //                case 21:
    //                case 23:
    //                case 24:
    //                case 25:
    //                case 26:
    //                case 28:
    //                case 30:
    //                case 31:
    //                case 32:
    //                case 34:
    //                case 35:
    //                case 36:
    //                case 37:
    //                case 38:
    //                case 40:
    //                case 41:
    //                case 51:
    //                case 52:
    //                case 54:
    //                case 33: return true;
    //                default: return false;
    //            }
    //        }
    //    }
    //    #endregion

    //    public void Load()
    //    {
    //        int ptr = 0;
    //        byte[] data = mydata;
    //        byte len = data[ptr]; Name = "";
    //        for (int n = 0; n < len; n++) { Name += (char)data[ptr + (20 - n)]; } ptr += 21;
    //        itemType = (eItemType)byteXor(data[ptr]); ptr++;
    //        ItemID = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        if (ItemID == 12072)
    //        {
    //        }
    //        IconNum = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        LargeIconNum = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        for (int n = 0; n < EquipImageNum.Length; n++) { EquipImageNum[n] = wordXor(Unpack16(data, ptr)); ptr += 2; }
    //        for (int n = 0; n < statType.Length; n++) { statType[n] = wordXor(Unpack16(data, ptr)); ptr += 2; }
    //        UnknownByte1 = byteXor(data[ptr]); ptr++;
    //        UnknownByte2 = byteXor(data[ptr]); ptr++;
    //        statVal[0] = (UInt16)((Unpack16(data, ptr) ^ 0xF4B4) - 109); ptr += 2;
    //        unknonVal = (UInt16)((Unpack16(data, ptr) ^ 0xF4B4) - 109); ptr += 2;
    //        statVal[1] = (UInt16)((Unpack16(data, ptr) ^ 0xF4B4) - 109); ptr += 2;
    //        unknonVal1 = (UInt16)((Unpack16(data, ptr) ^ 0xF4B4) - 109); ptr += 2;
    //        UnknownByte3 = byteXor(data[ptr]); ptr++;
    //        ItemRank = byteXor(data[ptr]); ptr++;
    //        EquipPos = (eWearSlot)byteXor(data[ptr]); ptr++;
    //        SpecialStatus = (eSpecialStatus)byteXor(data[ptr]); ptr++;//specialstatus
    //        for (int n = 0; n < ColorDef.Length; n++) { ColorDef[n] = dwordXor(getDWord(data, ptr)); ptr += 4; }
    //        Unused = byteXor(data[ptr]); ptr++;
    //        Level = byteXor(data[ptr]); ptr++;
    //        BuyingPrice = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        SellingPrice = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        EquipLimit = byteXor(data[ptr]); ptr++;//use limit maybe
    //        Control = wordXor(Unpack16(data, ptr)); ptr += 2;//control
    //        UnknownDWord1 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        SetID = byteXor(data[ptr]); ptr++;
    //        AntiSeal = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        SkillID = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        MaterialTypes = new UInt16[5];
    //        for (int n = 0; n < MaterialTypes.Length; n++) { MaterialTypes[n] = wordXor(Unpack16(data, ptr)); ptr += 2; }

    //        len = data[ptr]; Desccription = "";
    //        for (int n = 0; n < len; n++) { Desccription += (char)data[ptr + (254 - n)]; } ptr += 255;
    //        TentWidth = byteXor(data[ptr]); ptr++;
    //        TentHeight = byteXor(data[ptr]); ptr++;
    //        TentDepth = byteXor(data[ptr]); ptr++;
    //        UnknownWord2 = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        InvWidth = byteXor(data[ptr]); ptr++;
    //        InvHeight = byteXor(data[ptr]); ptr++;
    //        UnknownByte5 = byteXor(data[ptr]); ptr++;
    //        for (int n = 0; n < InTentImages.Length; n++) { InTentImages[n] = wordXor(Unpack16(data, ptr)); ptr += 2; }
    //        NpcID = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        UnknownByte6 = byteXor(data[ptr]); ptr++;
    //        UnknownByte7 = byteXor(data[ptr]); ptr++;
    //        UnknownByte8 = byteXor(data[ptr]); ptr++;
    //        UnknownByte9 = byteXor(data[ptr]); ptr++;
    //        UnknownByte10 = byteXor(data[ptr]); ptr++;
    //        UnknownByte11 = byteXor(data[ptr]); ptr++;
    //        Duration = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        UnknownWord4 = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        CapsuleForm = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        UnknownWord6 = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        UnknownWord7 = wordXor(Unpack16(data, ptr)); ptr += 2;
    //        UnknownDWord2 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        UnknownDWord3 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        UnknownDWord4 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        UnknownDWord5 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //        UnknownDWord6 = dwordXor(getDWord(data, ptr)); ptr += 4;
    //    }
    //}

    public class Item : Phoenix.Core.Controls.IUpdatableControl
    {
        PhxItemInfo data;
        byte ammt;
        byte damage;
        byte parentSlot;
        bool locked;

        public Item()
        {
            Clear();
        }
        public Item(PhxItemInfo i)
        {
            Clear();
            CopyFrom(i);
        }
        public Item(Item i)
        {
            Clear();
            CopyFrom(i);
        }


        #region Properties

        public byte Ammt { get { return ammt; } set { SetField<byte>(ref ammt, value); } }
        public byte Damage { get { return damage; } set { SetField<byte>(ref damage, value); } }
        public byte Parent { get { return parentSlot; } set { SetField<byte>(ref parentSlot, value); } }
        public bool isLocked { get { return locked; } set { SetField<bool>(ref locked, value); } }


        public eItemType Type { get { return (eItemType)Data.ItemType; } }
        /// <summary>
        /// an Item's Data
        /// </summary>
        protected PhxItemInfo Data { get { return data ?? new PhxItemInfo(); } }
        public string Name { get { return ASCIIEncoding.ASCII.GetString(Data.ItemName); } }
        public UInt16 ItemID { get { return (Data != null) ? Data.ItemID : (ushort)0; } }
        public int Height { get { return (Data != null) ? Data.cellheight : (ushort)0; } }
        public int Width { get { return (Data != null) ? Data.cellwidth : (ushort)0; } }
        //public ushort Control { get { return Data.Control; } }
        public eWearSlot Wear_At { get { return (eWearSlot)Data.Equippos; } }
        public byte Level { get { return (byte)Data.level; } }
        //public ushort NpcID { get { return Data.NpcID; } }

        /// <summary>
        /// Determines if an Item can be dropped or it must be destroyed
        /// </summary>
        public bool Dropable
        {
            get
            {
                switch ((byte)Type)
                {
                    case 23:
                    case 3:
                    case 31:
                    case 32:
                    case 1:
                    case 10:
                    case 12:
                    case 14:
                    case 15:
                    case 5:
                    case 6:
                    case 8:
                    case 9:
                    case 4:
                    case 37:
                    case 36:
                    case 35:
                    case 34:
                    case 33:
                    case 2:
                    case 13: return true;
                    default: return false;
                }
            }
        }
        /// <summary>
        /// Determines if the Item is Stackable
        /// </summary>
        public bool Stackable
        {
            get
            {
                switch ((byte)Type)
                {
                    case 10: return true;
                    case 15: return false;
                    case 17:
                    case 20:
                    case 21:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 28:
                    case 30:
                    case 31:
                    case 32:
                    case 34:
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 40:
                    case 41:
                    case 51:
                    case 52:
                    case 54:
                    case 33: return true;
                    default: return false;
                }
            }
        }
        public bool Tradeable
        {
            get
            {
                switch ((byte)Type)
                {
                    case 29: return false;
                    case 28: return true;
                    default: return false;
                }
            }
        }
        public bool Repairable { get { return false; } }

        #endregion

        /// <summary>
        /// Clears the Values from this Item object
        /// </summary>
        public void Clear()
        {
            data = null;
            ammt = 0;
            damage = 0;
            parentSlot = 0;
            locked = false;
        }
        /// <summary>
        /// Copys values from another Item object
        /// </summary>
        /// <param name="i"></param>
        public void CopyFrom(Item
 i)
        {
            if (i != null)
            {
                data = i.Data;
                ammt = i.Ammt;
                damage = i.Damage;
                parentSlot = i.Parent;
                locked = i.isLocked;
            }
        }
        public void CopyFrom(PhxItemInfo
 i)
        {
            if (i != null)
            {
                data = i;
                ammt = 1;
                damage = 0;
                parentSlot = 0;
                locked = false;
            }
        }
    }


    //public class ItemManager:IDataManager
    //{
    //    readonly object mylock;

    //    Dictionary<ushort, Item> itemList = new Dictionary<ushort, Item>();

    //    public ItemManager()
    //    {
    //        mylock = new object();
    //    }

    //    public Item GetItem(ushort id)
    //    {
    //        lock (mylock)
    //        {
    //            if (itemList.ContainsKey(id))
    //                return itemList[id];
    //            else
    //                return null;
    //        }
    //    }

    //    public bool LoadItems(string path)
    //    {
    //        lock (mylock)
    //        {
    //            try
    //            {
    //                itemList.Clear();
    //                if (!File.Exists(path)) return false;
    //                DebugSystem.Write("Loading Item.Dat");
    //                byte[] data = File.ReadAllBytes(path);
    //                int max = data.Length / 457;
    //                int at = 0;
    //                for (int n = 0; n < max; n++)
    //                {
    //                    PhxItemInfo f = new PhxItemInfo();
    //                    f.mydata = new byte[457];
    //                    Array.Copy(data, at, f.mydata, 0, 457);
    //                    f.lo.Load();
    //                    Item i = new Item(f);
    //                    i.Ammt = 1;
    //                    itemList.Add(i.ItemID, i);
    //                    at += 457;
    //                }
    //                DebugSystem.Write("Item.Dat Loaded ( " + itemList.Count + " Items)");
    //                return true;
    //            }
    //            catch (Exception ex) { DebugSystem.Write(new ExceptionData(ex)); }
    //            itemList.Clear();
    //            DebugSystem.Write("Item.dat not Loaded");
    //            return false;
    //        }
    //    }
    //}
}
