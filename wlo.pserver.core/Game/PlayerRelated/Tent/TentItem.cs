using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    public class TentItem
    {

        PhxItemInfo data; public PhxItemInfo Data { get { return data ?? new PhxItemInfo(); } private set { data = value; } }


        public TentItem(PhxItemInfo src)
        {
            Data = src;
        }

        public uint TentX { get; set; }
        public uint TentY { get; set; }
        public uint TentZ { get; set; }
        public byte Rotation { get; set; }
        public byte Floor { get; set; } //0 = 1f //1 = 2f

    }

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
