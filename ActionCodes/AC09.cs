using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC09:AC
    {
         public override int ID { get { return 09; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref r, p); break;
                default:Utilities.LogServices.Log("AC "+p.A+","+ p.B+" has not been coded"); break;
            }
        }

        void Recv1(ref Player tp, RecvPacket e)
        {
            try
            {
                if (string.IsNullOrEmpty(tp.Cipher))
                {
                    tp.Cipher = e.UnpackChar(20);
                    if ((tp.Cipher.Length < 6) ||
                        (tp.Cipher.Length > 14))
                    {
                        //make sure no save of data by setting values to 0
                        tp.Clear();
                        SendPacket p = new SendPacket();
                        p.PackArray(new byte[] { 0, 30 });
                        tp.Send(p);
                        return;
                    }
                }
            
                tp.Eqs.Element = (ElementType)e.Unpack8(14);
                tp.Eqs.Body = (BodyStyle)e.Unpack8(2);
                tp.Eqs.Head = e.Unpack8(4);
                tp.HairColor = e.Unpack16(6);
                tp.SkinColor = e.Unpack16(8);
                tp.ClothingColor = e.Unpack16(10);
                tp.EyeColor = e.Unpack16(12);
                tp.Eqs.Str = e.Unpack8(15);
                tp.Eqs.Agi = e.Unpack8(16);
                tp.Eqs.Wis = e.Unpack8(17);
                tp.Eqs.Int = e.Unpack8(18);
                tp.Eqs.Con = e.Unpack8(19);
                foreach (cItem u in GetNoobClothes(tp))
                {
                    InvItemCell i = new InvItemCell();
                    i.CopyFrom(u);
                    tp.Eqs.SetEQ((byte)u.Equippped_At, i);
                }
                tp.FillHP(); tp.FillSP();
                tp.Settings.Set(3, 31);
                tp.Settings.Set(1, 2);
                tp.Settings.Set(2, 1);
                tp.Settings.Set(4, 1);
                tp.Eqs.TotalExp = 6;
                tp.LoginMap = 60000; //ship map 10017;
                tp.X = 602; // ship x 1042;
                tp.Y = 455; //ship y 1075;
                tp.SetGold(0);
                //tp.Started_Quests.Add(new Quest() { QID = 1 });

                //tp.Info.MySkills.AddSkill(cGlobal.SkillManager.GetSkillByID(15003));
                //create a character id for this new character
                //tp.Info.ID = tp.UserID + (byte)(tp.Slot - 1);
                // cGlobal.GameDB.CreateInventory((sender as WloPlayer));

                var id = tp.ID;
                if (cGlobal.gCharacterDataBase.WriteNewPlayer(tp.ID, tp))
                {
                   cGlobal.gUserDataBase.Update_Player_ID(tp.DataBaseID, tp.ID, (byte)tp.Slot);
                   cGlobal.gUserDataBase.UpdateUser(tp.DataBaseID, tp.Cipher);
                    NormalLog(tp);
                }
                else 
                    throw new Exception("unable to write player");

                //tp.CharacterState = GameCharacter.PlayerState.Logging_In;
               
            }
            catch (Exception t)
            {
               Utilities.LogServices.Log(t); 
                SendPacket p = new SendPacket();
                p.PackArray(new byte[] { 0, 49 });
                tp.Send(p);
            }


        }
        void Recv2(ref Player tp, RecvPacket e)
        {
            int nameLen = e.Size - 2;
            string name = e.UnpackNChar(2);
            if ((nameLen < 4) || (nameLen > 14) || !cGlobal.gCharacterDataBase.LockName(tp.DataBaseID,name))
            {
                SendPacket p = new SendPacket();
                p.PackArray(new byte[] { 9, 3 });
                p.Pack8(1);
                tp.Send(p);
                return;
            }
            tp.CharacterName = name;
            SendPacket qp = new SendPacket();
            qp.PackArray(new byte[] { 9, 3 });
            qp.Pack8(0);
            tp.Send(qp);
        }

        cItem[] GetNoobClothes(Player p)
        {
            cItem[] y = new cItem[1];
            switch (p.Eqs.Body)
            {
                case BodyStyle.Big_Female:
                    {
                        HairStyle_BigF g = new HairStyle_BigF();
                        switch (Enum.GetName(g.GetType(), (HairStyle_BigF)p.Eqs.Head))
                        {
                            case "Iris":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22005),
                                       cGlobal.gItemManager.GetItem(21006),cGlobal.gItemManager.GetItem(23001),
                                       cGlobal.gItemManager.GetItem(24006)};
                                } break;
                            case "Lique":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21007),cGlobal.gItemManager.GetItem(23002),
                                       cGlobal.gItemManager.GetItem(24007)};
                                } break;
                            case "Maria":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22006),
                                       cGlobal.gItemManager.GetItem(21011),cGlobal.gItemManager.GetItem(10004),
                                       cGlobal.gItemManager.GetItem(24011)};
                                } break;
                            case "Vanessa":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21015),
                                       cGlobal.gItemManager.GetItem(24008)};
                                } break;
                            case "Breillat":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22007),
                                       cGlobal.gItemManager.GetItem(21009),cGlobal.gItemManager.GetItem(10002),
                                       cGlobal.gItemManager.GetItem(24009)};
                                } break;
                            case "Karin":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21015),
                                        cGlobal.gItemManager.GetItem(22008),
                                       cGlobal.gItemManager.GetItem(24015)};
                                } break;
                            case "Konnotsuroko":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(24013),
                                       cGlobal.gItemManager.GetItem(21013),};
                                } break;
                            case "Jessica":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22002),
                                       cGlobal.gItemManager.GetItem(21010),cGlobal.gItemManager.GetItem(10003),
                                       cGlobal.gItemManager.GetItem(24010)};
                                } break;
                        }
                    } break;
                case BodyStyle.Big_Male:
                    {
                        HairStyle_BigM g = new HairStyle_BigM();
                        switch (Enum.GetName(g.GetType(), (HairStyle_BigM)p.Eqs.Head))
                        {
                            case "Daniel":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21004),
                                       cGlobal.gItemManager.GetItem(24004)};
                                } break;
                            case "Sid":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21005),
                                       cGlobal.gItemManager.GetItem(24005)};
                                } break;
                            case "More":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21012),
                                       cGlobal.gItemManager.GetItem(24012)};
                                } break;
                            case "Kurogane":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22009),
                                       cGlobal.gItemManager.GetItem(21014),cGlobal.gItemManager.GetItem(18002),
                                       cGlobal.gItemManager.GetItem(24014)};
                                } break;
                        }
                    } break;
                case BodyStyle.Small_Female:
                    {
                        HairStyle_SmallF g = new HairStyle_SmallF();
                        switch (Enum.GetName(g.GetType(), (HairStyle_SmallF)p.Eqs.Head))
                        {
                            case "Nina":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22003),
                                       cGlobal.gItemManager.GetItem(21002),cGlobal.gItemManager.GetItem(24002)};
                                } break;
                            case "Betty":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(22001),
                                       cGlobal.gItemManager.GetItem(21003),cGlobal.gItemManager.GetItem(24003)};
                                } break;
                        }
                    } break;
                case BodyStyle.Small_Male:
                    {
                        HairStyle_SmallM g = new HairStyle_SmallM();
                        switch (Enum.GetName(g.GetType(), (HairStyle_SmallM)p.Eqs.Head))
                        {
                            case "Rocco":
                                {
                                    y = new cItem[]{ cGlobal.gItemManager.GetItem(21001),
                                       cGlobal.gItemManager.GetItem(24001)};
                                } break;
                        }

                    } break;
            }
            return y;
        }


        #region wlo methods
        byte[] Get_63_1Data(Character player, byte slot)
        {
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
            try
            {
                //c.CharacterState = GameCharacter.PlayerState.Logging_In;
                //a connection request was recieved
                c.DataOut = SendType.Multi;
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
                //Load final Data
               cGlobal.gGameDataBase.LoadFinalData(ref c);
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
            catch (Exception t) {Utilities.LogServices.Log(t); }
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
