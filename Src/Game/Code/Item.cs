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
}
