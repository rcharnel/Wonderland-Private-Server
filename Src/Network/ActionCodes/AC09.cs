using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using RCLibrary.Core.Networking;
using Game;
using Game.Code;

namespace Network.ActionCodes
{
    public class AC09:AC
    {
         public override int ID { get { return 09; } }

        public override void ProcessPkt(Player r, Packet p)
        {
            switch (p.Unpack8())
            {
                case 1: Recv1(ref r, p); break;
                case 2: Recv2(r, p); break;
            }
        }

        void Recv1(ref Player tp, Packet e)
        {
            try
            {
                tp.Eqs.Body = (BodyStyle)e.Unpack16();
                tp.Eqs.Head = (byte)e.Unpack16();
                tp.HairColor = e.Unpack16();
                tp.SkinColor = e.Unpack16();
                tp.ClothingColor = e.Unpack16();
                tp.EyeColor = e.Unpack16();
                tp.Eqs.Element = (Affinity)e.Unpack8();
                tp.Eqs.Str = e.Unpack8();
                tp.Eqs.Agi = e.Unpack8();
                tp.Eqs.Wis = e.Unpack8();
                tp.Eqs.Int = e.Unpack8();
                tp.Eqs.Con = e.Unpack8();
                if (string.IsNullOrEmpty(tp.UserAcc.Cipher))
                {

                    tp.UserAcc.Cipher = e.UnpackString();
                    if ((tp.UserAcc.Cipher.Length < 6) ||
                        (tp.UserAcc.Cipher.Length > 14))
                    {
                        //make sure no save of data by setting values to 0
                        tp.Clear();
                        tp.Send(new SendPacket(Packet.FromFormat("bb", 0, 30)));
                        return;
                    }
                }

                tp.SetBeginnerOutfit();

                tp.FillHP(); tp.FillSP();
                tp.Settings.ChannelCode = (ChannelCodeType)31;
                tp.Settings.TRADABLE = true;
                tp.Settings.PKABLE = false;
                tp.Settings.JOINABLE = true;
                tp.Eqs.TotalExp = 6;
                tp.LoginMap = 60000; //ship map 10017;
                tp.CurX = 602; // ship x 1042;
                tp.CurY = 455; //ship y 1075;
                tp.SetGold(0);
                //tp.Started_Quests.Add(new Quest() { QID = 1 });

                //tp.Info.MySkills.AddSkill(cGlobal.SkillManager.GetSkillByID(15003));
                //create a character id for this new character
                //tp.Info.ID = tp.UserID + (byte)(tp.Slot - 1);
                // cGlobal.GameDB.CreateInventory((sender as WloPlayer));

                if (cGlobal.gCharacterDataBase.WriteNewPlayer(tp.CharID, tp))
                {
                    cGlobal.gUserDataBase.Update_Player_ID(tp.UserAcc.DataBaseID, tp.CharID, (byte)tp.Slot);
                    cGlobal.gUserDataBase.UpdateUser(tp.UserAcc.DataBaseID, tp.UserAcc.Cipher);
                    //NormalLog(tp);
                }
                else
                    throw new Exception("unable to write player");

                //tp.CharacterState = GameCharacter.PlayerState.Logging_In;

            }
            catch (Exception t)
            {
                DebugSystem.Write(new ExceptionData(t));
                tp.Send(new SendPacket(Packet.FromFormat("bb", 0, 30)));
            }


        }

        void Recv2( Player tp, Packet e)
        {
            int nameLen = e.Count - 2;
            string name = e.UnpackStringN();
            if ((nameLen < 4) || (nameLen > 14) || !cGlobal.gCharacterDataBase.LockName(tp.UserAcc.DataBaseID, name))
            {
                tp.Send(new SendPacket(Packet.FromFormat("bbb", 9, 3, 1)));
                return;
            }
            tp.CharName = name;
            tp.Send(new SendPacket(Packet.FromFormat("bbb", 9, 3, 0)));
        }




        #region wlo methods

        //byte[] Get_63_1Data(Character player, byte slot)
        //{
        //    Packet temp = new Packet();
        //    temp.Pack(slot);// data[at] = slot; at++;//PackSend->Pack(1);
        //    temp.Pack(player.CharacterName);// data[at] = nameLen; at++;
        //    temp.Pack((byte)player.Level);// data[at] = level; at++;//	PackSend->Pack(tmp1.level);					// Level 
        //    temp.Pack((byte)player.Element);// data[at] = element; at++;//	PackSend->Pack(3);  					// element
        //    temp.Pack((uint)player.FullHP);// putDWord(maxHP, data + at); at += 4;//	PackSend->Pack(tmp1.maxHP); 			// max hp
        //    temp.Pack((uint)player.CurHP);// putDWord(curHP, data + at); at += 4;//	PackSend->Pack(tmp1.curHP); 			// cur hp
        //    temp.Pack((uint)player.FullSP);// putDWord(maxSP, data + at); at += 4;//	PackSend->Pack(tmp1.maxSP); 			// max sp
        //    temp.Pack((uint)player.CurSP);// putDWord(curSP, data + at); at += 4;//	PackSend->Pack(tmp1.curSP); 			// cur sp
        //    temp.Pack((uint)player.TotalExp);// putDWord(experience, data + at); at += 4;//	PackSend->Pack(tmp1.exp);			// exp
        //    temp.Pack(player.Gold);// putDWord(gold, data + at); at += 4;//	PackSend->Pack(tmp1.gold); 			// gold
        //    temp.Pack((byte)player.Body);// data[at] = body; at++;//	PackSend->Pack(tmp1.body); 					// body style
        //    temp.Pack(0);
        //    temp.Pack(player.Head);
        //    temp.Pack(0);// data[at] = 0; data[at + 1] = head; data[at + 2] = 0; at += 3;//	PackSend->Pack(tmp1.hair,3);// hair style
        //    temp.Pack(player.HairColor);// putDWord(color1, data + at); at += 4;//	PackSend->Pack(tmp1.colors1);
        //    temp.Pack(player.SkinColor);
        //    temp.Pack(player.ClothingColor);
        //    temp.Pack(player.EyeColor);
        //    temp.Pack(player.Reborn);
        //    temp.Pack((byte)player.Job);// data[at] = rebirth; data[at + 1] = job; at += 2;//PackSend->Pack(tmp1.rebirth);PackSend->Pack(tmp1.rebirthJob); 				// rebirth flag, job skill
        //    for (byte a = 1; a < 7; a++)
        //        temp.Pack(player[a].ItemID);

        //    //if (temp.Data.Count < 1) return null;
        //    //return temp.Data.ToArray();
        //}

        //void NormalLog(Player c)
        //{
        //    c.BlockSave = false;
        //    try
        //    {
        //        //c.CharacterState = GameCharacter.PlayerState.Logging_In;
        //        //a connection request was recieved
        //        c.DataOut = SendType.Multi;
        //        SendPacket p = new SendPacket();
        //        p.Pack(new byte[] { 20, 8 });
        //        c.Send(p);

        //        Send_24_5(c, 183);
        //        Send_24_5(c, 53);
        //        Send_24_5(c, 52);
        //        Send_24_5(c, 54);
        //        Send_70_1(ref c, 23, "Something", 194);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 20, 33 });
        //        p.Pack(0);
        //        c.Send(p);
        //        //--------------------------Player PreInfo
        //        c.Send8_1();
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 14, 13 });
        //        p.Pack(3);
        //        c.Send(p);
        //        //------------------------Im Mall List
        //        //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 75, 8 });
        //        p.Pack(0);
        //        c.Send(p);
        //        //------------------------
        //        c.Send_3_Me();
        //        cGlobal.WLO_World.SendCurrentPlayers(c);
        //        //Load final Data
        //        cGlobal.gGameDataBase.LoadFinalData(ref c);
        //        //send sidebar
        //        c.Send_5_3();
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 23, 5 });
        //        p.Pack(c.Inv._23_5Data);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 23, 11 });
        //        p.Pack(c.Eqs._23_11Data);
        //        c.Send(p);
        //        SendPacket g = new SendPacket();
        //        //    g.Pack(new byte[]{(24, 6);
        //        //    g.Pack(new byte[] { 001, 008, 047, 001, 002, 244, 050, 001, 003, 012, 043, 001 });
        //        //    g.SetSize();
        //        //    Send(g);
        //        //    g = new SPacket();
        //        //    g.Pack(new byte[]{(53, 10);
        //        //    g.Pack(new byte[] { 032, 164, 036, 002, 037, 240, 038, 041, 058, 048, 083, 015 });
        //        //    g.SetSize();
        //        //    Send(g);
        //        //    g = new SPacket();
        //        //    g.Pack(new byte[]{(26, 7);
        //        //    g.Pack(new byte[] { 001, 002, 002, 128, 003, 002, 004, 128 ,008,
        //        //                066, 009, 096, 010, 008, 011, 010 ,013, 001 });
        //        //    g.SetSize();
        //        //    Send(g);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 26, 4 });
        //        p.Pack(c.Gold);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 33, 2 });
        //        p.Pack(c.Settings.Data);
        //        c.Send(p);
        //        c.SendFriendList();

        //        //pets
        //        //-----------------------------------   
        //        //---------Warp Info---------------------------------------------------
        //        //put me in my maps list
        //        WarpData tmp2 = new WarpData();
        //        tmp2.DstMap = c.LoginMap;
        //        tmp2.DstX_Axis = c.X;
        //        tmp2.DstY_Axis = c.Y;
        //        if (!cGlobal.WLO_World.onTelePort(0, tmp2, ref c))
        //        {
        //            p = new SendPacket(true);
        //            p.Pack(new byte[] { 0, 7 });
        //            c.Send(p);
        //            return;
        //        }


        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 15 });
        //        p.Pack(0);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 62, 53 });
        //        p.Pack(2);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 21 });
        //        p.Pack((byte)c.Slot);//hmmmmm
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 11 });
        //        p.Pack(15085);//hmmmmm
        //        p.Pack(5000);
        //        c.Send(p);
        //        //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0,         

        //        //---------------------------------

        //        //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items

        //        //--------------------------------------
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 14 });
        //        p.Pack(2);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 16 });
        //        p.Pack(2);
        //        c.Send(p);
        //        var time = DateTime.Now.ToOADate();
        //        Send_23_140(ref c, 3, time);
        //        Send_25_44(ref c, 2, time);
        //        //g.ac23.Send_106(1, 1);
        //        Send_SingleByte_AC(ref c, 23, 160, (byte)3);
        //        Send_SingleByte_AC(ref c, 75, 7, (byte)1);
        //        Send_57("Welcome to the  WLO 4 EVER Community Server :! Enjoy !!", ref c);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 69, 1 });
        //        p.Pack(71);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 20, 60 });
        //        p.Pack(1);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 66, 1 });
        //        p.Pack(new byte[] { 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 });
        //        c.Send(p);
        //        for (byte a = 1; a < 11; a++)
        //            Send_5_13(ref c, a, 0);
        //        for (byte a = 1; a < 11; a++)
        //            Send_5_24(ref c, a, 0);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 23, 162 });
        //        p.Pack(2);
        //        p.Pack(0);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 26, 10 });
        //        p.Pack(0);
        //        c.Send(p);
        //        Send_SingleByte_AC(ref c, 23, 204, (ushort)1);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 23, 208 });
        //        p.Pack(2);
        //        p.Pack(3);
        //        p.Pack(0);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 23, 208 });
        //        p.Pack(2);
        //        p.Pack(4);
        //        p.Pack(0);
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 1, 11 });
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 15, 19 });
        //        p.Pack(new byte[] { 4, 6, 9, 94 });
        //        c.Send(p);

        //        //--------------------------------
        //        /*p = new SendPacket();
        //        p.Pack(new byte[]{(20, 33);
        //        p.Pack(0);
        //        p.SetSize();
        //        c.Send(p);*/
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 54 });
        //        p.Pack(
        //        new byte[] {
        //         89,2,2,90,2,1,91,2,1,189,2,2,190,2,1,191,2,1});
        //        c.Send(p);
        //        SendPacket im = new SendPacket();
        //        im.Pack(new byte[] { 35, 4 });
        //        im.Pack(0);
        //        im.Pack(0);
        //        im.Pack(0);
        //        im.Pack(0);
        //        c.Send(im);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 90, 1 });
        //        p.Pack(new byte[] { 000, 2, 2, 3 });
        //        c.Send(p);
        //        p = new SendPacket();
        //        p.Pack(new byte[] { 5, 4 });
        //        c.Send(p);
        //        c.DataOut = SendType.Normal;
        //        //c.CharacterState = GameCharacter.PlayerState.inMap;
        //        //---------------------------------
        //    }
        //    catch (Exception t) { Utilities.LogServices.Log(t); }
        //}

        //void Send_SingleByte_AC(ref Player player, byte ac, byte subac, object byteval)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { ac, subac });
        //    if (byteval is byte)
        //    {
        //        p.Pack((byte)byteval);
        //    }
        //    else if (byteval is ushort)
        //    {
        //        p.Pack((ushort)byteval);
        //    }
        //    else if (byteval is UInt32)
        //    {
        //        p.Pack((UInt32)byteval);
        //    }
        //    player.Send(p);

        //}
        //void Send_SingleByte_AC(ref Player player, byte ac, object byteval)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { ac });
        //    if (byteval is byte)
        //    {
        //        p.Pack((byte)byteval);
        //    }
        //    else if (byteval is ushort)
        //    {
        //        p.Pack((ushort)byteval);
        //    }
        //    player.Send(p);

        //}
        //void Send_57(string text, ref Player o) //sends sytem prompt message
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 23, 57 });
        //    p.Pack(0);
        //    p.PackNString(text);
        //    o.Send(p);
        //}
        //void Send_24_5(Player player, byte value)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 24, 5 });
        //    p.Pack(value);
        //    p.Pack(0);
        //    player.Send(p);

        //}
        //void Send_70_1(ref Player player, byte value, string name, UInt16 id)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 70, 1 });
        //    p.Pack(value);
        //    p.Pack(name);
        //    p.Pack(id);
        //    p.Pack(0);
        //    player.Send(p);

        //}
        //void Send_23_140(ref Player player, byte val, Double t)//time related
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 23, 140 });
        //    p.Pack(val);
        //    p.Pack((ulong)t);
        //    player.Send(p);
        //}
        //void Send_25_44(ref Player player, byte val, double v)//time related
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 25, 44 });
        //    p.Pack(val);
        //    p.Pack((ulong)v);
        //    player.Send(p);
        //}
        //void Send_5_13(ref Player player, byte value, UInt16 wVal)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 5, 13 });
        //    p.Pack(value);
        //    p.Pack(wVal);
        //    player.Send(p);
        //}
        //void Send_5_24(ref Player player, byte Val, UInt16 wVal)
        //{
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 5, 24 });
        //    p.Pack(Val);
        //    p.Pack(wVal);
        //    player.Send(p);
        //}
        #endregion
    }
}
