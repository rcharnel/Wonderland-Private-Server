using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Game.Code.PlayerRelated
{
    [Flags]
    public enum ChannelCodeType
    {
        None = 0,
        localchannel = 1,
        whisperchannel = 2,
        teamchannel = 4,
        guildchannel = 8,
        worldchannel = 16
    }

    public class clientSettings
    {
        readonly object mlock = new object();

        bool pk = false;
        bool fighting = true;
        bool trade = true;

        ChannelCodeType chatChannel;
        public ChannelCodeType ChannelCode
        {
            get { lock (mlock)return chatChannel; }
            set { lock (mlock)chatChannel = value; }
        }
        public bool PKABLE { get { lock (mlock)return pk; } set { lock (mlock)pk = value; } }
        public bool JOINABLE { get { lock (mlock)return fighting; } set { lock (mlock)fighting = value; } }
        public bool TRADABLE { get { lock (mlock) return trade; } set { lock (mlock)trade = value; } }



        public clientSettings()
        {
            chatChannel = ChannelCodeType.guildchannel |
                ChannelCodeType.localchannel |
                ChannelCodeType.teamchannel |
                ChannelCodeType.whisperchannel |
                ChannelCodeType.worldchannel;
        }

        public void ProcessSocket(SendPacket p)
        {
            p.m_nUnpackIndex = 4;

            var a = p.Unpack8();
            var b = p.Unpack8();

            if (a != 33) return;

            switch (a)
            {
                #region AC 33
                case 33:
                    {
                        switch (b)
                        {
                            case 1:
                                {
                                    switch (p.Unpack8())
                                    {
                                        case 1: PKABLE = p.UnpackBool(); break;
                                        case 2: JOINABLE = p.UnpackBool(); break;
                                        case 3: ChannelCode = (ChannelCodeType)p.Unpack8(); break;
                                        case 4: TRADABLE = p.UnpackBool(); break;
                                    }
                                } break;
                        }
                    } break;
                #endregion
            }
        }

        public IEnumerable<byte> ToArray()
        {
           RCLibrary.Core.Networking.PacketBuilder tmp = new RCLibrary.Core.Networking.PacketBuilder();
            tmp.Begin(false);
            tmp.Add(33);
            tmp.Add(2);
            tmp.Add((pk) ? 1 : 2);
            tmp.Add((fighting) ? 1 : 2);
            tmp.Add((trade) ? 1 : 2);
            tmp.Add((byte)chatChannel);
            return tmp.End().Buffer;
        }
    }
}
