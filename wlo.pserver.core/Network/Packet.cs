using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCLibrary.Core.Networking;

namespace Network
{
    public class SendPacket : Packet
    {

        public SendPacket(bool header = true)
            : base(header)
        {
        }
        public SendPacket(byte[] init, int initLength = -1)
            : base(init, initLength)
        {
        }
        public SendPacket(Packet src):base(src.Buffer.ToArray())
        {
           
        }

        public SendPacket()
        {
        }

    }

    

}
