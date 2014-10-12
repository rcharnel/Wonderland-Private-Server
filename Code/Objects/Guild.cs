using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Guild
    {
        public Dictionary<UInt16, CGuild> GlobalGuild;

        public Guild()
        {
            GlobalGuild = new Dictionary<UInt16, CGuild>();
        }        
        
        public void CreateGuild(ref Player src,string GuildName)
        {
            CGuild cg = new CGuild();
            cg.GuildName = GuildName;
            cg.TotalVice = 0;
            cg.GuildID = (ushort)(1000 + GlobalGuild.Count);
            cg.LeaderID = src.UserID;
            cg.LeaderName = src.CharacterName;
            cg.DateCreator = DateTime.Today; // data create guild need fix
            cg.IconGuil = 3402; //default insigna
            cg.Rules = "";

            if (GlobalGuild.ContainsKey(cg.GuildID)) return;
            else
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 26, 2 });
                s.Pack32(20000); //gold decrescimo
                src.Send(s);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 1, 0 }); // clean Tab guild
                src.Send(s);

                src.GuildID = cg.GuildID; // player get ID

                GlobalGuild.Add(cg.GuildID, cg); // add new guild here <--

                GlobalGuild[src.GuildID].AddMemberDataBase(ref src, true);
                GlobalGuild[src.GuildID].Send39_2(src.UserID);
                GlobalGuild[src.GuildID].Send39_17(src.UserID);
                GlobalGuild[src.GuildID].Send39_21_26_27(src.UserID);
                GlobalGuild[src.GuildID].GetMemberOnLinne(src.UserID);
                GlobalGuild[src.GuildID].GetMemberState(src.UserID);
                GlobalGuild[src.GuildID].GetGuilNickName(src.UserID);

                //unlock player dialogs if true;
                s = new SendPacket();
                s.PackArray(new byte[] { 20, 8 });
                src.Send(s);

            }
        }
        
    }


    public class CGuild
    {
        
        public UInt16 GuildID;        
        public string GuildName;
        public string Rules;
        public uint IconGuil;
        public uint LeaderID;
        public string LeaderName;
        public int TotalVice;
        public int TotalMembers { get { return (MembersDataBase.Count + 4) - TotalVice; } }
        public DateTime DateCreator;
        List<byte> ImgInsigne = new List<byte>();
        
        Dictionary<uint,Member> MembersDataBase = new Dictionary<uint, Member>();
        Dictionary<uint, Player> MembersOnlinne = new Dictionary<uint, Player>();

        public void ChangePermissionMember(uint actor, uint target,byte p1,byte p2, byte p3, byte p4, byte p5)
        {
            if (MembersDataBase.ContainsKey(target))
            {

                MembersDataBase[target].restriction1 = p1;
                MembersDataBase[target].restriction2 = p2;
                MembersDataBase[target].restriction3 = p3;
                MembersDataBase[target].restriction4 = p4;
                MembersDataBase[target].restriction5 = p5;

                SendPacket s = new SendPacket();
                s.PackArray(new byte[]{39, 24});
                s.Pack32(target);
                s.Pack8(1);
                s.Pack8(1);
                s.Pack8(p1);
                cGlobal.WLO_World.BroadcastTo(s, directTo: actor);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 24 });
                s.Pack32(target);
                s.Pack8(1);
                s.Pack8(2);
                s.Pack8(p2);
                cGlobal.WLO_World.BroadcastTo(s, directTo: actor);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 24 });
                s.Pack32(target);
                s.Pack8(1);
                s.Pack8(3);
                s.Pack8(p3);
                cGlobal.WLO_World.BroadcastTo(s, directTo: actor);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 24 });
                s.Pack32(target);
                s.Pack8(1);
                s.Pack8(4);
                s.Pack8(p4);
                cGlobal.WLO_World.BroadcastTo(s, directTo: actor);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 24 });
                s.Pack32(target);
                s.Pack8(1);
                s.Pack8(5);
                s.Pack8(p5);
                cGlobal.WLO_World.BroadcastTo(s, directTo: actor);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
                cGlobal.WLO_World.BroadcastTo(s, directTo: target);
            }

        }
        public void HoldThePostOfViceOrgleader(uint target, byte type)
        {
            if (MembersDataBase.ContainsKey(target))
            {
                switch (type)
                {
                    case 0: if (TotalVice > 0) TotalVice--; break;
                    case 1: if (TotalVice < 5) TotalVice++; break;

                }

                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39,12});
                s.Pack32(target);
                BroadCastGuild(s, 0); // all receiv
            }
        }
        public void AddNewMemberGuild(ref Player src)
        {
            if (MembersDataBase.ContainsKey(src.UserID)) return;
            else
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {39,4 });
                s.Pack32(src.UserID);
                s.Pack8(1);
                src.Send(s);
                AddMemberDataBase(ref src, false);
                Send39_2(src.UserID);
                Send39_21_26_27(src.UserID);
                GetMemberOnLinne(src.UserID);
                GetMemberState(src.UserID);

                s = new SendPacket();
                s.PackArray(new byte[] {39,62,1});                
                src.Send(s);

                GetGuilNickName(src.UserID);
                GetInsigneGuild(src.UserID);

                s = new SendPacket();
                s.PackArray(new byte[] {39,61});
                s.Pack32(src.UserID);
                s.Pack8(0);
                src.Send(s);

                s = new SendPacket();
                s.PackArray(new byte[] { 39, 4 });
                s.Pack32(src.UserID);
                s.Pack8(1);
                src.Send(s);

                Send39_60(ref src);

                s = new SendPacket();
                s.PackArray(new byte[] {24,5,1,1,0});                
                src.Send(s);
            }
        }
        
        public void AddMemberDataBase(ref Player src,bool Master)
        {
            if (MembersDataBase.ContainsKey(src.UserID)) return;            
            else
            {
                Member mw = new Member();
                mw.MemberName = src.CharacterName;
                mw.MemberID = src.UserID;
                mw.NickName = src.Nickname;
                mw.Level = src.Level;
                mw.Job = (byte)src.Job;
                mw.reborn = Convert.ToByte(src.Reborn);
                mw.body = (byte)src.Body;
                mw.head = src.Head;
                mw.color1 = src.HairColor;
                mw.color2 = src.SkinColor;
                mw.color3 = src.ClothingColor;
                mw.color4 = src.EyeColor;
                #region Restriciton member
                if (Master)
                {
                    mw.restriction1 = 1;
                    mw.restriction2 = 1;// Invite Join
                    mw.restriction3 = 1;
                    mw.restriction4 = 1;
                    mw.restriction5 = 1;// Modify emblem

                }
                else
                {  
                    mw.restriction1 = 0;
                    mw.restriction2 = 0;// Invite Join
                    mw.restriction3 = 0;
                    mw.restriction4 = 0;
                    mw.restriction5 = 0;// Modify emblem

                }
                #endregion

                MembersDataBase.Add(src.UserID, mw);
                MembersOnlinne.Add(src.UserID, src);
            }
        }
       
        public void Send39_2(uint src) // here send database member ON and OFF
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 2 });
            s.PackString(GuildName);//guild name
            s.Pack8(0); // total leaders 
            s.Pack8((byte)TotalMembers);// total player in guild +4 - number leaders
            #region LoadPLayers
            foreach (var pair in MembersDataBase)
            {
                s.Pack32(pair.Value.MemberID);
                s.PackString(pair.Value.MemberName);
                s.Pack8(pair.Value.Level);
                s.Pack8(pair.Value.Job);
                s.Pack8(1);//(pair.Value.elemement);
                s.Pack8(pair.Value.Level);
                s.Pack8(pair.Value.body);
                s.Pack8(pair.Value.head);
                s.Pack16(pair.Value.color1);
                s.Pack16(pair.Value.color2);
                s.Pack16(pair.Value.color3);
                s.Pack16(pair.Value.color4);
                s.PackString(pair.Value.NickName);
                s.Pack32(0);
                s.Pack8(pair.Value.restriction1);
                s.Pack8(pair.Value.restriction2);
                s.Pack8(pair.Value.restriction3);
                s.Pack8(pair.Value.restriction4);
                s.Pack8(pair.Value.restriction5);
                s.Pack32(0);
                s.Pack8(0);
            }
            #endregion
            s.PackString(Rules); // rules! defalt = 0
            s.PackArray(new byte[] {000, 000, 000, 000, 000, 000, 000, 000, 157,
                115, 241, 148, 144, 109, 228, 064, 000 }); // 17 need fix here.data timer creator:??
            MembersOnlinne[src].Send(s);
        }
        public void Send39_17(uint src)
        {
            if (MembersOnlinne.ContainsKey(src))
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 17 });                
                MembersOnlinne[src].Send(s);
            }

        }
        
        public void Send39_21_26_27(uint src)
        {
            if (MembersOnlinne.ContainsKey(src))
            {
                #region Packet 39,21  39,26 39, 27

                for (int a = 1; a < 8; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 21 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    MembersOnlinne[src].Send(s);
                }
                for (int a = 1; a < 5; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 26 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    MembersOnlinne[src].Send(s);
                }
                for (int a = 1; a < 8; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 27 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    MembersOnlinne[src].Send(s);
                }

                #endregion

            }
        }     
  
        public void GetMemberOnLinne(uint src)
        {
            foreach (var pair in MembersOnlinne)
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 60 });
                s.Pack32(pair.Value.UserID);
                s.PackString(pair.Value.CharacterName);
                s.Pack8(pair.Value.Level);
                s.Pack8((byte)pair.Value.Job);// job

                if (pair.Value.Reborn) s.Pack8(1);//reborn
                else s.Pack8(0);

                s.Pack8((byte)pair.Value.Element);//elemnt
                s.Pack8((byte)pair.Value.Body);// body
                s.Pack8((byte)pair.Value.Head);// head
                s.Pack16(pair.Value.HairColor);
                s.Pack16(pair.Value.SkinColor);
                s.Pack16(pair.Value.ClothingColor);
                s.Pack16(pair.Value.EyeColor);
                s.PackString(pair.Value.Nickname);
                MembersOnlinne[src].Send(s);
            }
        }
        //39,61 este pacote ele é foda precisa estudar mais um pouco ele manda alocado.
        public void GetMemberState(uint src)
        {
            foreach (var pair in MembersOnlinne)
            {               
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 61 });
                s.Pack32(pair.Value.UserID);
                s.Pack8(Convert.ToByte(pair.Value.Busy));// 0 = onlinne 1 = busy
                MembersOnlinne[src].Send(s);              
            }
        }
        void GetInsigneGuild(uint src)
        {            
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 30,2 });
                s.PackArray(ImgInsigne.ToArray());
                MembersOnlinne[src].Send(s);            
        }
        public void GetGuilNickName(uint src)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 9 });
            s.Pack32(src);
            s.Pack32(IconGuil); // UINT ICON GUILD
            s.PackString(GuildName); // name guild NICK SHOW IN MAP (acima do nick name)
            MembersOnlinne[src].Send(s);
            //player in current map too receiv this packet
            //cglobal.world.current map . send(s);/// need testes.
        }
        void Send39_60(ref Player src)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 60 });
            s.Pack32(src.UserID);
            s.PackString(src.CharacterName);
            s.Pack8(src.Level);
            s.Pack8((byte)src.Job);// job

            if (src.Reborn) s.Pack8(1);//reborn
            else s.Pack8(0);

            s.Pack8((byte)src.Element);//elemnt
            s.Pack8((byte)src.Body);// body
            s.Pack8((byte)src.Head);// head
            s.Pack16(src.HairColor);
            s.Pack16(src.SkinColor);
            s.Pack16(src.ClothingColor);
            s.Pack16(src.EyeColor);
            s.PackString(src.Nickname);

            BroadCastGuild(s, src.UserID);

        }

        
        void BroadCastGuild(SendPacket spk,uint exceptID)
        {            
                foreach (var pair in MembersOnlinne)
                {
                    if (pair.Value.UserID != exceptID)
                    {
                        pair.Value.Send(spk);
                    }
                }            
        }
        //void UpdateMemberInfoDataBase(ref Player src)
        //{
        //    uint user = src.UserID;
        //    if(MembersDataBase.ContainsKey(user))
        //    {
        //        MembersDataBase[user].NickName = src.Nickname;
        //        MembersDataBase[user].reborn = Convert.ToByte(src.Reborn);
        //        MembersDataBase[user].Job = (byte)src.Job;
        //        MembersDataBase[user].color1 = src.HairColor;
        //        MembersDataBase[user].color2 = src.SkinColor;
        //        MembersDataBase[user].color3 = src.ClothingColor;
        //        MembersDataBase[user].color4 = src.EyeColor;

        //    }
        //}
        
        
        //void SendMeToMembersGuild(uint src)
        //{
        //    foreach (var pair in MembersOnlinne)
        //    {
        //        if (pair.Value.UserID != src)
        //        {
        //            SendPacket s = new SendPacket();
        //            s.PackArray(new byte[] { 39, 60 });
        //            s.Pack32(pair.Value.UserID);
        //            s.PackString(pair.Value.CharacterName);
        //            s.Pack8(pair.Value.Level);
        //            s.Pack8((byte)pair.Value.Job);// job

        //            if (pair.Value.Reborn) s.Pack8(1);//reborn
        //            else s.Pack8(0);

        //            s.Pack8((byte)pair.Value.Element);//elemnt
        //            s.Pack8((byte)pair.Value.Body);// body
        //            s.Pack8((byte)pair.Value.Head);// head
        //            s.Pack16(pair.Value.HairColor);
        //            s.Pack16(pair.Value.SkinColor);
        //            s.Pack16(pair.Value.ClothingColor);
        //            s.Pack16(pair.Value.EyeColor);
        //            s.PackString(pair.Value.Nickname);
        //            MembersOnlinne[src].Send(s);
        //        }
        //    }

        //}

        public void ChangInsigneGuild(RecvPacket r)
        {
            ImgInsigne.AddRange(r.Data.Skip(2).Take(r.Data.Count - 2).ToArray());

            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 30, 1 });
            s.PackArray(ImgInsigne.ToArray());
            BroadCastGuild(s, 0);// all
        }

        public void Edit_Rule(string text)
        {
            Rules = text;
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 11 });
            s.PackNString(text);
            BroadCastGuild(s, 0);// all
        }
        public void Dismiss(uint target, uint actor)
        {            
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39,6});
            s.Pack32(target);
            BroadCastGuild(s, target);// all except the Demiss

            s = new SendPacket();
            s.PackArray(new byte[] { 39,7, 0 });
            s.Pack32(target);
            cGlobal.WLO_World.BroadcastTo(s, directTo: actor); // actor 
            cGlobal.WLO_World.BroadcastTo(s, directTo: target); // demiss

            RemoveNickNameGuild(target);
            RemoveMember(target);
        }
        public void LeaveGuild(uint src)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] {39,7,0 });
            s.Pack32(src);

            s = new SendPacket();
            s.PackArray(new byte[] { 39, 6 });
            s.Pack32(src);
            BroadCastGuild(s, src);// all except id

            MembersOnlinne[src].Send(s);
            RemoveNickNameGuild(src);
            RemoveMember(src);
        }

        void RemoveNickNameGuild(uint src)
        {
            // CLEAN GUILD NICK NAME
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 9 });
            s.Pack32(src);
            s.PackArray(new byte[] { 0, 0, 0, 0, 0 });
            cGlobal.WLO_World.BroadcastTo(s, directTo: src);
        }
        void RemoveMember(uint src)
        {
            MembersDataBase.Remove(src);
            MembersOnlinne[src].GuildID = 0; // clean
            MembersOnlinne.Remove(src);

        }
    }

    public class Member
    {
        public uint MemberID;
        public string MemberName;
        public byte Level;
        public byte Job;
        public byte reborn;
        public byte elemement;
        public byte body;
        public byte head;
        public UInt16 color1;
        public UInt16 color2;
        public UInt16 color3;
        public UInt16 color4;
        public string NickName;
        //public byte Leador;
        public byte restriction1;
        public byte restriction2;
        public byte restriction3;
        public byte restriction4;
        public byte restriction5;

    }
}
