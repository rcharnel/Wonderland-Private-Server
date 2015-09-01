using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace System
{
    public static class Tools
    {

        /// <summary>
        /// Made by Kodie (Sharky) edited 12/30/14
        /// </summary>
        /// <param name="packetFormat"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static SendPacket FromFormat(string packetFormat, params object[] list)
        {
            IEnumerable<byte> ret = new byte[0];
            List<byte> tmp;

            int idx = 0, len = 0;
            foreach (char ch in packetFormat)
            {
                switch (ch)
                {
                    case 'D':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt32(list[idx])));
                        len += 4;
                        break;
                    case 'd':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt32(list[idx])));
                        len += 4;
                        break;
                    case 'W':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt16(list[idx])));
                        len += 2;
                        break;
                    case 'w':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt16(list[idx])));
                        len += 2;
                        break;
                    case 'B':
                        tmp = ret.ToList();
                        tmp.Add(Convert.ToByte(list[idx]));
                        ret = tmp;
                        ++len;
                        break;
                    case 'b':
                        tmp = ret.ToList();
                        tmp.Add(Convert.ToByte(list[idx]));
                        ret = tmp;
                        ++len;
                        break;
                    case 'L':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt64(list[idx])));
                        len += 8;
                        break;
                    case 'l':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt64(list[idx])));
                        len += 8;
                        break;
                    case 's':
                    case 'S':
                        var str = Convert.ToString(list[idx]);
                        tmp = ret.ToList();
                        tmp.Add((byte)str.Length);
                        tmp.AddRange(ASCIIEncoding.ASCII.GetBytes(str));
                        ret = tmp;
                        len += str.Length + 1;
                        break;
                }
                ++idx;
            }
            SendPacket tmp3 = new SendPacket();
            tmp3.PackArray(ret.ToArray());
            return tmp3;
        }
        public static byte[] FromFormatToArray(string packetFormat, params object[] list)
        {
            IEnumerable<byte> ret = new byte[0];
            List<byte> tmp;

            int idx = 0, len = 0;
            foreach (char ch in packetFormat)
            {
                switch (ch)
                {
                    case 'D':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt32(list[idx])));
                        len += 4;
                        break;
                    case 'd':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt32(list[idx])));
                        len += 4;
                        break;
                    case 'W':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt16(list[idx])));
                        len += 2;
                        break;
                    case 'w':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt16(list[idx])));
                        len += 2;
                        break;
                    case 'B':
                        tmp = ret.ToList();
                        tmp.Add(Convert.ToByte(list[idx]));
                        ret = tmp;
                        ++len;
                        break;
                    case 'b':
                        tmp = ret.ToList();
                        tmp.Add(Convert.ToByte(list[idx]));
                        ret = tmp;
                        ++len;
                        break;
                    case 'L':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToUInt64(list[idx])));
                        len += 8;
                        break;
                    case 'l':
                        ret = ret.Concat(BitConverter.GetBytes(Convert.ToInt64(list[idx])));
                        len += 8;
                        break;
                    case 's':
                    case 'S':
                        var str = Convert.ToString(list[idx]);
                        tmp = ret.ToList();
                        tmp.Add((byte)str.Length);
                        tmp.AddRange(ASCIIEncoding.ASCII.GetBytes(str));
                        ret = tmp;
                        len += str.Length + 1;
                        break;
                }
                ++idx;
            }
            return ret.ToArray();
        }


        public class PacketBuilder
        {
             List<byte> tmp;
             ushort? header;

            public void Begin(ushort? header = 0x44f4)
            {
                this.header = header;
                tmp = new List<byte>();
            }

            public void Add(object src)
            {
                if (src is byte)
                    tmp.AddRange(BitConverter.GetBytes((byte)src).Take(1).ToArray());
                else if (src is ushort)
                    tmp.AddRange(BitConverter.GetBytes((ushort)src).Take(2).ToArray());
                else if (src is uint)
                    tmp.AddRange(BitConverter.GetBytes((uint)src).Take(4).ToArray());
                else if (src is UInt64)
                    tmp.AddRange(BitConverter.GetBytes((UInt64)src).Take(8).ToArray());
                else if (src is Int64)
                    tmp.AddRange(BitConverter.GetBytes((Int64)src).Take(8).ToArray());
                else if (src is int)
                    tmp.AddRange(BitConverter.GetBytes((int)src).Take(4).ToArray());
                else if (src is short)
                    tmp.AddRange(BitConverter.GetBytes((short)src).Take(2).ToArray());
                else if (src is bool)
                    tmp.AddRange(BitConverter.GetBytes((bool)src).Take(1).ToArray());
            }
            public void Add(string obj, bool nullstring = false)
            {
                if (obj == null) return;
                if (!nullstring)
                    tmp.Add((byte)obj.Length);
                tmp.AddRange(ASCIIEncoding.ASCII.GetBytes(obj));
            }
            public void Add(byte[] obj)
            {
                tmp.AddRange(obj);
            }
            public void Add(IEnumerable<byte> obj)
            {
                tmp.AddRange(obj.ToArray());
            }

            public byte[] End()
            {
                try
                {
                    if (header.HasValue)
                    {
                        tmp.InsertRange(0, BitConverter.GetBytes((ushort)tmp.Count));
                        tmp.InsertRange(0, BitConverter.GetBytes(header.Value));
                        return tmp.ToArray();
                    }
                    else
                        return tmp.ToArray();
                }
                finally
                {
                    tmp = null;
                }
            }
        }

    }
}
