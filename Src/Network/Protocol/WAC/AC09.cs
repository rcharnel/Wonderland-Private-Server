using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlo.Core;
using Game;

namespace Server.Network.WAC
{
    public class AC09 : WLOAC
    {
        public override int ID
        {
            get
            {
                return 9;
            }
        }
        public override void Process( Game.Player src, SendPacket r)
        {
            switch (r.Unpack8())
            {
                case 1: Recv1( src, r); break;
                case 2: Recv2( src, r); break;
            }
        }

        async void Recv1(Player tp, SendPacket e)
        {
            tp.CharID = (uint)((tp.Slot == 1) ? tp.UserID : tp.UserID + 4500000);
            tp.Body = (BodyStyle)e.Unpack16();
            tp.Head = (byte)e.Unpack16();
            tp.ColorCode1 = BitConverter.ToUInt32(e.Buffer.ToArray(), e.m_nUnpackIndex);
            tp.ColorCode2 = BitConverter.ToUInt32(e.Buffer.ToArray(), e.m_nUnpackIndex + 4);            
            tp.HairColor = e.Unpack16();
            tp.SkinColor = e.Unpack16();
            tp.ClothingColor = e.Unpack16();
            tp.EyeColor = e.Unpack16();
            tp.Element = (Affinity)e.Unpack8(); if (tp.Element == Affinity.Undefined) { tp.Disconnect(); return; }
            int stat = e[e.m_nUnpackIndex];
            tp.SetBaseStat(28, (ushort)e.Unpack8());
            stat += e[e.m_nUnpackIndex];
            tp.SetBaseStat(29, (ushort)e.Unpack8());
            stat += e[e.m_nUnpackIndex];
            tp.SetBaseStat(27, (ushort)e.Unpack8());
            stat += e[e.m_nUnpackIndex];
            tp.SetBaseStat(30, (ushort)e.Unpack8());
            stat += e[e.m_nUnpackIndex];
            tp.SetBaseStat(33, (ushort)e.Unpack8());
            if (stat != 5) { tp.Disconnect(); return; }

            tp.UserAcc.Cipher = (!string.IsNullOrEmpty(tp.UserAcc.Cipher) ? tp.UserAcc.Cipher : e.UnpackString());
            if ((tp.UserAcc.Cipher.Length < 6) || (tp.UserAcc.Cipher.Length > 14))
            {
                DebugSystem.Write(new ExceptionData(new Exception("Cipher key not correct size")));
                tp.Disconnect();
                return;
            }

            tp.SetBeginnerOutfit();
            tp.FillHP(); tp.FillSP();
            tp.Settings.ChannelCode = (ChannelCodeType)31;
            tp.Settings.PKABLE = false;
            tp.Settings.JOINABLE = true;
            tp.Settings.TRADABLE = true;
            tp.TotalExp = 6;
            tp.LoginMap = 60000; //ship map 10017;
            tp.CurX = 602; // ship x 1042;
            tp.CurY = 455; //ship y 1075;
            tp.SetGold(0);
            //tp.Started_Quests.Add(new Quest() { QID = 1 });

            //tp.Info.MySkills.AddSkill(cGlobal.SkillManager.GetSkillByID(15003));
            // cGlobal.GameDB.CreateInventory((sender as WloPlayer));

            cGlobal.gUserDataBase.UpdateUserField(tp.UserID,
               new DbParam((tp.Slot == 1) ? "userchar1ID" : "userchar2ID", tp.CharID),
               new DbParam("usercipher", tp.UserAcc.Cipher));

                if (await cGlobal.gGameDataBase.SaveCharacterInfo(tp, true))
                {
                    tp.Flags.Remove(PlayerFlag.Creating_Character);
                    cGlobal.GameServer.CommenceLogin(tp);
                }
        }
    

        async void Recv2( Player tp, SendPacket e)
        {
            string name = e.UnpackStringN();
            bool resp = false;
            if (!(resp = !((name.Length >= 4) && (name.Length <= 14) && await cGlobal.gGameDataBase.RequestName(ref tp, name))))
                tp.CharName = name;
            tp.SendPacket(SendPacket.FromFormat("bbb", 9, 3, resp));
        }
    }
}
