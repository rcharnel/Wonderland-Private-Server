using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    //public class TentItem
    //{
    //    ItemData data; public ItemData Data { get { return data ?? new ItemData(); } private set { data = value; } }
    //    public uint TentX { get; set; }
    //    public uint TentY { get; set; }
    //    public byte Visible { get; set; }
    //    public byte Rotation { get; set; }
    //    public byte Especial { get; set; }
    //    public byte Acessory0 { get; set; }
    //    public byte Acessory1 { get; set; }
    //    public byte Acessory2 { get; set; }
    //    public byte Acessory3 { get; set; }
    //    public byte Acessory4 { get; set; }
    //    public byte Acessory5 { get; set; }
    //    public byte Acessory6 { get; set; }
    //    public byte Acessory7 { get; set; }
    //    public byte Object { get; set; } // 10 = objects
    //    public byte Floor { get; set; } //0 = 1f //1 = 2f
    //    public byte Pick { get; set; }
    //    public bool SecondFloor { get; set; }
       

    //}
    public class ItemSale
    {
        public uint PlayerID { get; set; }
        public ushort ItemID { get; set; }
        public byte Pos { get; set; }
        public byte Tool { get; set; }
        public byte qnt { get; set; }
        public byte PosInv { get; set; }
        public uint Price { get; set; }

    }
}
