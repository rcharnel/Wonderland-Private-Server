using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Game.Code;
using Network;
using DataFiles;
using RCLibrary.Core.Networking;

namespace Game
{
    public class Character : EquipManager
    {
        readonly object c_lock = new object();
        Action<SendPacket> Send;

        byte emote;
        Dictionary<string, string> m_colors; public Dictionary<string, string> Colors { get { lock (c_lock) return m_colors; } }

        UInt16 m_hairColor; public UInt16 HairColor { get { lock (c_lock)return m_hairColor; } set { lock (c_lock)m_hairColor = value; } }
        UInt16 m_skinColor; public UInt16 SkinColor { get { lock (c_lock)return m_skinColor; } set { lock (c_lock)m_skinColor = value; } }
        UInt16 m_clothingColor; public UInt16 ClothingColor { get { lock (c_lock)return m_clothingColor; } set { lock (c_lock)m_clothingColor = value; } }
        UInt16 m_eyeColor; public UInt16 EyeColor { get { lock (c_lock)return m_eyeColor; } set { lock (c_lock)m_eyeColor = value; } }
        UInt32 m_colorcode1; public UInt32 ColorCode1 { get { lock (c_lock)return m_colorcode1; } set { lock (c_lock)m_colorcode1 = value; } }
        UInt32 m_colorcode2; public UInt32 ColorCode2 { get { lock (c_lock)return m_colorcode2; } set { lock (c_lock)m_colorcode2 = value; } }
        UInt32 m_charID = 0; public UInt32 CharID { get { lock (c_lock)return m_charID; } set { lock (c_lock)m_charID = value; } }
        string m_name; public String CharName { get { lock (c_lock)return m_name; } set { lock (c_lock)m_name = value; } }
        string m_nickname; public String NickName { get { lock (c_lock)return m_nickname; } set { lock (c_lock)m_nickname = value; } }
        UInt16 m_x; public UInt16 CurX { get { lock (c_lock)return m_x; } set { lock (c_lock)m_x = value; } }
        UInt16 m_y; public UInt16 CurY { get { lock (c_lock)return m_y; } set { lock (c_lock)m_y = value; } }
        //Maps.IMap curMap;
        //public virtual Maps.IMap CurMap
        //{
        //    get { lock (c_lock) return curMap; }
        //    set
        //    {
        //        lock (c_lock)
        //        {
        //            curMap = value;
        //        }
        //    }
        //}
        ushort m_loginMap; public UInt16 LoginMap { get { lock (c_lock)return m_loginMap; } set { lock (c_lock)m_loginMap = value; } }
        byte m_slot; public byte Slot { get { lock (c_lock)return m_slot; } set { lock (c_lock)m_slot = value; } }

        public Character(Action<IPacket> src,PhxItemDat itemdat)
            : base(src,itemdat)
        {
            Send = src;
            m_colors = new Dictionary<string, string>();
            m_nickname = "";
            m_name = "";
        }
        public Character()
            : base(null,null)
        {
            m_colors = new Dictionary<string, string>();
        }

        public override void ProcessSocket(Player src, RCLibrary.Core.Networking.Packet p)
        {
            base.ProcessSocket(src, p);
        }

        public override void Clear()
        {
            base.Clear();
            m_colors.Clear();
        }

        public void Send_3_Me()
        {
            PacketBuilder p = new PacketBuilder();
            p.Begin();
            p.Add((byte)3);
            p.Add(CharID);
            p.Add((byte)Body);
            p.Add((ushort)LoginMap);
            p.Add((ushort)CurX);
            p.Add((ushort)CurY);
            p.Add((byte)0);
            p.Add((ushort)Head);
            p.Add((ushort)HairColor);
            p.Add((ushort)SkinColor);
            p.Add((ushort)ClothingColor);
            p.Add((ushort)EyeColor);
            p.Add((byte)WornCount);//clothesAmmt); // ammt of clothes
            p.Add(Worn_Equips);
            p.Add(0);
            p.Add(CharName);
            p.Add(NickName);
            p.Add(0);
            Send( new SendPacket(p.End(),p.End().Count()));
        }
        public void Send_5_3() //logging in player info
        {
            PacketBuilder p = new PacketBuilder();
            p.Begin();
            p.Add((byte)5);
            p.Add((byte)3);
            p.Add((byte)Element);
            p.Add(CurHP);
            p.Add((ushort)CurSP);
            p.Add(Str); //base str
            p.Add(Con); //base con
            p.Add(Int); //base int
            p.Add(Wis); //base wis
            p.Add(Agi); //base agi
            p.Add(Level); //lvl
            p.Add(TotalExp); //exp ???
            //p.Add(Level -1); //lvl -1
            p.Add(FullHP); //max hp
            p.Add((ushort)FullSP); //max sp
            //-------------- 7 DWords
            p.Add(417);
            p.Add(0);
            p.Add(0);
            p.Add(240);
            p.Add(0);
            p.Add(0);
            p.Add(0);

            //--------------- Skills
            p.Add((ushort)0/*(ushort)MySkills.Count*/);
            //if (MySkills.Count > 0)
            //    p.PackArray(MySkills.GetSkillData());
            //p.Pack16(1); //ammt of skills
            //p.Pack16(188); p.Pack16(1); p.Pack16(0); p.Pack((byte)0); //skill data
            //--------------- table with rebirth and job
            p.Add(0);
            p.Add(Reborn);
            p.Add((byte)Job);
            p.Add((byte)Potential);
            Send(new SendPacket(p.End(), p.End().Count()));
        }

        #region Inotify Property
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    public static class charExt
    {

        public static IEnumerable<Byte> ToArray(this Character src)
        {
            if (src == null) return null;
            PacketBuilder temp = new PacketBuilder();
            temp.Begin();
            temp.Add((byte)src.Slot);// data[at] = slot; at++;//PackSend->Pack((byte)1);
            temp.Add(src.CharName);// data[at] = nameLen; at++;
            temp.Add((byte)src.Level);// data[at] = level; at++;//	PackSend->Pack((byte)tmp1.level);					// Level 
            temp.Add((byte)src.Element);// data[at] = element; at++;//	PackSend->Pack((byte)3);  					// element
            temp.Add(src.FullHP);// putDWord(maxHP, data + at); at += 4;//	PackSend->Pack(tmp1.maxHP); 			// max hp
            temp.Add(src.CurHP);// putDWord(curHP, data + at); at += 4;//	PackSend->Pack(tmp1.curHP); 			// cur hp
            temp.Add(src.FullSP);// putDWord(maxSP, data + at); at += 4;//	PackSend->Pack(tmp1.maxSP); 			// max sp
            temp.Add(src.CurSP);// putDWord(curSP, data + at); at += 4;//	PackSend->Pack(tmp1.curSP); 			// cur sp
            temp.Add((uint)src.TotalExp);// putDWord(experience, data + at); at += 4;//	PackSend->Pack(tmp1.exp);			// exp
            temp.Add(src.Gold);// putDWord(gold, data + at); at += 4;//	PackSend->Pack(tmp1.gold); 			// gold
            temp.Add((ushort)src.Body);// data[at] = body; at++;//	PackSend->Pack((byte)tmp1.body); 					// body style
            temp.Add((ushort)src.Head);
            temp.Add((ushort)src.HairColor);// putDWord(color1, data + at); at += 4;//	PackSend->Pack(tmp1.colors1);
            temp.Add((ushort)src.SkinColor);
            temp.Add((ushort)src.ClothingColor);
            temp.Add((ushort)src.EyeColor);
            temp.Add(src.Reborn);
            temp.Add((byte)src.Job);// data[at] = rebirth; data[at + 1] = job; at += 2;//PackSend->Pack((byte)tmp1.rebirth);PackSend->Pack((byte)tmp1.rebirthJob); 				// rebirth flag, job skill

            for (byte a = 1; a < 7; a++)
                temp.Add((ushort)src[a].ItemID);

            return temp.End();

        }
        public static SendPacket ToAC3Packet(this Character src)
        {
            if (src == null) return null;
            PacketBuilder p = new PacketBuilder();
            p.Begin();
            p.Add((byte)3);
            p.Add(src.CharID);
            p.Add((byte)src.Body);
            p.Add((byte)src.Element);
            p.Add((byte)src.Level);
            //p.Add((ushort)src.CurMap.MapID);
            p.Add((ushort)src.CurX);
            p.Add((ushort)src.CurY);
            p.Add((byte)0);
            p.Add((ushort)src.Head);
            p.Add((ushort)src.HairColor);
            p.Add((ushort)src.SkinColor);
            p.Add((ushort)src.ClothingColor);
            p.Add((ushort)src.EyeColor);
            p.Add((byte)src.WornCount);//clothesAmmt); // ammt of clothes
            p.Add(src.Worn_Equips);
            p.Add(0);
            p.Add((byte)0);
            p.Add(src.Reborn);
            p.Add((byte)src.Job);
            p.Add(src.CharName);
            p.Add(src.NickName);
            p.Add(255);
            return new SendPacket(p.End(),p.End().Count());
        }
    }
}

