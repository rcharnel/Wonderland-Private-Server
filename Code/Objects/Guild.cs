using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Interface;

namespace Wonderland_Private_Server.Code.Objects
{
    /// <summary>
    /// Manages all Guilds in game
    /// </summary>
    public class GuildSystem
    {
        readonly object mylock = new object();
        Dictionary<UInt16, Guild> GlobalGuild;

        ushort AvailableGuildID { get { ushort a = 1000; while (GlobalGuild.ContainsKey(a)) { a++; } return a; } }
        public GuildSystem()
        {
            GlobalGuild = new Dictionary<UInt16, Guild>();
        }
        public bool CreateNewGuild(ref Player src, string GuildName)
        {
            lock (mylock)
            {
                //Prechecks
                //if name is the same,if player is in a guild, is required lvl, has the funds

                if (GlobalGuild.Values.Count(c => c.GuildName == GuildName) > 0)//name exists
                {
                    return false;
                }
                else if (src.Level < 20)
                {
                    return false;
                }
                else if (src.inGuild)
                {

                }
                else if (src.Gold < 10000)
                {

                }
                else
                {
                    try
                    {
                        //take money
                        src.TakeGold(20000);

                        Guild cg = new Guild();
                        cg.GuildName = GuildName;
                        cg.GuildID = AvailableGuildID;//this way ist will get the next available ID
                        cg.Leader = new GuildMember(src);
                        cg.DateCreator = DateTime.Today; // data create guild need fix
                        cg.IconGuil = 3402; //default insigna
                        cg.Rules = "";
                        
                        SendPacket s = new SendPacket();
                        s.PackArray(new byte[] { 39, 1, 0 }); // clean Tab guild
                        src.Send(s);

                        cg.AddMember(src,true);
                        
                        GlobalGuild.Add(cg.GuildID, cg); // add new guild here <--

                        onPlayerLogin(ref src,cg.GuildID);

                        //depending on the conversation between npc this may not be needed
                        //unlock player dialogs if true;
                        s = new SendPacket();
                        s.PackArray(new byte[] { 20, 8 });
                        src.Send(s);


                    }
                    catch { return false; }
                    
                }
                return true;
            }
        }
        
        public void onPlayerLogin(ref Player src,ushort guildID)
        {

            if(GlobalGuild.ContainsKey(guildID))
            {
                
                src.CurGuild = GlobalGuild[guildID];

                GlobalGuild[guildID].SendInfo(ref src);



                        //give Player Reference to Guild
                        
            }
        }
    }


    public class Guild
    {
        
        public UInt16 GuildID;        
        public string GuildName;
        public string Rules;
        public uint IconGuil;
        public DateTime DateCreator;

        public GuildMember Leader;

        List<GuildMember> ViceOrg = new List<GuildMember>(4);
        List<GuildMember> Members = new List<GuildMember>(50);
        List<byte> ImgInsigne = new List<byte>();
        public int TotalMembers { get { return (5 + Members.Count); } }


        public Dictionary<uint,GuildMember>MembersOnlinne 
        {
            get{
                Dictionary<uint,GuildMember> tmp = new Dictionary<uint,GuildMember>();
                foreach(var t in Members.Where(c=>c.isOnline).ToList())
                tmp.Add(t.ID,t);
                return tmp;
            }
        }


        public Guild()
        {

        }

        public void ChangePermissionMember(uint actor, uint target,byte p1,byte p2, byte p3, byte p4, byte p5)
        {
            if (Members.Count(c=>c.ID == target)> 0)
            {

                //MembersDataBase[target].restriction1 = p1;
                //MembersDataBase[target].restriction2 = p2;
                //MembersDataBase[target].restriction3 = p3;
                //MembersDataBase[target].restriction4 = p4;
                //MembersDataBase[target].restriction5 = p5;

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
            //if (Members.Count(c => c.ID == target) > 0)
            //{
            //    switch (type)
            //    {
            //        case 0: if (TotalVice > 0) TotalVice--; break;
            //        case 1: if (TotalVice < 5) TotalVice++; break;

            //    }

            //    SendPacket s = new SendPacket();
            //    s.PackArray(new byte[] { 39,12});
            //    s.Pack32(target);
            //    BroadCastGuild(s, 0); // all receiv
            //}
        }
        public void AddNewMemberGuild(Player src)
        {
            if (Members.Count(c => c.ID == src.ID) > 0) return;
            else
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {39,4 });
                s.Pack32(src.UserID);
                s.Pack8(1);
                src.Send(s);
                AddMember(src, false);
                //Send39_2(src.UserID);
                //Send39_21_26_27(src.UserID);
                //GetMemberOnLinne(src.UserID);
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
        
        public void AddMember(Player src,bool Master)
        {
            if (Members.Count(c=>c.ID == src.ID) > 0) return;            
            else
            {
                GuildMember mw = new GuildMember();
                mw.OnlineSrc = src;

                #region Restriciton member
                if (Master)
                {
                    mw.can_Modify_Badge = true;
                    mw.can_Invite = true;
                    mw.can_DisBand_Guild = true;
                    mw.can_Modify_Rights = true;
                    mw.can_Modify_Rules = true;

                }
                #endregion

                Members.Add(mw);
            }
        }

        public bool SendInfo(ref Player src)
        {
            Send39_2(src);
            Send39_21_26_27(src);
            GetMemberOnLinne(src);
            GetMemberState(src.UserID);
            GetGuilNickName(src.UserID);
            return true;
        }

        void Send39_2(Player src) // here send database member ON and OFF
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 2 });
            s.PackString(GuildName);//guild name
            s.Pack8(0); // total leaders 
            s.Pack8((byte)TotalMembers);// total player in guild +4 - number leaders

            #region LoadPLayers
            foreach (var pair in Members.ToList())
            {
                s.Pack32(pair.ID);
                s.PackString(pair.CharacterName);
                s.Pack8(pair.Level);
                s.Pack8((byte)pair.Job);
                s.PackBoolean(pair.Reborn);
                s.Pack8((byte)pair.Element);
                s.Pack8((byte)pair.Body);
                s.Pack8(pair.Head);
                s.Pack16(pair.HairColor);
                s.Pack16(pair.SkinColor);
                s.Pack16(pair.ClothingColor);
                s.Pack16(pair.EyeColor);
                s.PackString(pair.Nickname);
                s.Pack32(0);
                s.PackBoolean(pair.can_DisBand_Guild);
                s.PackBoolean(pair.can_Invite);
                s.PackBoolean(pair.can_Modify_Rules);
                s.PackBoolean(pair.can_Modify_Rights);
                s.PackBoolean(pair.can_Modify_Badge);
                s.Pack32(0);
                s.Pack8(0);
            }
            #endregion

            s.PackString(Rules); // rules! defalt = 0
            s.PackArray(new byte[] {000, 000, 000, 000, 000, 000, 000, 000, 157,
                115, 241, 148, 144, 109, 228, 064, 000 }); // 17 need fix here.data timer creator:??
            src.Send(s);
        }
        public void Send39_17(Player src)
        {
            if (Members.Count(c => c.ID == src.ID) > 0)
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 17 });                
                src.Send(s);
            }

        }        
        public void Send39_21_26_27(Player src)
        {
            if (Members.Count(c => c.ID == src.ID) > 0)
            {
                #region Packet 39,21  39,26 39, 27

                for (int a = 1; a < 8; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 21 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    src.Send(s);
                }
                for (int a = 1; a < 5; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 26 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    src.Send(s);
                }
                for (int a = 1; a < 8; a++)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 27 });
                    s.Pack8((byte)a);
                    s.Pack32(0);
                    src.Send(s);
                }

                #endregion

            }
        }     
  
        public void GetMemberOnLinne(Player src)
        {
            foreach (var pair in Members.ToList())
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 60 });
                s.Pack32(pair.ID);
                s.PackString(pair.CharacterName);
                s.Pack8(pair.Level);
                s.Pack8((byte)pair.Job);// job

                if (pair.Reborn) s.Pack8(1);//reborn
                else s.Pack8(0);

                s.Pack8((byte)pair.Element);//elemnt
                s.Pack8((byte)pair.Body);// body
                s.Pack8((byte)pair.Head);// head
                s.Pack16(pair.HairColor);
                s.Pack16(pair.SkinColor);
                s.Pack16(pair.ClothingColor);
                s.Pack16(pair.EyeColor);
                s.PackString(pair.Nickname);
                src.Send(s);
            }
        }
        //39,61 este pacote ele é foda precisa estudar mais um pouco ele manda alocado.
        public void GetMemberState(uint src)
        {
            //foreach (var pair in MembersOnlinne)
            //{               
            //    SendPacket s = new SendPacket();
            //    s.PackArray(new byte[] { 39, 61 });
            //    s.Pack32(pair.Value.UserID);
            //    s.Pack8(Convert.ToByte(pair.Value.Busy));// 0 = onlinne 1 = busy
            //    MembersOnlinne[src].Send(s);              
            //}
        }
        void GetInsigneGuild(uint src)
        {            
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 39, 30,2 });
                s.PackArray(ImgInsigne.ToArray());
                //MembersOnlinne[src].Send(s);            
        }
        public void GetGuilNickName(uint src)
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 9 });
            s.Pack32(src);
            s.Pack32(IconGuil); // UINT ICON GUILD
            s.PackString(GuildName); // name guild NICK SHOW IN MAP (acima do nick name)
            //MembersOnlinne[src].Send(s);
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
                //foreach (var pair in MembersOnlinne)
                //{
                //    if (pair.Value.UserID != exceptID)
                //    {
                //        pair.Value.Send(spk);
                //    }
                //}            
        }
        void UpdateMemberInfoDataBase(Player src)
        {
            uint user = src.UserID;
            //if (Members.Count(c => c.ID == src.ID) > 0)
            //{
            //    MembersDataBase[user].NickName = src.Nickname;
            //    MembersDataBase[user].reborn = Convert.ToByte(src.Reborn);
            //    MembersDataBase[user].Job = (byte)src.Job;
            //    MembersDataBase[user].color1 = src.HairColor;
            //    MembersDataBase[user].color2 = src.SkinColor;
            //    MembersDataBase[user].color3 = src.ClothingColor;
            //    MembersDataBase[user].color4 = src.EyeColor;

            //}
        }


        void SendMeToMembersGuild(Player src)
        {
            foreach (var pair in MembersOnlinne)
            {
                if (pair.Value.ID != src.ID)
                {
                    SendPacket s = new SendPacket();
                    s.PackArray(new byte[] { 39, 60 });
                    s.Pack32(pair.Value.ID);
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
                   src.Send(s);
                }
            }

        }

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

            //MembersOnlinne[src].Send(s);
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
            Members.Remove(Members.Single(c=>c.ID ==src));
            //MembersOnlinne[src].GuildID = 0; // clean
            //MembersOnlinne.Remove(src);

        }
    }

}
