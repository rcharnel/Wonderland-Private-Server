using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.DataManagement.DataFiles;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Code.Objects
{
    public class cItem
    {
        ItemData data; 
        public byte Ammt;
        public byte Damage;
        public byte ParentSlot;
        public bool locked;

        public cItem()
        {
            Clear();
        }
        public cItem(ItemData src):base()
        {
            data = src;
        }

        #region Properties

        public eItemType Type { get { return Data.itemType; } }
        /// <summary>
        /// an Item's Data
        /// </summary>
        protected ItemData Data { get { return data ?? new ItemData(); } }
        public UInt16 ItemID { get { return (Data != null) ? Data.ItemID : (ushort)0; } }        
        public int InvHeight { get {return (Data != null) ? Data.InvHeight : (ushort)0; } }
        public int InvWidth { get { return (Data != null) ? Data.InvWidth : (ushort)0; } }
        public ushort Control { get { return Data.Control; } }
        public eWearSlot Equippped_At { get { return Data.EquipPos; } }
        public byte Level { get { return Data.Level; } }
        public ushort NpcID { get { return Data.NpcID; } }

        /// <summary>
        /// Determines if an Item can be dropped or it must be destroyed
        /// </summary>
        public bool Dropable
        {
            get
            {
                ushort[] nodrop = new ushort[] { };

                return ((Data.able_to_drop) && (Data.NpcID == 0) && !nodrop.Contains(Data.Control));
            }
        }
        /// <summary>
        /// Determines if the Item is Stackable
        /// </summary>
        public bool Stackable { get { return (Data != null) ? Data.able_to_stack : false; } }
        public bool Tradeable { get { return false; } }
        public bool Repairable { get { return false; } }

        #endregion


        public void Clear()
        {
            data = new ItemData();
            Ammt = 0;
            Damage = 0;
            ParentSlot = 0;
            locked = false;
        }

        public void CopyFrom(cItem i)
        {
            if (i != null)
            {
                data = i.Data;
                Ammt = i.Ammt;
                Damage = i.Damage;
                ParentSlot = i.ParentSlot;
                locked = i.locked;
            }
        }
    }


    public class EquipmentCell : cItem
    {
        public EquipmentCell()
        {
        }

        public Int32 HP
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 207) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 207) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 SP
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 208) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 208) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 ATK
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 210) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 210) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 DEF
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 211) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 211) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 MAT
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 215) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 215) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 MDF
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 216) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 216) ? (Int32)Data.statVal[1] : 0;
                return val;
            }
        }

        public Int32 SPD
        {
            get
            {
                int val = 0;
                val += (Data.statType[0] == 214) ? (Int32)Data.statVal[0] : 0;
                val += (Data.statType[1] == 214) ? (Int32)Data.statVal[1] : 0;
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
                return (Data.SpecialStatus == eSpecialStatus.Critical_Increase) ? Data.ItemRank * 2 + 10 : 0;
            }
        }

    }


    public class InvItemCell : cItem
    {
        public int SpaceLeft { get { if (Stackable) return 50 - Ammt; else return 0; } }
    }
    public class DroppedItem : cItem
    {
        public bool NonExpirable;
        bool Expired;
        DateTime dropped;
        public DateTime Expires;

        public UInt16 X = 0;
        public UInt16 Y = 0; 
    }

    public class TentItem:cItem
    {
        public byte index;
        public cItem citem;
        public ushort ItemID;
        public byte tentX;
        public byte tentY;
        public byte tentZ;
        public byte qnt;
        public byte rotate;
        public byte especial; // itens stoneKnife ,woodenSaw
        public byte[] accessory;  //testing tmp
        public byte a1;
        public byte a2;
        public byte a3;
        public byte a4;
        public byte a5;
        public byte a6;
        public byte a7;
        public byte a8;
        public byte a9;
        public byte a10;
        public byte ukn; // type = 10 object
        public byte pick; // item without space
        public byte floor; // 1 = 2 floor 0 = 1 floor

        public TentItem()
        {
          accessory = new byte[10];
        }
       
    }
}
