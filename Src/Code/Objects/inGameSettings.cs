using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Code.Objects
{
    public class inGameSettings
    {
        bool pk = false;
        bool fighting = true;
        bool trade = true;
        public bool lc = true;
        public bool wc = true;
        public bool gc = true;
        public bool whc = true;
        public bool tc = true;

        public inGameSettings()
        {
        }

        public bool PKABLE { get { return pk; } }
        public bool JOINABLE { get { return fighting; } }
        public bool TRADABLE { get { return trade; } }
        public byte PkSetting { get { if (pk)return 1; else return 2; } }
        public byte ViewSetting { get { if (fighting)return 1; else return 2; } }
        public byte TradeSetting { get { if (trade)return 1; else return 2; } }

        public byte[] Data
        {
            get
            {
                SendPacket p = new SendPacket(false,true);
                p.Pack(PkSetting);
                p.Pack(ViewSetting);
                p.Pack(TradeSetting);
                p.Pack((byte)(GetChannelCode() + 96));
                return p.Data.ToArray();
            }
        }

        void SetChannel(byte code)
        {
            switch (code)
            {
                case 0: { lc = false; whc = false; gc = false; wc = false; tc = false; } break;
                case 1: lc = true; break;
                case 2: whc = true; break;
                case 3: { lc = true; whc = true; } break;
                case 4: tc = true; break;
                case 5: { lc = true; tc = true; } break;
                case 6: { tc = true; whc = true; } break;
                case 7: { lc = true; whc = true; tc = true; } break;
                case 8: gc = true; break;
                case 9: { lc = true; gc = true; } break;
                case 10: { whc = true; gc = true; } break;
                case 11: { lc = true; whc = true; gc = true; } break;
                case 12: { tc = true; gc = true; } break;
                case 13: { lc = true; gc = true; tc = true; } break;
                case 14: { whc = true; gc = true; tc = true; } break;
                case 15: { lc = true; whc = true; tc = true; gc = true; } break;
                case 16: wc = true; break;
                case 17: { wc = true; lc = true; } break;
                case 18: { wc = true; whc = true; } break;
                case 19: { wc = true; whc = true; lc = true; } break;
                case 20: { wc = true; tc = true; } break;
                case 21: { wc = true; tc = true; lc = true; } break;
                case 22: { wc = true; whc = true; tc = true; } break;
                case 23: { wc = true; whc = true; tc = true; lc = true; } break;
                case 24: { wc = true; gc = true; } break;
                case 25: { wc = true; lc = true; gc = true; } break;
                case 26: { wc = true; whc = true; gc = true; } break;
                case 27: { wc = true; whc = true; lc = true; gc = true; } break;
                case 28: { wc = true; tc = true; gc = true; } break;
                case 29: { wc = true; lc = true; tc = true; gc = true; } break;
                case 30: { wc = true; whc = true; tc = true; gc = true; } break;
                case 31: { lc = true; whc = true; gc = true; wc = true; tc = true; } break;
            }
        }

        public byte GetChannelCode()
        {
            int ammt = 0;
            if (lc) ammt += 1;
            if (whc) ammt += 2;
            if (tc) ammt += 4;
            if (gc) ammt += 8;
            if (wc) ammt += 16;
            return (byte)ammt;
        }

        public string Get_SysytemFlag()
        {
            return string.Format("{0} {1} {2} {3}", pk.ToString(), fighting.ToString(), GetChannelCode(),trade.ToString());
        }

        public void Load_SystemFlag(string str)
        {
            var word = str.Split(' ');
            bool.TryParse(word[0],out pk);
            bool.TryParse(word[1],out fighting);
            Set(3, byte.Parse(word[2]));
            bool.TryParse(word[3],out trade);
        }

        public void Set(byte type, byte val)
        {
            bool set;
            if (val == 1) set = true; else set = false;
            switch (type)
            {
                case 1: pk = set; break;
                case 2: fighting = set; break;
                case 3: SetChannel(val); break;
                case 4: trade = set; break;
            }
        }
    }
}
