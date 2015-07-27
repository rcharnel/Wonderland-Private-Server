using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCLibrary.Core.Networking;
using Network;
using Game;

namespace Network.ActionCodes
{
    public class AC63:AC
    {
        public override int ID { get { return 63; } }

        public override void ProcessPkt(Player p,  Packet r)
        {
            switch (r.Unpack8())
            {
                case 0:
                //case 3: Recv3(ref p, r); break;
                case 2: Recv2(ref p, r); break;
                case 4: Recv4(ref p, r); break;
            }

        }

        void Recv3(ref Player p, Packet r)
        {
            //try
            //{
            //    cGlobal.gCharacterDataBase.unLockName(p.CharacterName);
            //    p.UserName = "";
            //    p.DataBaseID = 0;
            //}
            //catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv2(ref Player p, Packet e)
        {
            try
            {
                byte charNum = e.Unpack8();

                if ((charNum < 1) || (charNum > 2))//by userid
                {
                    p.Send(new SendPacket(Packet.FromFormat("bb", 0, 32)));
                    return;
                }

                p.Slot = charNum;

                if (cGlobal.gCharacterDataBase.GetCharacterData( (p.Slot == 1)?p.UserAcc.Character1ID:p.UserAcc.Character2ID) == null) // char is not created
                {
                    #region Create Character
                    cGlobal.gUserDataBase.Update_Player_ID(p.UserAcc.DataBaseID, p.CharID, charNum);
                    //p.State = PlayerState.Connected_CharacterCreation;

                    SendPacket tmp = new SendPacket();
                    tmp.Pack8(1);
                    tmp.Pack8(3);
                    tmp.PackBool((!string.IsNullOrEmpty(p.UserAcc.Cipher)));
                    p.Send(tmp);
                    #endregion
                }
                else
                {
                    #region Login
                    cGlobal.gCharacterDataBase.GetCharacterData((p.Slot == 1) ? p.UserAcc.Character1ID : p.UserAcc.Character2ID, ref p);
                    SendPacket tmp = new SendPacket();
                    tmp.Pack8(63);
                    tmp.Pack8(2);
                    tmp.Pack32(p.UserAcc.UserID);
                    p.Send(tmp);
                    cGlobal.gWorld.OnLogin(p);
                    //NormalLog(p);
                    #endregion
                }
            }
            catch (Exception t) { DebugSystem.Write(new ExceptionData(t)); throw; }
        }

        /// <summary>
        /// Recieved Login info from Client
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        void Recv4(ref Player p, Packet r)
        {
            try
            {
                int loginState = 0; //0-good login  1-bad un/pw  2-dup log 3-wrong version 4-need update

                //sending username and password
                string name = r.UnpackString();
                string password = r.UnpackString();
                name = name.ToLower();

                string[] userdata = null;//data of user

                UInt16 version = r.Unpack16();
                byte lcLen = r.Unpack8();
                byte key = r.Unpack8();
                char[] lCode = new char[20];
                Array.Copy(r.Buffer, r.m_nUnpackIndex, lCode, 0, lcLen);
                for (int n = 0; n < lcLen; n++)
                    lCode[n] = (char)((byte)lCode[n] ^ (byte)key);
                r.m_nUnpackIndex += lcLen;

                if ((name.Length < 4) || (name.Length > 14))
                {
                    loginState = 1;
                }
                else if ((password.Length < 4) || (password.Length > 14))
                {
                    loginState = 1;
                }
                else if (version < 1096)//bad aloign version
                { 
                    loginState = 3;
                }
                else if ((lcLen < 2) || (lcLen > 15))//bad login code length
                { 
                    loginState = 4;
                }

                //at this point we have no use, and only an empty cCharacter object
                //check to see if we are still in the clear for loggin ing
                if (loginState == 0)
                {
                    //should have correct version if at this point

                    uint dbid = 0;
                    //check if user exist on wloforever
                    #region Validate Account
                       p.UserAcc.DataBaseID =  (uint)cGlobal.gUserDataBase.isValidAccount(name, password);

                        if (p.UserAcc.DataBaseID != 0)
                        {
                            if (cGlobal.gLoginServer.IsOnline(p.UserAcc.UserID))
                            {
                                //TODO implement Disconnect
                                loginState = 2;
                                cGlobal.gLoginServer.Disconnect(p.UserAcc.UserID);
                            }
                            else
                            {
                                userdata =  cGlobal.gUserDataBase.GetUserData(p.UserAcc.DataBaseID, password);
                                if (userdata != null)
                                {
                                    p.UserAcc.UserName = userdata[0];
                                    p.UserAcc.Cipher = userdata[1];
                                    p.UserAcc.IM = int.Parse(userdata[2]);
                                }
                                else
                                    loginState = 1;
                            }
                        }
                        else
                            loginState = 1;
                    #endregion
                }
                //if (userdata != null)
                //    if (userdata.Length != 6)
                        //loginState = 1;
                    //else if (userdata.Length == 6 && userdata[4].ToString() == "0" && myhost.GameWorld.ServerStatus == ServerMode.TestMode)
                    //    loginState = 6;

                #region Result of Login State
                // here we do the results of loginstate
                switch (loginState)
                {
                    case 0:
                        {                            
                            SendPacket tmp = new SendPacket();
                            tmp.Pack8(63);
                            tmp.Pack8(2);
                            tmp.Pack32(p.UserAcc.UserID);
                            p.Send(tmp);

                            
                            tmp = new SendPacket();
                            tmp.Pack8(63);
                            tmp.Pack8(1);
                            tmp.PackArray(cGlobal.gCharacterDataBase.GetCharacterData(p.UserAcc.Character1ID).ToArray());
                            tmp.PackArray(cGlobal.gCharacterDataBase.GetCharacterData(p.UserAcc.Character2ID).ToArray());
                            p.Send(tmp);

                            //p.State = PlayerState.Connected_CharacterSelection;
                            p.Send(new SendPacket(Packet.FromFormat("bb", 35, 11)));

                        } break;
                    case 1:
                        {
                            p.Send(new SendPacket(Packet.FromFormat("bb", 63, 2)));
                            p.Send(new SendPacket(Packet.FromFormat("bb", 1, 6)));
                        } break;
                    case 2:
                        {
                            //if (status != null) status("Server", "Already logged in. ( " + p.UserName + " )");
                            p.Send(new SendPacket(Packet.FromFormat("bb", 63, 2)));
                            p.Send(new SendPacket(Packet.FromFormat("bb", 0, 19)));
                        } break;
                    case 3:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            p.Send(new SendPacket(Packet.FromFormat("bb", 0, 17)));
                            p.Send(sp);
                        } break;
                    case 4:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            p.Send(new SendPacket(Packet.FromFormat("bb", 0, 65)));
                            p.Send(sp);
                        } break;
                    case 5:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            p.Send(new SendPacket(Packet.FromFormat("bb", 1, 7)));
                            p.Send(sp);
                        } break;
                    case 6:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            p.Send(new SendPacket(Packet.FromFormat("bb", 0, 79)));
                            p.Send(sp);
                        } break;
                }
                #endregion
            }
            catch (Exception t) { throw; }
        }

        #region wlo methods
        //byte[] Get_63_1Data(Character player, byte slot)
        //{
        //    if (player == null)
        //        return null;
        //    Packet temp = new Packet();
        //    temp.Pack8(slot);// data[at] = slot; at++;//PackSend->Pack(1);
        //    temp.PackString(player.CharName);// data[at] = nameLen; at++;
        //    temp.Pack8((byte)player.Level);// data[at] = level; at++;//	PackSend->Pack(tmp1.level);					// Level 
        //    temp.Pack8((byte)player.Element);// data[at] = element; at++;//	PackSend->Pack(3);  					// element
        //    temp.Pack32((uint)player.FullHP);// putDWord(maxHP, data + at); at += 4;//	PackSend->Pack(tmp1.maxHP); 			// max hp
        //    temp.Pack32((uint)player.CurHP);// putDWord(curHP, data + at); at += 4;//	PackSend->Pack(tmp1.curHP); 			// cur hp
        //    temp.Pack32((uint)player.FullSP);// putDWord(maxSP, data + at); at += 4;//	PackSend->Pack(tmp1.maxSP); 			// max sp
        //    temp.Pack32((uint)player.CurSP);// putDWord(curSP, data + at); at += 4;//	PackSend->Pack(tmp1.curSP); 			// cur sp
        //    temp.Pack32((uint)player.TotalExp);// putDWord(experience, data + at); at += 4;//	PackSend->Pack(tmp1.exp);			// exp
        //    temp.Pack32(player.Gold);// putDWord(gold, data + at); at += 4;//	PackSend->Pack(tmp1.gold); 			// gold
        //    temp.Pack8((byte)player.Body);// data[at] = body; at++;//	PackSend->Pack(tmp1.body); 					// body style
        //    temp.Pack8(0);
        //    temp.Pack8(player.Head);
        //    temp.Pack8(0);// data[at] = 0; data[at + 1] = head; data[at + 2] = 0; at += 3;//	PackSend->Pack(tmp1.hair,3);// hair style
        //    temp.Pack16(player.HairColor);// putDWord(color1, data + at); at += 4;//	PackSend->Pack(tmp1.colors1);
        //    temp.Pack16(player.SkinColor);
        //    temp.Pack16(player.ClothingColor);
        //    temp.Pack16(player.EyeColor);
        //    temp.PackBool(player.Reborn);
        //    temp.Pack8((byte)player.Job);// data[at] = rebirth; data[at + 1] = job; at += 2;//PackSend->Pack(tmp1.rebirth);PackSend->Pack(tmp1.rebirthJob); 				// rebirth flag, job skill
        //    for (byte a = 1; a < 7; a++)
        //        temp.Pack16(player[a].ItemID);

        //    if (temp.Count < 1) return null;
        //    return temp.Buffer.ToArray();
        //}

        //void NormalLog(Player c)
        //{
        //    c.BlockSave = false;
        //    //c.CharacterState = GameCharacter.PlayerState.Logging_In;
        //    //a connection request was recieved
        //   // c.DataOut = SendType.Multi;
        //    SendPacket p = new SendPacket();
        //    p.Pack(new byte[] { 20, 8 });
        //    c.Send(p);

        //    Send_24_5(c, 183);
        //    Send_24_5(c, 53);
        //    Send_24_5(c, 52);
        //    Send_24_5(c, 54);
        //    Send_70_1(ref c, 23, "Something", 194);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 20, 33 });
        //    p.Pack(0);
        //    c.Send(p);
        //    //--------------------------Player PreInfo
        //    c.Send8_1();
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 14, 13 });
        //    p.Pack(3);
        //    c.Send(p);
        //    //------------------------Im Mall List
        //    //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 75, 8 });
        //    p.Pack(0);
        //    c.Send(p);
        //    //------------------------
        //    c.Send_3_Me();
        //    cGlobal.WLO_World.SendCurrentPlayers(c);
        //    cGlobal.gGameDataBase.LoadFinalData(ref c);
        //    //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Frederico"),true);//,31038);
        //    //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Niss"),true);//,31026);
        //    c.Send(c.Pets.PetlistData);
        //    c.Pets.SendPetlistStatData();            
        //    c.Pets.Bring_into_Battle(1);
        //    //send sidebar
        //    c.Send_5_3();
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 23, 5 });
        //    p.Pack(c.Inv._23_5Data);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 23, 11 });
        //    p.Pack(c.Eqs._23_11Data);
        //    c.Send(p);
        //    if (c.Pets.BattlePet != null)
        //    {
        //        SendPacket tmp = new SendPacket();
        //        tmp.Pack(new byte[] { 19, 4 });
        //        tmp.Pack(c.Pets.BattlePet.ID);
        //        c.Send(tmp);
        //    }
        //    SendPacket g = new SendPacket();
        //    //    g.Pack(new byte[]{(24, 6);
        //    //    g.Pack(new byte[] { 001, 008, 047, 001, 002, 244, 050, 001, 003, 012, 043, 001 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    //    g = new SPacket();
        //    //    g.Pack(new byte[]{(53, 10);
        //    //    g.Pack(new byte[] { 032, 164, 036, 002, 037, 240, 038, 041, 058, 048, 083, 015 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    //    g = new SPacket();
        //    //    g.Pack(new byte[]{(26, 7);
        //    //    g.Pack(new byte[] { 001, 002, 002, 128, 003, 002, 004, 128 ,008,
        //    //                066, 009, 096, 010, 008, 011, 010 ,013, 001 });
        //    //    g.SetSize();
        //    //    Send(g);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 26, 4 });
        //    p.Pack(c.Gold);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 33, 2 });
        //    p.Pack(c.Settings.Data);
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
        //    if (!cGlobal.WLO_World.onTelePort(0,tmp2,ref c))
        //    {
        //        p = new SendPacket(true);
        //        p.Pack(new byte[] { 0, 7 });
        //        c.Send(p);
                
        //        return;
        //    }


        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 15 });
        //    p.Pack(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 62, 53 });
        //    p.Pack(2);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 21 });
        //    p.Pack((byte)c.Slot);//hmmmmm
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 11 });
        //    p.Pack(15085);//hmmmmm
        //    p.Pack(5000);
        //    c.Send(p);
        //    //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0,         

        //    //---------------------------------

        //    //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items

        //    //--------------------------------------
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 14 });
        //    p.Pack(2);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 16 });
        //    p.Pack(2);
        //    c.Send(p);
        //    var time = DateTime.Now.ToOADate();
        //    Send_23_140(ref c, 3, time);
        //    Send_25_44(ref c, 2, time);
        //    //g.ac23.Send_106(1, 1);
        //    Send_SingleByte_AC(ref c, 23, 160, (byte)3);
        //    Send_SingleByte_AC(ref c, 75, 7, (byte)1);
        //    Send_57("Welcome to the  WLO 4 EVER Community Server :! Enjoy !!", ref c);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 69, 1 });
        //    p.Pack(71);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 20, 60 });
        //    p.Pack(1);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 66, 1 });
        //    p.Pack(new byte[] { 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 });
        //    c.Send(p);
        //    for (byte a = 1; a < 11; a++)
        //        Send_5_13(ref c, a, 0);
        //    for (byte a = 1; a < 11; a++)
        //        Send_5_24(ref c, a, 0);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 23, 162 });
        //    p.Pack(2);
        //    p.Pack(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 26, 10 });
        //    p.Pack(0);
        //    c.Send(p);
        //    Send_SingleByte_AC(ref c, 23, 204, (ushort)1);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 23, 208 });
        //    p.Pack(2);
        //    p.Pack(3);
        //    p.Pack(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 23, 208 });
        //    p.Pack(2);
        //    p.Pack(4);
        //    p.Pack(0);
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 1, 11 });
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 15, 19 });
        //    p.Pack(new byte[] { 4, 6, 9, 94 });
        //    c.Send(p);

        //    //--------------------------------
        //    /*p = new SendPacket();
        //    p.Pack(new byte[]{(20, 33);
        //    p.Pack(0);
        //    p.SetSize();
        //    c.Send(p);*/
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 54 });
        //    p.Pack(
        //    new byte[] {
        //         89,2,2,90,2,1,91,2,1,189,2,2,190,2,1,191,2,1});
        //    c.Send(p);
        //    SendPacket im = new SendPacket();
        //    im.Pack(new byte[] { 35, 4 });
        //    im.Pack(0);
        //    im.Pack(0);
        //    im.Pack(0);
        //    im.Pack(0);
        //    c.Send(im);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 90, 1 });
        //    p.Pack(new byte[] { 000, 2, 2, 3 });
        //    c.Send(p);
        //    p = new SendPacket();
        //    p.Pack(new byte[] { 5, 4 });
        //    c.Send(p);
        //    c.DataOut = SendType.Normal;
        //    //c.CharacterState = GameCharacter.PlayerState.inMap;
        //    //---------------------------------
        //}


        void Send_SingleByte_AC(ref Player player, byte ac, byte subac, object byteval)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { ac, subac });
            //if (byteval is byte)
            //{
            //    p.Pack((byte)byteval);
            //}
            //else if (byteval is ushort)
            //{
            //    p.Pack((ushort)byteval);
            //}
            //else if (byteval is UInt32)
            //{
            //    p.Pack((UInt32)byteval);
            //}
            //player.Send(p);

        }
        void Send_SingleByte_AC(ref Player player, byte ac, object byteval)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { ac });
            //if (byteval is byte)
            //{
            //    p.Pack((byte)byteval);
            //}
            //else if (byteval is ushort)
            //{
            //    p.Pack((ushort)byteval);
            //}
            //player.Send(p);

        }
        void Send_57(string text, ref Player o) //sends sytem prompt message
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 23, 57 });
            //p.Pack(0);
            //p.PackNString(text);
            //o.Send(p);
        }
        void Send_24_5(Player player, byte value)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 24, 5 });
            //p.Pack(value);
            //p.Pack(0);
            //player.Send(p);

        }
        void Send_70_1(ref Player player, byte value, string name, UInt16 id)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 70, 1 });
            //p.Pack(value);
            //p.Pack(name);
            //p.Pack(id);
            //p.Pack(0);
            //player.Send(p);

        }
        void Send_23_140(ref Player player, byte val, Double t)//time related
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 23, 140 });
            //p.Pack(val);
            //p.Pack((ulong)t);
            //player.Send(p);
        }
        void Send_25_44(ref Player player, byte val, double v)//time related
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 25, 44 });
            //p.Pack(val);
            //p.Pack((ulong)v);
            //player.Send(p);
        }
        void Send_5_13(ref Player player, byte value, UInt16 wVal)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 5, 13 });
            //p.Pack(value);
            //p.Pack(wVal);
            //player.Send(p);
        }
        void Send_5_24(ref Player player, byte Val, UInt16 wVal)
        {
            //SendPacket p = new SendPacket();
            //p.Pack(new byte[] { 5, 24 });
            //p.Pack(Val);
            //p.Pack(wVal);
            //player.Send(p);
        }
        #endregion
    }
}
