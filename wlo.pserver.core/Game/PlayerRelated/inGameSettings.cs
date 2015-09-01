using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCLibrary.Core.Networking;
using Network;

namespace Game.Code
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

    public class ClientSettings
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



        public ClientSettings()
        {
            chatChannel = ChannelCodeType.guildchannel |
                ChannelCodeType.localchannel |
                ChannelCodeType.teamchannel |
                ChannelCodeType.whisperchannel |
                ChannelCodeType.worldchannel;
        }

        public void ProcessSocket(RecievePacket p)
        {
            p.SetPtr();

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

        public byte[] ToArray()
        {
           RCLibrary.Core.Networking.PacketBuilder tmp = new RCLibrary.Core.Networking.PacketBuilder();
            tmp.Begin();
            tmp.Add(33);
            tmp.Add(2);
            tmp.Add((pk) ? 1 : 2);
            tmp.Add((fighting) ? 1 : 2);
            tmp.Add((trade) ? 1 : 2);
            tmp.Add((byte)chatChannel);
            return tmp.End();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", pk, fighting, (byte)chatChannel, trade);
        }

        public void Load(string str)
        {
            try
            {
                var word = str.Split(' ');
                pk = bool.Parse(word[0]);
                fighting = bool.Parse(word[1]);
                chatChannel = (ChannelCodeType)byte.Parse(word[2]);
                trade = bool.Parse(word[3]);
            }
            catch (Exception f) { DebugSystem.Write(new ExceptionData(f)); }

        }

    }
}
