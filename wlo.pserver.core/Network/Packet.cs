using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCLibrary.Core.Networking;

namespace Network
{
    public class SendPacket : Packet
    {

        public SendPacket(byte[] init, int initLength = -1)
            : base(init, initLength)
        {
        }
        public SendPacket(IPacket src):base(src.Buffer.ToArray())
        {
        }

        public SendPacket()
        {
            m_buffer = new byte[0];
            CopyToBuffer(BitConverter.GetBytes(0x44F4), 2);
            CopyToBuffer(new byte[2], 2);
        }

        public override void Pack16(ushort nVal)
        {
            base.Pack16(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer,2);
        }
        public override void Pack32(uint nVal)
        {
            base.Pack32(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void Pack64(ulong nVal)
        {
            base.Pack64(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void Pack8(byte nVal)
        {
            base.Pack8(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackArray(byte[] nVal)
        {
            base.PackArray(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackArray(IEnumerable<byte> nVal)
        {
            base.PackArray(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackBool(bool val)
        {
            base.PackBool(val);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackStringN(string strVal)
        {
            base.PackStringN(strVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackDouble(double nVal)
        {
            base.PackDouble(nVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }
        public override void PackString(string strVal)
        {
            base.PackString(strVal);
            BitConverter.GetBytes((UInt16)(m_buffer.Length - 4)).CopyTo(m_buffer, 2);
        }

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

    }

    public class RecievePacket : Packet
    {

         public RecievePacket(byte[] init, int initLength = -1)
            : base(init, initLength)
        {
            SetPtr();  
        }
        public RecievePacket(IPacket src):base(src.Buffer.ToArray())
        {
            SetPtr();  
        }

        public RecievePacket()
        {
        }


        public byte A { get { return this[4]; } }
        public byte? B { get { if (this.Buffer.Count() > 5) return this[5]; else return null; } }

        public override void SetPtr(int ptr = 4)
        {
            base.SetPtr(ptr);
        }
    }

}
