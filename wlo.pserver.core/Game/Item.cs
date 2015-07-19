using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFiles;

namespace Game.Code
{

    public class EquipmentCell : Item
    {
        public EquipmentCell()
        {
        }

        public Int32 HP
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 207) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 207) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 SP
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 208) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 208) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 ATK
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 210) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 210) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 DEF
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 211) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 211) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 MAT
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 215) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 215) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 MDF
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 216) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 216) ? (Int32)Data.StatusUp[1] : 0;
                return val;
            }
        }

        public Int32 SPD
        {
            get
            {
                int val = 0;
                //val += (Data.StatusType[0] == 214) ? (Int32)Data.StatusUp[0] : 0;
                //val += (Data.StatusType[1] == 214) ? (Int32)Data.StatusUp[1] : 0;
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
                return 0;// (Data.SpecialStatus == eSpecialStatus.Critical_Increase) ? Data.ItemRank * 2 + 10 : 0;
            }
        }

    }


    public class InvItem : Item
    {
        public int SpaceLeft { get { if (Stackable) return 50 - Ammt; else return 0; } }
    }

    public class DroppedItem : Item
    {
        public bool NonExpirable;
        bool Expired;
        DateTime dropped;
        public DateTime Expires;

        public UInt16 X = 0;
        public UInt16 Y = 0; 
    }

    public class TentItem:Item
    {
        public byte index;

        public ushort tentX;
        public ushort tentY;
        public ushort tentZ;
        public byte rotate;
        public byte especial; // itens stoneKnife ,woodenSaw
        byte[] accessory = new byte[10];  //testing tmp
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
        }
       
    }



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
}
