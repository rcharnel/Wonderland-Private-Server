using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlo.Core;
using Game;

namespace Server.Network.WAC
{
    public class AC63 : WLOAC
    {
        public override int ID
        {
            get
            {
                return 63;
            }
        }

        public override void Process(Player client, SendPacket r)
        {
            switch (r.Unpack8())
            {
                case 0: { cGlobal.gUserDataBase.unRegisterUser(client); client.Clear(); } break;
                case 3:client.CharName = ""; break;
                case 2: Recv2(client, r); break;
                case 4: Recv4(client, r); break;
                default: base.Process(client, r); break;
            }
        }

        async void Recv2(Player p, SendPacket e)
        {
            p.Slot = e.Unpack8();

            if ((p.Slot < 1) || (p.Slot > 2))//by userid
            {
                p.Clear();
                return;
            }
            if (p.Load_CharacterInfo(await cGlobal.gGameDataBase.RequestCharacterInfo((p.Slot == 1) ? p.UserAcc.Character1ID : p.UserAcc.Character2ID)))
            {
                //p.SendPacket(SendPacket.FromFormat<OutgoingPacket>("bbd", 63, 2, p.UserID));
                cGlobal.GameServer.CommenceLogin(p);
            }
            else
            {
                p.Flags.Add(PlayerFlag.Creating_Character);
                p.SendPacket(SendPacket.FromFormat("bbb", 1, 3, (!string.IsNullOrEmpty(p.UserAcc.Cipher))));
            }
        }

        async void Recv4(Player client, SendPacket r)
        {
            //try
            //{
            int loginState = 0; //0-good login  1-bad un/pw  2-dup log 3-wrong version 4-need update

            //get username and password

            string name = r.UnpackString().ToLower();
            string password = r.UnpackString();
            UInt16 version = r.Unpack16();
            byte lcLen = r.Unpack8();
            byte key = r.Unpack8();
            char[] lCode = new char[20];
            Array.Copy(r.Buffer.ToArray<byte>(), r.m_nUnpackIndex, lCode, 0, lcLen);
            for (int n = 0; n < lcLen; n++)
                lCode[n] = (char)((byte)lCode[n] ^ (byte)key);

            if (((name.Length < 4) || (name.Length > 14)) || ((password.Length < 4) || (password.Length > 14)))
            {
                loginState = 2;
                DebugSystem.Write(DebugItemType.Info_Heavy,client.SockAddress() + " Username or Passowrd length not valid" );
                goto checkResult;
            }
            else if (version < 1096)
            { //bad aloign version
                loginState = 3;
                DebugSystem.Write(DebugItemType.Info_Heavy, client.SockAddress() + " client version not valid");
                goto checkResult;
            }
            else if ((lcLen < 2) || (lcLen > 15))
            { //bad login code length
                loginState = 4;
                DebugSystem.Write(DebugItemType.Info_Heavy, client.SockAddress() + " logincode not valid");
                goto checkResult;

            }

            //Validate Account
            if ((loginState = (cGlobal.gUserDataBase.isLoggedin(name)) ? 1 : 0) != 0) goto checkResult;
            if ((loginState = cGlobal.gUserDataBase.LoadUserData(name, password, client))!= 0) goto checkResult;

            loginState = (cGlobal.gGameDataBase.isOnline(client.UserAcc.Character1ID) || cGlobal.gGameDataBase.isOnline(client.UserAcc.Character2ID)) ? 1 : 0;


        checkResult:

            PacketBuilder t = new PacketBuilder();

            t.Begin(false);          

            #region Result of Login State
            // here we do the results of loginstate
            switch (loginState)
            {
                #region No Errors Loggin in 0
                case 0:
                    {
                        t.Add(SendPacket.FromFormat("bbd", 63, 2, (loginState == 0) ? client.UserID : 0));
                        SendPacket pkt = new SendPacket(new byte[] { 244, 68, 0, 0, 63, 1 });
                        pkt.PackArray((await cGlobal.gGameDataBase.RequestCharacterInfo(client.UserAcc.Character1ID)).ToArray());
                        pkt.PackArray((await cGlobal.gGameDataBase.RequestCharacterInfo(client.UserAcc.Character2ID)).ToArray());
                        pkt.SetHeader();
                        t.Add(pkt);
                        t.Add(SendPacket.FromFormat("bb", 35, 11));
                    } break;
                #endregion
                case 1: t.Add(Handles.ErrorHandle.SendError(Handles.GameErrorSection.AlreadyOnline)); break;
                case 2: t.Add(SendPacket.FromFormat("bb", 1, 6)); break;
                case 3: t.Add(Handles.ErrorHandle.SendError(Handles.GameErrorSection.BadGameVersion)); break;
                case 4: t.Add(Handles.ErrorHandle.SendError(Handles.GameErrorSection.BadItemDat)); break;
                case 5:
                    {
                        //SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                        ////PackSend->Clear();
                        //sp.PackArray(new byte[] { 1, 7 });//PackSend->Header(63,2);
                        //p.Send(sp);
                    } break;
            }
            #endregion
            client.SendPacket(t.End());
        }

        //#region wlo methods
        //byte[] Get_63_1Data(Character player, byte slot)
        //{
        //    if (player == null) return null;
        //    SendPacket temp = new SendPacket();
        //    temp.Pack8(slot);// data[at] = slot; at++;//PackSend->Pack8(1);
        //    temp.PackString(player.CharacterName);// data[at] = nameLen; at++;
        //    temp.Pack8((byte)player.Level);// data[at] = level; at++;//	PackSend->Pack8(tmp1.level);					// Level 
        //    temp.Pack8((byte)player.Element);// data[at] = element; at++;//	PackSend->Pack8(3);  					// element
        //    temp.Pack32((uint)player.FullHP);// putDWord(maxHP, data + at); at += 4;//	PackSend->Pack32(tmp1.maxHP); 			// max hp
        //    temp.Pack32((uint)player.CurHP);// putDWord(curHP, data + at); at += 4;//	PackSend->Pack32(tmp1.curHP); 			// cur hp
        //    temp.Pack32((uint)player.FullSP);// putDWord(maxSP, data + at); at += 4;//	PackSend->Pack32(tmp1.maxSP); 			// max sp
        //    temp.Pack32((uint)player.CurSP);// putDWord(curSP, data + at); at += 4;//	PackSend->Pack32(tmp1.curSP); 			// cur sp
        //    temp.Pack32((uint)player.TotalExp);// putDWord(experience, data + at); at += 4;//	PackSend->Pack32(tmp1.exp);			// exp
        //    temp.Pack32(player.Gold);// putDWord(gold, data + at); at += 4;//	PackSend->Pack32(tmp1.gold); 			// gold
        //    temp.Pack8((byte)player.Body);// data[at] = body; at++;//	PackSend->Pack8(tmp1.body); 					// body style
        //    temp.Pack8(0);
        //    temp.Pack8(player.Head);
        //    temp.Pack8(0);// data[at] = 0; data[at + 1] = head; data[at + 2] = 0; at += 3;//	PackSend->PackArray(tmp1.hair,3);// hair style
        //    temp.Pack16(player.HairColor);// putDWord(color1, data + at); at += 4;//	PackSend->Pack32(tmp1.colors1);
        //    temp.Pack16(player.SkinColor);
        //    temp.Pack16(player.ClothingColor);
        //    temp.Pack16(player.EyeColor);
        //    temp.PackBoolean(player.Reborn);
        //    temp.Pack8((byte)player.Job);// data[at] = rebirth; data[at + 1] = job; at += 2;//PackSend->Pack8(tmp1.rebirth);PackSend->Pack8(tmp1.rebirthJob); 				// rebirth flag, job skill
        //    for (byte a = 1; a < 7; a++)
        //        temp.Pack16(player[a].ItemID);

        //    if (temp.Data.Count < 1) return null;
        //    return temp.Data.ToArray();
        //}
        //void NormalLog(Player c)
        //{
        //    c.BlockSave = false;
        //    //c.CharacterState = GameCharacter.PlayerState.Logging_In;
        //    //a connection request was recieved
        //    // c.DataOut = SendType.Multi;
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 20, 8 });
        //    c.Send(p);

        //    Send_24_5(c, 183);
        //    Send_24_5(c, 53);
        //    Send_24_5(c, 52);
        //    Send_24_5(c, 54);
        //    Send_70_1( c, 23, "Something", 194);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 20, 33 });
        //    p.Pack8(0);
        //    c.Send(p);
        //    //--------------------------Player PreInfo
        //    c.Send8_1();
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 14, 13 });
        //    p.Pack8(3);
        //    c.Send(p);
        //    //------------------------Im Mall List
        //    //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 75, 8 });
        //    p.Pack16(0);
        //    c.Send(p);
        //    //------------------------
        //    c.Send_3_Me();
        //    cGlobal.WLO_World.SendCurrentPlayers(c);
        //    cGlobal.gGameDataBase.LoadFinalData( c);
        //    //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Frederico"),true);//,31038);
        //    //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Niss"),true);//,31026);
        //    c.Send(c.Pets.PetlistData);
        //    c.Pets.SendPetlistStatData();
        //    c.Pets.Bring_into_Battle(1);
        //    //send sidebar
        //    c.Send_5_3();
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 5 });
        //    p.PackArray(c.Inv._23_5Data);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 11 });
        //    p.PackArray(c.Eqs._23_11Data);
        //    c.Send(p);
        //    if (c.Pets.BattlePet != null)
        //    {
        //        SendPacket tmp = new SendPacket();
        //        tmp.PackArray(new byte[] { 19, 4 });
        //        tmp.Pack32(c.Pets.BattlePet.ID);
        //        c.Send(tmp);
        //    }
        //    SendPacket g = new SendPacket();
        //    //    g.PackArray(new byte[]{(24, 6);
        //    //    g.PackArray(new byte[] { 001, 008, 047, 001, 002, 244, 050, 001, 003, 012, 043, 001 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    //    g = new SPacket();
        //    //    g.PackArray(new byte[]{(53, 10);
        //    //    g.PackArray(new byte[] { 032, 164, 036, 002, 037, 240, 038, 041, 058, 048, 083, 015 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    //    g = new SPacket();
        //    //    g.PackArray(new byte[]{(26, 7);
        //    //    g.PackArray(new byte[] { 001, 002, 002, 128, 003, 002, 004, 128 ,008,
        //    //                066, 009, 096, 010, 008, 011, 010 ,013, 001 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 26, 4 });
        //    p.Pack32(c.Gold);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 33, 2 });
        //    p.PackArray(c.Settings.Data);
        //    c.Send(p);
        //    c.SendFriendList();

        //    //pets
        //    //-----------------------------------   
        //    //---------Warp Info---------------------------------------------------
        //    //put me in my maps list
        //    WarpData tmp2 = new WarpData();
        //    tmp2.DstMap = c.LoginMap;
        //    tmp2.DstX_Axis = c.X;
        //    tmp2.DstY_Axis = c.Y;
        //    if (!cGlobal.WLO_World.onTelePort(0, tmp2,  c))
        //    {
        //        p = new SendPacket(true);
        //        p.PackArray(new byte[] { 0, 7 });
        //        c.Send(p);

        //        return;
        //    }


        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 15 });
        //    p.Pack8(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 62, 53 });
        //    p.Pack16(2);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 21 });
        //    p.Pack8((byte)c.Slot);//hmmmmm
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 11 });
        //    p.Pack32(15085);//hmmmmm
        //    p.Pack16(5000);
        //    c.Send(p);
        //    //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0,         

        //    //---------------------------------

        //    //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items

        //    //--------------------------------------
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 14 });
        //    p.Pack8(2);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 16 });
        //    p.Pack8(2);
        //    c.Send(p);
        //    var time = DateTime.Now.ToOADate();
        //    Send_23_140( c, 3, time);
        //    Send_25_44( c, 2, time);
        //    //g.ac23.Send_106(1, 1);
        //    Send_SingleByte_AC( c, 23, 160, (byte)3);
        //    Send_SingleByte_AC( c, 75, 7, (byte)1);
        //    Send_57("Welcome to the  WLO 4 EVER Community Server :! Enjoy !!",  c);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 69, 1 });
        //    p.Pack8(71);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 20, 60 });
        //    p.Pack8(1);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 66, 1 });
        //    p.PackArray(new byte[] { 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 });
        //    c.Send(p);
        //    for (byte a = 1; a < 11; a++)
        //        Send_5_13( c, a, 0);
        //    for (byte a = 1; a < 11; a++)
        //        Send_5_24( c, a, 0);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 162 });
        //    p.Pack8(2);
        //    p.Pack16(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 26, 10 });
        //    p.Pack32(0);
        //    c.Send(p);
        //    Send_SingleByte_AC( c, 23, 204, (ushort)1);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 208 });
        //    p.Pack8(2);
        //    p.Pack8(3);
        //    p.Pack32(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 208 });
        //    p.Pack8(2);
        //    p.Pack8(4);
        //    p.Pack32(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 1, 11 });
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 15, 19 });
        //    p.PackArray(new byte[] { 4, 6, 9, 94 });
        //    c.Send(p);

        //    //--------------------------------
        //    /*p = new SendPacket();
        //    p.PackArray(new byte[]{(20, 33);
        //    p.Pack8(0);
        //    p.SetSize();
        //    c.Send(p);*/
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 54 });
        //    p.PackArray(
        //    new byte[] {
        //         89,2,2,90,2,1,91,2,1,189,2,2,190,2,1,191,2,1});
        //    c.Send(p);
        //    SendPacket im = new SendPacket();
        //    im.PackArray(new byte[] { 35, 4 });
        //    im.Pack32(0);
        //    im.Pack32(0);
        //    im.Pack32(0);
        //    im.Pack32(0);
        //    c.Send(im);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 90, 1 });
        //    p.PackArray(new byte[] { 000, 2, 2, 3 });
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 4 });
        //    c.Send(p);
        //    c.DataOut = SendType.Normal;
        //    //c.CharacterState = GameCharacter.PlayerState.inMap;
        //    //---------------------------------
        //}


        //void Send_SingleByte_AC( Player player, byte ac, byte subac, object byteval)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { ac, subac });
        //    if (byteval is byte)
        //    {
        //        p.Pack8((byte)byteval);
        //    }
        //    else if (byteval is ushort)
        //    {
        //        p.Pack16((ushort)byteval);
        //    }
        //    else if (byteval is UInt32)
        //    {
        //        p.Pack32((UInt32)byteval);
        //    }
        //    player.Send(p);

        //}
        //void Send_SingleByte_AC( Player player, byte ac, object byteval)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { ac });
        //    if (byteval is byte)
        //    {
        //        p.Pack8((byte)byteval);
        //    }
        //    else if (byteval is ushort)
        //    {
        //        p.Pack16((ushort)byteval);
        //    }
        //    player.Send(p);

        //}
        //void Send_57(string text,  Player o) //sends sytem prompt message
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 57 });
        //    p.Pack8(0);
        //    p.PackNString(text);
        //    o.Send(p);
        //}
        //void Send_24_5(Player player, byte value)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 24, 5 });
        //    p.Pack8(value);
        //    p.Pack16(0);
        //    player.Send(p);

        //}
        //void Send_70_1( Player player, byte value, string name, UInt16 id)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 70, 1 });
        //    p.Pack8(value);
        //    p.PackString(name);
        //    p.Pack16(id);
        //    p.Pack8(0);
        //    player.Send(p);

        //}
        //void Send_23_140( Player player, byte val, Double t)//time related
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 23, 140 });
        //    p.Pack8(val);
        //    p.Pack64((ulong)t);
        //    player.Send(p);
        //}
        //void Send_25_44( Player player, byte val, double v)//time related
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 25, 44 });
        //    p.Pack8(val);
        //    p.Pack64((ulong)v);
        //    player.Send(p);
        //}
        //void Send_5_13( Player player, byte value, UInt16 wVal)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 13 });
        //    p.Pack8(value);
        //    p.Pack16(wVal);
        //    player.Send(p);
        //}
        //void Send_5_24( Player player, byte Val, UInt16 wVal)
        //{
        //    SendPacket p = new SendPacket();
        //    p.PackArray(new byte[] { 5, 24 });
        //    p.Pack8(Val);
        //    p.Pack16(wVal);
        //    player.Send(p);
        //}
        //#endregion

    }
}
