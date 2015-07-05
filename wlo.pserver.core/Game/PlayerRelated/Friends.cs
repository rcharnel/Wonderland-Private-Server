using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;

using Server.Bots;

namespace Game.Code.PlayerRelated
{
    public class Friendlist
    {
        Character[] m_friendlist;

        public Friendlist(Action<SendPacket> src)
        {
            SendPacket = src;
            m_friendlist = new Character[50];
            m_friendlist[0] = new Cupid();
        }
        
        #region Events/Funcs/Actions
        Action<SendPacket> SendPacket;
        #endregion
        
        public void ProcessSocket(SendPacket p)
        {
            p.m_nUnpackIndex = 4;

            var a = p.Unpack8();
            var b = p.Unpack8();

            if (a != 33) return;

            switch (a)
            {
               
            }
        }

        public void SendFriendList()
        {
            SendPacket y = new SendPacket();
            y.Pack((byte)14);
            y.Pack((byte)5);

            foreach (Character h in m_friendlist)
            {
                if (h == null) continue;
                y.Pack(h.CharID);
                y.Pack(h.CharName);
                y.Pack((byte)h.Level);
                y.Pack(h.Reborn);
                y.Pack((byte)h.Job);
                y.Pack((byte)h.Element);
                y.Pack((byte)h.Body);
                y.Pack(h.Head);
                y.Pack(h.HairColor);
                y.Pack(h.SkinColor);
                y.Pack(h.ClothingColor);
                y.Pack(h.EyeColor);
                y.Pack(h.NickName);
                y.Pack((byte)0);
            }
            y.SetHeader();
            SendPacket(y);
        }
        public void AddFriend(Player t)
        {
            if (m_friendlist.Count(c => c != null) == 50) return;
            for (int a = 0; a < 50;a++ )
                if(m_friendlist[a] == null)
                    m_friendlist[a] = (Character)t;

            SendPacket s = new SendPacket();
            s.Pack((byte)14);
            s.Pack((byte)9);
            s.Pack(t.CharID);
            s.Pack((byte)0);
            s.SetHeader();
            SendPacket(s);
            s = new SendPacket();
            s.Pack((byte)14);
            s.Pack((byte)7);
            s.Pack(t.CharID);
            s.PackString("Test");
            s.SetHeader();
            SendPacket(s);
        }
        public void DelFriend(uint t)
        {
            for (int a = 0; a < 50; a++)
                if (m_friendlist[a] != null && m_friendlist[a].CharID == t)
                {
                    m_friendlist[a] = null;
                    SendPacket s = new SendPacket();
                    s.Pack((byte)14);
                    s.Pack((byte)4);
                    s.Pack(t);
                    s.SetHeader();
                    SendPacket(s);
                }
        }
        public bool LoadFriends(string str)
        {
            var frilist = str.Split('&');
            for(int a = 0;a<50;a++)
            {
                if (frilist[a].Length > 0)
                {
                    m_friendlist[a] = new Character();
                    m_friendlist[a].CharID = uint.Parse(frilist[a]);

                    //m_friends.Add(cGlobal.gCharacterDataBase.GetCharacterData(uint.Parse(f[0])));
                }
            }
            return true;
        }
        public string GetFriends_Flag
        {
            get
            {
                string query = "";
                for (int a = 0; a < m_friendlist.Length; a++)
                {
                    if (m_friendlist[a] != null)
                    {
                        query += m_friendlist[a].CharID.ToString();
                        if (a < m_friendlist.Length)
                            query += "&";
                    }
                }
                if (query == "")
                    query += "none";
                return query;
            }
        }
    }
}
