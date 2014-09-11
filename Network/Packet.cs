using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Network
{
    public class Packet
    {
        public List<byte> Data;
        public int m_nUnpackIndex;

        public Packet()
        {
            Data = new List<byte>();
        }

        #region Unpacking Values
        public bool? UnpackBoolean(object at = null)
        {
            int a =  m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                m_nUnpackIndex++;
                return BitConverter.ToBoolean(Data.ToArray(), a);               
            }
            return null;
        }
        public byte Unpack8(object at = null)
        {
            int a = m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                m_nUnpackIndex++;
                return Data[a];
            }
            return 0;
        }
        public UInt16 Unpack16(object at = null)
        {
            int a = m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                m_nUnpackIndex += 2;
                return BitConverter.ToUInt16(Data.ToArray(), a);
            }
            return 0;
        }
        public UInt32 Unpack32(object at = null)
        {
            int a = m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                m_nUnpackIndex += 4; return BitConverter.ToUInt32(Data.ToArray(), a);
            }
            return 0;
        }
        public UInt64 Unpack64(object at = null)
        {
            int a = m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                m_nUnpackIndex += 8;
                return BitConverter.ToUInt64(Data.ToArray(), a);
            }
            return 0;
        }
        public string UnpackChar(object at = null)
        {
            int nLength = 0; string strRet = "";

            m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                nLength = (int)Data[m_nUnpackIndex];
                m_nUnpackIndex++;
                for (int a = 0; a < nLength; a++)
                    strRet += (char)Data.Skip(m_nUnpackIndex).ToArray()[a];
                m_nUnpackIndex += (ushort)nLength;
                return strRet;
            }
            return null;
        }
        public string UnpackNChar(object at = null)
        {
            string strRet = "";
            m_nUnpackIndex = (at == null) ? m_nUnpackIndex : (int)at;
            if (m_nUnpackIndex < Data.Count)
            {
                for (int a = 0; a < Data.Skip(m_nUnpackIndex).Count(); a++)
                    strRet += (char)Data.Skip(m_nUnpackIndex).ToArray()[a];
                m_nUnpackIndex += (ushort)strRet.Length;
                return strRet;
            }
            return null;
        }
        #endregion

        #region Packing Values
        public virtual void PackArray(byte[] Bytes, int size = 0)
        {
            Data.AddRange(Bytes.Take((size == 0) ? Bytes.Length : size));
        }
        public virtual void PackBoolean(bool val)
        {
            Data.AddRange(BitConverter.GetBytes(val).Take(1));
        }
        public virtual void Pack8(byte val)
        {
            Data.Add(val);
        }
        public virtual void Pack16(UInt16 val)
        {
            Data.AddRange(BitConverter.GetBytes(val).Take(2));
        }
        public virtual void Pack32(UInt32 val)
        {
            Data.AddRange(BitConverter.GetBytes(val).Take(4));
        }
        public virtual void Pack64(UInt64 val)
        {
            Data.AddRange(BitConverter.GetBytes(val).Take(8));
        }
        public virtual void PackString(string str)
        {
            if (str == null) str = "";
            List<byte> tmp = new List<byte>();
            foreach (var r in str)
                tmp.Add(BitConverter.GetBytes(r)[0]);

            Pack8((byte)tmp.Count);
            Data.AddRange(tmp.ToArray());
        }
        public virtual void PackNString(string str)
        {
            if (str == null) str = "";
            List<byte> tmp = new List<byte>();
            foreach (var r in str)
                tmp.Add(BitConverter.GetBytes(r)[0]);

            Data.AddRange(tmp.ToArray());
        }
        #endregion

    }

    public class RecvPacket : Packet
    {
        public byte A { get { return Data[0]; } }
        public byte B { get { return (Size > 1) ? Data[1] : (byte)0; } }
        public int Size { get { return Data.Count; } }

        public RecvPacket()
        {
        }
    }

    public class SendPacket : Packet
    {
        bool header;
        private bool m_bDisconnectAfter;
        public SendPacket(bool bDisconnectAfter = false, bool noHeader = false)
        {
            header = !noHeader;
            if (!noHeader)
            {
                Data.AddRange(new byte[] { 244, 68, 0, 0 });
            }
            m_bDisconnectAfter = bDisconnectAfter;
        }
        public bool DisconnectAfter()
        {
            return m_bDisconnectAfter;
        }
        public override void Pack16(ushort val)
        {
            base.Pack16(val);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void Pack32(uint val)
        {
            base.Pack32(val);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void Pack64(ulong val)
        {
            base.Pack64(val);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void Pack8(byte val)
        {
            base.Pack8(val);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void PackArray(byte[] Bytes, int size = 0)
        {
            base.PackArray(Bytes, size);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void PackBoolean(bool val)
        {
            base.PackBoolean(val);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void PackNString(string str)
        {
            base.PackNString(str);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
        public override void PackString(string str)
        {
            base.PackString(str);
            if (header)
            {
                var sizer = BitConverter.GetBytes(Data.Count - 4).Take(2).ToArray();
                Data[2] = sizer[0];
                Data[3] = sizer[1];
            }
        }
    }
}
