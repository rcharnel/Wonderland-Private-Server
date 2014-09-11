using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Code.Objects
{
    public class InventoryManager
    {
        private readonly object mylock;
        private Dictionary<byte,InvItemCell> Items;
        Player host;

        public InventoryManager(Player src)
        {
            host = src;
            mylock = new object();
            Items = new Dictionary<byte, InvItemCell>();
             for (byte a = 1; a < 51; a++)
               Items.Add(a, new InvItemCell());
        }
        
        public InvItemCell this[byte key]
        {
            get
            {
                lock (mylock)
                {
                    // If this key is in the dictionary, return its value.
                    if (Items.ContainsKey(key))
                    {
                        // The key was found; return its value. 
                        return Items[key];
                    }
                    else
                    {
                        // The key was not found; return null. 
                        return null;
                    }
                }
            }

            set
            {
                lock (mylock)
                {
                    // If this key is in the dictionary, change its value. 
                    if (Items.ContainsKey(key))
                    {
                        // The key was found; change its value.
                        Items[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Used to check if an item exists in the list
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="slot"></param>
        /// <returns>true if exists and returns the first slot location of the item</returns>
        public bool ContainsItem(ushort ItemID, out byte slot)
        {
            lock(mylock)
            {
            for (byte a = 1; a < Items.Count; a++)
            {
                if (Items[a].ItemID == ItemID)
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
        public void RemoveAll()
        {
            lock(mylock)
            {
            for(byte a = 1;a<51;a++)
                if(Items[a].ItemID > 0)
                RemoveItem(a, Items[a].Ammt);
            }
        }
        /// <summary>
        /// Removes and Item from the Inventory
        /// </summary>
        /// <param name="at"></param>
        /// <param name="ammt"></param>
        /// <param name="senddata"></param>
        /// <returns>the Item Removed</returns>
        public InvItemCell RemoveItem(byte at, byte ammt, bool senddata = true)
        {
            lock (mylock)
            {
                
                InvItemCell remItem = new InvItemCell();
               remItem.CopyFrom(Items[at]);
                if (remItem.ItemID > 0)
                {
                    var matrix = NumbertoMatrix(at);

                    for (byte h = 0; h < remItem.InvHeight; h++)
                        for (byte w = 0; w < remItem.InvWidth; w++)
                        {
                            byte slot = (byte)MatrixtoNumber(matrix[0] + h, matrix[1] + w);
                            if (w == 0 && h == 0)
                            {
                                if (Items[slot].ParentSlot == 0 && (Items[at].Ammt == 1 || Items[at].Ammt - ammt == 0))
                                {
                                    Items[slot].Clear(); continue;
                                }
                                else
                                    Items[slot].Ammt -= ammt;
                            }
                            else
                                Items[slot].Clear();
                        }
                    if (senddata)
                    {
                        SendPacket p = new SendPacket();
                        p.PackArray(new byte[] { 23, 9 });
                        p.Pack8(at);
                        p.Pack8(ammt);
                        host.Send(p);
                    }
                    return remItem;
                }
                return null;
            }
        }
        /// <summary>
        /// Adds an item to the Inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="at">default will choose next available space or tries to add at a specific place</param>
        /// <param name="sendData">whether to send data to client</param>
        public int AddItem(InvItemCell item,byte at = 0 ,bool sendData = true)
        {
            lock (mylock)
            {
                if (item.ItemID == 0) return 0;

                int addammt = item.Ammt;
                int totalammt = 0;

                for (byte a = 1; a < 51; a++)
                {
                    if (at != 0) a = at;

                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 23, 6 });
                    tmp.Pack16(item.ItemID);

                    byte ammt = 0;

                    for (int b = 0; b < addammt; b++)
                        if (CanPlace(a, item))
                        {
                            var matrix = NumbertoMatrix(a);
                            for (byte h = 0; h < item.InvHeight; h++)
                                for (byte w = 0; w < item.InvWidth; w++)
                                {
                                    byte slot = (byte)MatrixtoNumber(matrix[0] + h, matrix[1] + w);
                                    if (w == 0 && h == 0)
                                    {
                                        if (Items[slot].ItemID == 0 && Items[slot].ParentSlot == 0)
                                        {
                                            Items[slot].CopyFrom(item);
                                            Items[slot].Ammt = 1;
                                        }
                                        else if (Items[slot].SpaceLeft == 0) goto end;
                                        else
                                            Items[slot].Ammt++;
                                        ammt++;
                                        totalammt++;
                                    }
                                    else
                                        Items[slot].ParentSlot = (byte)a;
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
                        host.Send(tmp);
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
                if (Items[from].ItemID == 0 || from == to) return;

                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 23, 10 });
                tmp.Pack8(from);

                var item = RemoveItem(from, ammt, false);

                if (item != null && (Items[to].ItemID == 0 || (Items[to].ItemID == item.ItemID)))
                {
                    byte wasplaced = (byte)AddItem(item, to, false);
                    if (wasplaced > 0)
                    {
                        tmp.Pack8(wasplaced);
                        tmp.Pack8(to);
                        host.Send(tmp);
                    }
                }
            }
        }
        bool CanPlace(byte cell, InvItemCell item)
        {
            lock (mylock)
            {
                var matrix = NumbertoMatrix(cell);
                for (int s = 0; s < item.InvHeight; s++)
                    for (int y = 0; y < item.InvWidth; y++)
                    {
                        var chk = (byte)MatrixtoNumber(matrix[0] + s, matrix[1] + y);
                        if (Items[chk].ItemID == 0 && Items[chk].ParentSlot == 0) continue;
                        else if (matrix[0] + (item.InvHeight - 1) > 51 && matrix[1] + (item.InvWidth - 1) > 6) return false;
                        else if (Items[chk].ItemID != 0 && Items[chk].ParentSlot != 0) return false;
                    }
                return true;
            }
        }
        public int FilledCount
        {
            get { lock (mylock) { return Items.Count(c => c.Value.ItemID > 0); } }
        }
        public int unFilledCount
        {
            get { lock (mylock) { return 50 - FilledCount; } }
        }

        public byte[] _23_5Data
        {
            get
            {
                lock (mylock)
                {
                    Packet tmp = new Packet();
                    if (FilledCount > 0)
                    {
                        for (byte a = 1; a < Items.Count; a++)
                            if (Items[a].ItemID != 0)
                            {
                                tmp.Pack8(a);
                                tmp.Pack16(Items[a].ItemID);
                                tmp.Pack8(Items[a].Ammt);
                                tmp.Pack8(Items[a].Damage);
                                tmp.PackArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                            }
                    }
                    return tmp.Data.ToArray();
                }
            }
        }

        public Dictionary<byte, uint[]> InventoryDBData
        {
            get
            {
                lock (mylock)
                {
                    Dictionary<byte, uint[]> tmp = new Dictionary<byte, uint[]>();

                    foreach (var f in Items)
                        tmp.Add((byte)f.Key, new uint[] { f.Value.ItemID, f.Value.Damage, f.Value.Ammt, f.Key, 0, 0, 0, 0 });
                    return tmp;
                }
            }
        }
        public void onItemUsed(byte slot, byte tgrt, byte ammt)
        {
            //Get item first
            if (Items[slot].ItemID > 0)
            {
                switch (Items[slot].Data.itemType)
                {
                    case eItemType.tent: host.Tent.Open(); break;
                    default:
                        {
                            host.DataOut = SendType.Multi;
                            switch (tgrt)
                            {
                                case 0: for (int a = 0; a < 2; a++) host.AddStat((byte)Items[slot].Data.statType[a], (byte)Items[slot].Data.statVal[a]); break;
                                //default:for (int a = 0; a < 2; a++) 
                            }
                            Items[slot].Ammt -= 1;
                            SendPacket p = new SendPacket();
                            p.PackArray(new byte[] { 23, 9 });
                            p.Pack8(slot);
                            p.Pack8(ammt);
                           host.Send(p);
                           p = new SendPacket();
                           p.PackArray(new byte[] { 23, 15 });
                           host.Send(p);
                            host.DataOut = SendType.Normal;
                        } break;
                }
            }
        }
        public void onItemCanceled(byte slot)
        {
            //Get item first
            if (Items[slot].ItemID > 0)
            {
                switch (Items[slot].Data.itemType)
                {
                    case eItemType.tent: host.Tent.Close(); break;
                }

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
}
