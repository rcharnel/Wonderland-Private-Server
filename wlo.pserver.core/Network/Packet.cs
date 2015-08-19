using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCLibrary.Core.Networking;

namespace Network
{

    public class SendPacket : Packet
    {
        /// <summary>
        /// Create a Packet from array
        /// !!Does not add header!!
        /// </summary>
        /// <param name="init"> the data</param>
        /// <param name="nLength">copy a specific length of the data array </param>
        public SendPacket(byte[] init, int initLength = -1)
            : base(init, initLength)
        {

        }
        /// <summary>
        /// Create a Packet from array
        /// !!Does not add header!!
        /// </summary>
        /// <param name="init"> the data</param>
        /// <param name="nLength">copy a specific length of the data array </param>
        public SendPacket(IPacket src):base(src.Buffer.ToArray())
        {
        }
        /// <summary>
        /// Initialize a new Packet with a header
        /// </summary>
        /// <param name="header"> the header to identify the packet</param>
        public SendPacket(ushort header = 0x44F4)
        {
            m_buffer = new byte[0];
            CopyToBuffer(BitConverter.GetBytes(header), 2);
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

        public void SetPtr(int ptr = 4)
        {
            base.SetPtr(ptr);
        }
    }

}
