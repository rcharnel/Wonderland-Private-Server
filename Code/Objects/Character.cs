using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Maps;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Character : EquipementManager
    {
        private readonly object mylock;
        byte emote;
        UInt16 m_hairColor;
        UInt16 m_skinColor;
        UInt16 m_clothingColor;
        UInt16 m_eyeColor;
        UInt32 m_charID = 0;
        public string m_name;
        string m_nickname;
        UInt16 x;
        UInt16 y;
        Map currentMap;
        ushort loginMap; // Used to designate the Map to Login too
        protected List<Mail> mailBox;

        public Character()
        {
            mylock = new object();
        }

        public virtual void Clear()
        {
            mailBox = new List<Mail>();
            Nickname = "";
        }

        public virtual uint ID
        {
            get
            {
                return m_charID;
            }
            set
            {
                m_charID = value;
            }
        }
        public virtual string CharacterName
        {
            get { return m_name; }
            set { m_name = value; }
        }
        public virtual string Nickname
        {
            get { return m_nickname; }
            set { m_nickname = value; }
        }
        public virtual UInt16 HairColor
        {
            get { return m_hairColor; }
            set { m_hairColor = value; }
        }
        public virtual UInt16 SkinColor
        {
            get { return m_skinColor; }
            set { m_skinColor = value; }
        }
        public virtual UInt16 ClothingColor
        {
            get { return m_clothingColor; }
            set { m_clothingColor = value; }
        }
        public virtual UInt16 EyeColor
        {
            get { return m_eyeColor; }
            set { m_eyeColor = value; }
        }
        public virtual byte Emote
        {
            get { return emote; }
            set { emote = value; }
        }
        public virtual Map CurrentMap
        {
            get { return currentMap; }
            set { currentMap = value; }
        }
        public virtual UInt16 X
        {
            get { return x; }
            set { x = value; }
        }
        public virtual UInt16 Y
        {
            get { return y; }
            set { y = value; }
        }
        public virtual UInt16 LoginMap
        {
            get { return loginMap; }
            set { loginMap = value; }
        }


        #region Mail
        public virtual void RecvMailfrom(Player t, string msg, double Date = 0)
        {
            Mail a = new Mail();
            a.message = msg;
            a.id = m_charID;
            a.targetid = t.ID;
            a.when = (Date == 0) ? DateTime.Now.ToOADate() : Date;
            a.type = "Recv";
            mailBox.Add(a);
        }
        #endregion

    }
}

