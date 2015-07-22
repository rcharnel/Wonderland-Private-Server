using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFiles;
using Network;
using RCLibrary.Core.Networking;


namespace Game.Code
{
    public class Inventory
    {
        Player owner;
        private readonly object mylock;
        private InvItem[] m_Items;

        public Inventory(Player src)
        {
            owner = src;
            mylock = new object();
            m_Items = new InvItem[50];
            for (int a = 0; a < 50; a++)
                m_Items[a] = new InvItem();
        }

        public InvItem this[byte key]
        {
            get
            {
                lock (mylock) return m_Items[key - 1];
            }
        }

        #region Events/Funcs/Actions
        public Item onWearEquip(byte loc)
        {
            if ((loc > 0) && (loc < 51))
            {
                if (this[loc].ItemID > 0)
                {
                    InvItem i = new InvItem();
                    i.CopyFrom(this[loc]);
                    this[loc].Clear();
                    return i;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public bool onUnEquip(Item src, byte loc, bool senddata)
        {
            if (src == null) return false;
            return (AddItem(src, loc, senddata) > 0);
        }

        public void onItemDropped_fromMap(byte loc, byte ammt)
        {
            RemoveItem(loc, ammt);
        }

        public bool onItemPickedUp_fromMap(Item item)
        {
            return (AddItem(item) > 0);
        }
        #endregion


        /// <summary>
        /// Used to check if an item exists in the list
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="slot"></param>
        /// <returns>true if exists and returns the first slot location of the item</returns>
        public bool ContainsItem(ushort ItemID, out byte slot)
        {
            lock (mylock)
            {
                for (byte a = 1; a < 51; a++)
                {
                    if (this[a].ItemID == ItemID)
                    {
                        slot = a;
                        return true;
                    }
                }
                slot = 0;
                return false;
            }
        }
        /// <summary>
        /// Clears the Inventory
        /// </summary>
        public void RemoveAll(bool force)
        {
            lock (mylock)
            {
                for (byte a = 1; a < 51; a++)
                    if (!force)
                    {
                        if (this[a].ItemID > 0)
                            RemoveItem((byte)(a), this[a].Ammt);
                    }
                    else
                        this[a].Clear();
            }
        }
        /// <summary>
        /// Removes and Item from the Inventory
        /// </summary>
        /// <param name="at"></param>
        /// <param name="ammt"></param>
        /// <param name="senddata"></param>
        /// <returns>the Item Removed</returns>
        public InvItem RemoveItem(byte at, byte ammt, bool senddata = true)
        {
            lock (mylock)
            {

                InvItem remItem = new InvItem();
                remItem.CopyFrom(this[at]);
                try
                {
                    if (remItem.ItemID > 0)
                    {
                        var matrix = NumbertoMatrix(at);

                        for (byte h = 0; h < remItem.Height; h++)
                            for (byte w = 0; w < remItem.Width; w++)
                            {
                                byte slot = (byte)MatrixtoNumber(matrix[0] + h, matrix[1] + w);
                                if (w == 0 && h == 0)
                                {
                                    if (this[slot].Parent == 0 && (this[at].Ammt == 1 || this[at].Ammt - ammt == 0))
                                    {
                                        this[slot].Clear(); continue;
                                    }
                                    else
                                        this[slot].Ammt -= ammt;
                                }
                                else
                                    this[slot].Clear();
                            }
                        if (senddata) owner.Send(new SendPacket(Packet.FromFormat("bbbb", 23, 9, at, ammt)));
                        return remItem;
                    }
                }
                catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); }
                return null;
            }
        }
        /// <summary>
        /// Adds an item to the Inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="at">default will choose next available space or tries to add at a specific place</param>
        /// <param name="sendData">whether to send data to client</param>
        public int AddItem(Item item, byte at = 0, bool sendData = true)
        {
            lock (mylock)
            {
                if (item == null || item.ItemID == 0) return 0;

                int addammt = item.Ammt;
                int totalammt = 0;

                for (byte a = 1; a < 51; a++)
                {
                    if (at != 0) a = at;

                    SendPacket tmp = new SendPacket();
                    tmp.Pack8(23);
                    tmp.Pack8(6);
                    tmp.Pack16(item.ItemID);

                    byte ammt = 0;

                    for (int b = 0; b < addammt; b++)
                        if (CanPlace(a, item))
                        {
                            var matrix = NumbertoMatrix(a);
                            for (byte h = 0; h < item.Height; h++)
                                for (byte w = 0; w < item.Width; w++)
                                {
                                    byte slot = (byte)MatrixtoNumber(matrix[0] + h, matrix[1] + w);
                                    if (w == 0 && h == 0)
                                    {
                                        if (this[slot].ItemID == 0 && this[slot].Parent == 0)
                                        {
                                            this[slot].CopyFrom(item);
                                            this[slot].Ammt = 1;
                                        }
                                        else if (this[slot].SpaceLeft == 0) goto end;
                                        else
                                            this[slot].Ammt++;
                                        ammt++;
                                        totalammt++;
                                    }
                                    else
                                        this[slot].Parent = (byte)a;
                                }


                            //if (i.itemtype.ItemType == 39)
                            //    own.vechile.Add(i.ID, mast);
                        }
                        else
                            goto end;

                end:
                    if (ammt > 0 && sendData)
                    {
                        addammt -= ammt;
                        tmp.Pack8(ammt);
                        tmp.Pack32(0);
                        tmp.Pack32(0);
                        tmp.Pack32(0);
                        tmp.Pack32(0);
                        tmp.Pack32(0);
                        tmp.Pack32(0);
                        tmp.Pack16(0);
                        tmp.SetHeader();
                        owner.Send(tmp);
                    }
                    if (totalammt == item.Ammt || at != 0)
                        return ammt;
                }
                return 0;
            }
        }
        /// <summary>
        /// Moves and item in the Inventory List
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="ammt"></param>
        public void MoveItem(byte from, byte to, byte ammt)
        {
            lock (mylock)
            {
                if (this[from].ItemID == 0 || from == to) return;

                SendPacket tmp = new SendPacket();
                tmp.Pack8(23);
                tmp.Pack8(10);
                tmp.Pack8(from);

                var item = RemoveItem(from, ammt, false);

                if (item != null && (this[to].ItemID == 0 || (this[to].ItemID == item.ItemID)))
                {
                    byte wasplaced = (byte)AddItem(item, to, false);
                    if (wasplaced > 0)
                    {
                        tmp.Pack8(wasplaced);
                        tmp.Pack8(to);
                        tmp.SetHeader();
                        owner.Send(tmp);
                    }
                }
            }
        }

        public void ProcessSocket(Packet p)
        {
            p.m_nUnpackIndex = 4;

            var a = p.Unpack8();
            var b = p.Unpack8();

            if (a != 23) return;

            switch (b)
            {
                #region move item inv
                case 10:
                    {
                        byte src = p.Unpack8();
                        byte ammt = p.Unpack8();
                        byte dst = p.Unpack8();

                        if (((src > 0) && (src < 51)) && ((dst > 0) && (dst < 51)) && ((ammt > 0) && (ammt < 51)))
                            MoveItem(src, dst, ammt);
                    } break;
                #endregion
                #region item being used
                case 15:
                    {
                        byte pos = p.Unpack8();

                        if (this[pos].ItemID > 0)
                            switch (this[pos].Type)
                            {
                                //case eItemType.Tent: owner.Tent.Open(); break;
                            }
                    }
                    break;
                #endregion
                #region destroy an item
                case 124:
                    {
                        byte pos = p.Unpack8();
                        byte qnt = p.Unpack8();
                        byte ukn = p.Unpack8(); //??

                        if (this[pos].ItemID > 0)
                        {
                            // test confirm destroy item
                            owner.Send(new SendPacket(Packet.FromFormat("bbwb", 23, 26, this[pos].ItemID, qnt)));
                            RemoveItem(pos, qnt);
                        }
                    } break;
                #endregion
            }
        }

        bool CanPlace(byte cell, Item item)
        {
            lock (mylock)
            {
                var matrix = NumbertoMatrix(cell);
                for (int s = 0; s < item.Height; s++)
                    for (int y = 0; y < item.Width; y++)
                    {
                        var chk = (byte)MatrixtoNumber(matrix[0] + s, matrix[1] + y);
                        if (this[chk].ItemID == 0 && this[chk].Parent == 0) continue;
                        else if (matrix[0] + (item.Height - 1) > 51 && matrix[1] + (item.Width - 1) > 6) return false;
                        else if (this[chk].ItemID != 0 && this[chk].Parent != 0) return false;
                        else if (this[chk].ItemID != 0 && this[chk].ItemID != item.ItemID) return false;
                    }
                return true;
            }
        }
        public int FilledCount
        {
            get { lock (mylock) { return m_Items.Count(c => c.ItemID > 0); } }
        }
        public int unFilledCount
        {
            get { lock (mylock) { return 50 - FilledCount; } }
        }

        public IEnumerable<byte> GetAC23_5()
        {
            lock (mylock)
            {
                SendPacket tmp = new SendPacket(false);
                tmp.Pack8(23);
                tmp.Pack8(5);
                if (FilledCount > 0)
                {
                    for (byte a = 1; a < 51; a++)
                        if (this[a].ItemID != 0)
                        {
                            tmp.Pack8(a);
                            tmp.Pack16(this[a].ItemID);
                            tmp.Pack8(this[a].Ammt);
                            tmp.Pack8(this[a].Damage);
                            tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                        }
                }
                return tmp.Buffer;
            }
        }

        public Dictionary<byte, uint[]> InventoryDBData
        {
            get
            {
                lock (mylock)
                {
                    Dictionary<byte, uint[]> tmp = new Dictionary<byte, uint[]>();

                    for(byte a =1;a<51;a++)
                        tmp.Add(a, new uint[] { this[a].ItemID, this[a].Damage, this[a].Ammt, a, 0, 0, 0, 0 });
                    return tmp;
                }
            }
        }



        public void onItemUsed(byte slot, byte tgrt, byte ammt)
        {
            //Get item first
            //if (m_Items[slot].ItemID > 0)
            //{
            //    switch (m_Items[slot].Type)
            //    {
            //        case eItemType.tent: host.Tent.Open(); break;
            //        default:
            //            {
            //                host.DataOut = SendType.Multi;
            //                //switch (tgrt)
            //                //{
            //                //    case 0: for (int a = 0; a < 2; a++) host.AddStat((byte)Items[slot].Data.StatusType[a], (byte)Items[slot].Data.StatusUp[a]); break;
            //                //    //default:for (int a = 0; a < 2; a++) 
            //                //}
            //                m_Items[slot].Ammt -= 1;
            //                SendPacket p = new SendPacket();
            //                p.PackArray(new byte[] { 23, 9 });
            //                p.Pack((byte)slot);
            //                p.Pack((byte)ammt);
            //                host.Send(p);
            //                p = new SendPacket();
            //                p.PackArray(new byte[] { 23, 15 });
            //                host.Send(p);
            //                host.DataOut = SendType.Normal;
            //            } break;
            //    }
            //}
        }
        public void onItemCanceled(byte slot)
        {
            //Get item first
            if (m_Items[slot].ItemID > 0)
                switch (m_Items[slot].Type)
                {
                    //case eItemType.Tent: owner.Tent.Close(); break;
                }
        }
        int MatrixtoNumber(int a, int b) { return ((a * 5) + (b - 5)); }//wlo specific
        byte[] NumbertoMatrix(int a)
        {
            var s = 0;
            if (a == 5 || a == 10 || a == 15 || a == 20 || a == 25 || a == 30 || a == 35 || a == 40 || a == 45 || a == 50)
                s = (a / 5);
            else
                s = 1 + (a / 5);
            var t = 0;
            if (a == 5 || a == 10 || a == 15 || a == 20 || a == 25 || a == 30 || a == 35 || a == 40 || a == 45 || a == 50)
                t = 5;
            else if (a > 5)
                t = 5 - (((1 + (a / 5)) * 5) - a);
            else
                t = a;
            byte[] matrixloc = new byte[2];
            matrixloc[0] = (byte)(s);
            matrixloc[1] = (byte)(t);
            return matrixloc;
        }//wlo specific
    }

    public class TentInventoryManager : Inventory
    {

        public TentInventoryManager(Player src):base(src)
        {

        }
    }
}
