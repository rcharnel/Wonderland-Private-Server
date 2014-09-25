using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC63:AC
    {
        public override int ID { get { return 63; } }

        public override void ProcessPkt(ref Player p, RecvPacket r)
        {
            switch (r.B)
            {
                case 0:
                case 3: Recv3(ref p, r); break;
                case 2: Recv2(ref p, r); break;
                case 4: Recv4(ref p, r); break;
                default: Utilities.LogServices.Log("AC "+r.A+","+r.B+" has not been coded", Utilities.LogType.NET); break;
            }

        }

        void Recv3(ref Player p, RecvPacket r)
        {
            try
            {
                cGlobal.gCharacterDataBase.unLockName(p.CharacterName);
                p.UserName = "";
                p.DataBaseID = 0;
            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }

        void Recv2(ref Player p, RecvPacket e)
        {
            try
            {
                byte charNum = e.Unpack8(2);

                if ((charNum < 1) || (charNum > 2))//by userid
                {
                    p.UserName = "";
                    p.DataBaseID = 0;
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 0, 32 });
                    p.Send(tmp);
                    return;
                }
                p.SetCharSlot(charNum);

                if ((p.m_charids[charNum] == 0 || cGlobal.gCharacterDataBase.GetCharacterData(p.m_charids[charNum]) == null)) // char is not created
                {
                    #region Create Character
                   cGlobal.gUserDataBase.Update_Player_ID(p.DataBaseID, p.ID, charNum);
                    p.State = PlayerState.Connected_CharacterCreation;
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 1, 3 });
                    tmp.PackBoolean((!string.IsNullOrEmpty(p.Cipher)));
                    p.Send(tmp);
                    #endregion
                }
                else
                {
                    #region Login
                    cGlobal.gCharacterDataBase.GetCharacterData(p.m_charids[charNum], ref p);
                    //SendPacket tmp = new SendPacket();
                    //tmp.PackArray(new byte[] { 63, 2 });
                    //tmp.Pack32(p.UserID);
                    //p.Send(tmp);
                    NormalLog(p);
                    #endregion
                }
            }
            catch (Exception t) { Utilities.LogServices.Log(t); throw; }
        }

        void Recv4(ref Player p, RecvPacket r)
        {
            try
            {
                int loginState = 0; //0-good login  1-bad un/pw  2-dup log 3-wrong version 4-need update

                //sending username and password
                int at = 2;

                string name = r.UnpackChar(at); at += name.Length + 1;
                string password = r.UnpackChar(at); at += password.Length + 1;
                name = name.ToLower();

                string[] userdata = new string[1];//data of user

                UInt16 version = r.Unpack16(at); at += 2;
                byte lcLen = r.Data[at]; at++;
                byte key = r.Data[at]; at++;
                char[] lCode = new char[20];
                Array.Copy(r.Data.ToArray(), at, lCode, 0, lcLen);
                for (int n = 0; n < lcLen; n++)
                    lCode[n] = (char)((byte)lCode[n] ^ (byte)key);

                if ((name.Length < 4) || (name.Length > 14))
                {
                    loginState = 1;
                }
                else if ((password.Length < 4) || (password.Length > 14))
                {
                    loginState = 1;
                }

                else if (version < 1096)
                { //bad aloign version
                    loginState = 3;
                }
                else if ((lcLen < 2) || (lcLen > 15))
                { //bad login code length
                    loginState = 4;
                }

                //at this point we have no use, and only an empty cCharacter object
                //check to see if we are still in the clear for loggin ing
                if (loginState == 0)
                {
                    //should have correct version if at this point

                    uint userid = 0;
                    //check if user exist on wloforever
                    #region Validate Account
                        userid =  cGlobal.gUserDataBase.isValidAccount(name, password);
                        if (userid != 0)
                        {
                            if (cGlobal.WLO_World.isOnline(userid))
                            {
                                //TODO implement Disconnect
                                loginState = 2;
                                cGlobal.WLO_World.DisconnectPlayer(userid);
                            }
                            else
                            {
                                userdata =  cGlobal.gUserDataBase.GetUserData(userid, password);
                                if (userdata != null)
                                {
                                    p.DataBaseID = userid;
                                    p.UserName = userdata[0];
                                    p.Cipher = userdata[3];
                                    p.m_gmlvl = int.Parse(userdata[4]);

                                }
                                else
                                    loginState = 1;
                            }
                        }
                        else
                            loginState = 1;
                    #endregion
                }
                if (userdata != null)
                    if (userdata.Length != 6)
                        loginState = 1;
                    //else if (userdata.Length == 6 && userdata[4].ToString() == "0" && myhost.GameWorld.ServerStatus == ServerMode.TestMode)
                    //    loginState = 6;
                #region Result of Login State
                // here we do the results of loginstate
                switch (loginState)
                {
                    case 0:
                        {
                            uint char1 = 0;
                            uint char2 = char1;
                            Character charsrc1 = null, charsrc2 = null;

                            uint.TryParse(userdata[1], out char1);
                            uint.TryParse(userdata[2], out char2);
                            p.m_charids[1] = char1;
                            p.m_charids[2] = char2;

                            SendPacket tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 63, 2 });
                            tmp.Pack32(p.UserID); 
                            p.Send(tmp);
                            if (char1 > 0)
                                charsrc1 = cGlobal.gCharacterDataBase.GetCharacterData(char1);
                            if (char2 > 0)
                                charsrc2 = cGlobal.gCharacterDataBase.GetCharacterData(char2);
                            byte[] data1 = null;
                            byte[] data2 = null;
                            if (p.m_charids[1] != 0) { data1 = Get_63_1Data(charsrc1, 1); }
                            if (p.m_charids[2] != 0) { data2 = Get_63_1Data(charsrc2, 2); }

                            tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 63, 1 });
                            if (data1 != null) tmp.PackArray(data1);
                            if (data2 != null) tmp.PackArray(data2);
                            p.Send(tmp);
                            p.State = PlayerState.Connected_CharacterSelection;
                            tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 35, 11 });
                            p.Send(tmp);

                        } break;
                    case 1:
                        {
                            SendPacket tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 63, 2 });
                            tmp.Pack32(0);
                            p.Send(tmp);
                            tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 1, 6 });
                            p.Send(tmp);
                        } break;
                    case 2:
                        {
                            //if (status != null) status("Server", "Already logged in. ( " + p.UserName + " )");
                            SendPacket tmp = new SendPacket();
                            tmp.PackArray(new byte[] { 63, 2 });
                            tmp.Pack32(p.ID);
                            p.Send(tmp);
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            sp.PackArray(new byte[] { 0, 19 });//PackSend->Header(63,2);
                            p.Send(sp);
                        } break;
                    case 3:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            sp.PackArray(new byte[] { 0, 17 });//PackSend->Header(63,2);
                            p.Send(sp);
                        } break;
                    case 4:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            sp.PackArray(new byte[] { 0, 65 });//PackSend->Header(63,2);
                            p.Send(sp);
                        } break;
                    case 5:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            sp.PackArray(new byte[] { 1, 7 });//PackSend->Header(63,2);
                            p.Send(sp);
                        } break;
                    case 6:
                        {
                            SendPacket sp = new SendPacket();// PSENDPACKET PackSend = new SENDPACKET;
                            //PackSend->Clear();
                            sp.PackArray(new byte[] { 0, 79 });//PackSend->Header(63,2);
                            p.Send(sp);
                        } break;
                }
                #endregion
            }
            catch (Exception t) {Utilities.LogServices.Log(t); throw; }
        }

        #region wlo methods
        byte[] Get_63_1Data(Character player, byte slot)
        {
            if (player == null) return null;
            Packet temp = new Packet();
            temp.Pack8(slot);// data[at] = slot; at++;//PackSend->Pack8(1);
            temp.PackString(player.CharacterName);// data[at] = nameLen; at++;
            temp.Pack8((byte)player.Level);// data[at] = level; at++;//	PackSend->Pack8(tmp1.level);					// Level 
            temp.Pack8((byte)player.Element);// data[at] = element; at++;//	PackSend->Pack8(3);  					// element
            temp.Pack32((uint)player.FullHP);// putDWord(maxHP, data + at); at += 4;//	PackSend->Pack32(tmp1.maxHP); 			// max hp
            temp.Pack32((uint)player.CurHP);// putDWord(curHP, data + at); at += 4;//	PackSend->Pack32(tmp1.curHP); 			// cur hp
            temp.Pack32((uint)player.FullSP);// putDWord(maxSP, data + at); at += 4;//	PackSend->Pack32(tmp1.maxSP); 			// max sp
            temp.Pack32((uint)player.CurSP);// putDWord(curSP, data + at); at += 4;//	PackSend->Pack32(tmp1.curSP); 			// cur sp
            temp.Pack32((uint)player.TotalExp);// putDWord(experience, data + at); at += 4;//	PackSend->Pack32(tmp1.exp);			// exp
            temp.Pack32(player.Gold);// putDWord(gold, data + at); at += 4;//	PackSend->Pack32(tmp1.gold); 			// gold
            temp.Pack8((byte)player.Body);// data[at] = body; at++;//	PackSend->Pack8(tmp1.body); 					// body style
            temp.Pack8(0);
            temp.Pack8(player.Head);
            temp.Pack8(0);// data[at] = 0; data[at + 1] = head; data[at + 2] = 0; at += 3;//	PackSend->PackArray(tmp1.hair,3);// hair style
            temp.Pack16(player.HairColor);// putDWord(color1, data + at); at += 4;//	PackSend->Pack32(tmp1.colors1);
            temp.Pack16(player.SkinColor);
            temp.Pack16(player.ClothingColor);
            temp.Pack16(player.EyeColor);
            temp.PackBoolean(player.Reborn);
            temp.Pack8((byte)player.Job);// data[at] = rebirth; data[at + 1] = job; at += 2;//PackSend->Pack8(tmp1.rebirth);PackSend->Pack8(tmp1.rebirthJob); 				// rebirth flag, job skill
            for (byte a = 1; a < 7; a++)
                temp.Pack16(player[a].ItemID);

            if (temp.Data.Count < 1) return null;
            return temp.Data.ToArray();
        }
        void NormalLog(Player c)
        {
            c.BlockSave = false;
            //c.CharacterState = GameCharacter.PlayerState.Logging_In;
            //a connection request was recieved
           // c.DataOut = SendType.Multi;
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 20, 8 });
            c.Send(p);

            Send_24_5(c, 183);
            Send_24_5(c, 53);
            Send_24_5(c, 52);
            Send_24_5(c, 54);
            Send_70_1(ref c, 23, "Something", 194);
            p = new SendPacket();
            p.PackArray(new byte[] { 20, 33 });
            p.Pack8(0);
            c.Send(p);
            //--------------------------Player PreInfo
            c.Send8_1();
            p = new SendPacket();
            p.PackArray(new byte[] { 14, 13 });
            p.Pack8(3);
            c.Send(p);
            //------------------------Im Mall List
            //g.ac75.Send_1(g.gImMall_Manager.Get_75IM);
            p = new SendPacket();
            p.PackArray(new byte[] { 75, 8 });
            p.Pack16(0);
            c.Send(p);
            //------------------------
            c.Send_3_Me();
            cGlobal.WLO_World.SendCurrentPlayers(c);
            cGlobal.gGameDataBase.LoadFinalData(ref c);
            //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Frederico"),true);//,31038);
            //c.Pets.RecievePet(myhost.NpcDataBase.GetNpc("Niss"),true);//,31026);
            c.Send(c.Pets.PetlistData);
            c.Pets.SendPetlistStatData();            
            c.Pets.Bring_into_Battle(1);
            //send sidebar
            c.Send_5_3();
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 5 });
            p.PackArray(c.Inv._23_5Data);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 11 });
            p.PackArray(c.Eqs._23_11Data);
            c.Send(p);
            if (c.Pets.BattlePet != null)
            {
                SendPacket tmp = new SendPacket();
                tmp.PackArray(new byte[] { 19, 4 });
                tmp.Pack32(c.Pets.BattlePet.ID);
                c.Send(tmp);
            }
            SendPacket g = new SendPacket();
            //    g.PackArray(new byte[]{(24, 6);
            //    g.PackArray(new byte[] { 001, 008, 047, 001, 002, 244, 050, 001, 003, 012, 043, 001 });
            //    g.SetSize();
            //    Send(g);
            //    g = new SPacket();
            //    g.PackArray(new byte[]{(53, 10);
            //    g.PackArray(new byte[] { 032, 164, 036, 002, 037, 240, 038, 041, 058, 048, 083, 015 });
            //    g.SetSize();
            //    Send(g);
            //    g = new SPacket();
            //    g.PackArray(new byte[]{(26, 7);
            //    g.PackArray(new byte[] { 001, 002, 002, 128, 003, 002, 004, 128 ,008,
            //                066, 009, 096, 010, 008, 011, 010 ,013, 001 });
            //    g.SetSize();
            //    Send(g);
            p = new SendPacket();
            p.PackArray(new byte[] { 26, 4 });
            p.Pack32(c.Gold);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 33, 2 });
            p.PackArray(c.Settings.Data);
            c.Send(p);
            c.SendFriendList();

            //pets
            //-----------------------------------   
            //---------Warp Info---------------------------------------------------
            //put me in my maps list
            if (!cGlobal.WLO_World.TransferinGame(c.LoginMap, ref c))
            {
                p = new SendPacket(true);
                p.PackArray(new byte[] { 0, 7 });
                c.Send(p);
                
                return;
            }


            p = new SendPacket();
            p.PackArray(new byte[] { 5, 15 });
            p.Pack8(0);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 62, 53 });
            p.Pack16(2);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 5, 21 });
            p.Pack8((byte)c.Slot);//hmmmmm
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 5, 11 });
            p.Pack32(15085);//hmmmmm
            p.Pack16(5000);
            c.Send(p);
            //g.ac5.Send_11(15085, 0);//244, 68, 8, 0, 5, 11, 237, 58, 0, 0, 0, 0,         

            //---------------------------------

            //g.ac62.Send_4(g.packet.cCharacter.cCharacterID); //tent items

            //--------------------------------------
            p = new SendPacket();
            p.PackArray(new byte[] { 5, 14 });
            p.Pack8(2);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 5, 16 });
            p.Pack8(2);
            c.Send(p);
            var time = DateTime.Now.ToOADate();
            Send_23_140(ref c, 3, time);
            Send_25_44(ref c, 2, time);
            //g.ac23.Send_106(1, 1);
            Send_SingleByte_AC(ref c, 23, 160, (byte)3);
            Send_SingleByte_AC(ref c, 75, 7, (byte)1);
            Send_57("Welcome to the  WLO 4 EVER Community Server :! Enjoy !!", ref c);
            p = new SendPacket();
            p.PackArray(new byte[] { 69, 1 });
            p.Pack8(71);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 20, 60 });
            p.Pack8(1);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 66, 1 });
            p.PackArray(new byte[] { 001, 012, 043, 000, 000, 000, 000, 000, 000, 000, 000 });
            c.Send(p);
            for (byte a = 1; a < 11; a++)
                Send_5_13(ref c, a, 0);
            for (byte a = 1; a < 11; a++)
                Send_5_24(ref c, a, 0);
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 162 });
            p.Pack8(2);
            p.Pack16(0);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 26, 10 });
            p.Pack32(0);
            c.Send(p);
            Send_SingleByte_AC(ref c, 23, 204, (ushort)1);
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 208 });
            p.Pack8(2);
            p.Pack8(3);
            p.Pack32(0);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 23, 208 });
            p.Pack8(2);
            p.Pack8(4);
            p.Pack32(0);
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 1, 11 });
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 15, 19 });
            p.PackArray(new byte[] { 4, 6, 9, 94 });
            c.Send(p);

            //--------------------------------
            /*p = new SendPacket();
            p.PackArray(new byte[]{(20, 33);
            p.Pack8(0);
            p.SetSize();
            c.Send(p);*/
            p = new SendPacket();
            p.PackArray(new byte[] { 54 });
            p.PackArray(
            new byte[] {
                 89,2,2,90,2,1,91,2,1,189,2,2,190,2,1,191,2,1});
            c.Send(p);
            SendPacket im = new SendPacket();
            im.PackArray(new byte[] { 35, 4 });
            im.Pack32(0);
            im.Pack32(0);
            im.Pack32(0);
            im.Pack32(0);
            c.Send(im);
            p = new SendPacket();
            p.PackArray(new byte[] { 90, 1 });
            p.PackArray(new byte[] { 000, 2, 2, 3 });
            c.Send(p);
            p = new SendPacket();
            p.PackArray(new byte[] { 5, 4 });
            c.Send(p);
            c.DataOut = SendType.Normal;
            //c.CharacterState = GameCharacter.PlayerState.inMap;
            //---------------------------------
        }


        void Send_SingleByte_AC(ref Player player, byte ac, byte subac, object byteval)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { ac, subac });
            if (byteval is byte)
            {
                p.Pack8((byte)byteval);
            }
            else if (byteval is ushort)
            {
                p.Pack16((ushort)byteval);
            }
            else if (byteval is UInt32)
            {
                p.Pack32((UInt32)byteval);
            }
            player.Send(p);

        }
        void Send_SingleByte_AC(ref Player player, byte ac, object byteval)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { ac });
            if (byteval is byte)
            {
                p.Pack8((byte)byteval);
            }
            else if (byteval is ushort)
            {
                p.Pack16((ushort)byteval);
            }
            player.Send(p);

        }
        void Send_57(string text, ref Player o) //sends sytem prompt message
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 57 });
            p.Pack8(0);
            p.PackNString(text);
            o.Send(p);
        }
        void Send_24_5(Player player, byte value)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 24, 5 });
            p.Pack8(value);
            p.Pack16(0);
            player.Send(p);

        }
        void Send_70_1(ref Player player, byte value, string name, UInt16 id)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 70, 1 });
            p.Pack8(value);
            p.PackString(name);
            p.Pack16(id);
            p.Pack8(0);
            player.Send(p);

        }
        void Send_23_140(ref Player player, byte val, Double t)//time related
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 23, 140 });
            p.Pack8(val);
            p.Pack64((ulong)t);
            player.Send(p);
        }
        void Send_25_44(ref Player player, byte val, double v)//time related
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 25, 44 });
            p.Pack8(val);
            p.Pack64((ulong)v);
            player.Send(p);
        }
        void Send_5_13(ref Player player, byte value, UInt16 wVal)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 5, 13 });
            p.Pack8(value);
            p.Pack16(wVal);
            player.Send(p);
        }
        void Send_5_24(ref Player player, byte Val, UInt16 wVal)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 5, 24 });
            p.Pack8(Val);
            p.Pack16(wVal);
            player.Send(p);
        }
        #endregion
    }
}
