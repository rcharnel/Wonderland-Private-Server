using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Utilities;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC50:AC
    {
        public override int ID { get { return 50; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1:Recv_1(ref r,p);break;
                default: LogServices.Log(p.A + "," + p.B + " Has not been coded"); break;

            }
        }
        void Recv_1(ref Player r, RecvPacket p) //recieve an attack command
        {
            if (r.BattleScene != null && r.BattleScene.RoundState == Code.Enums.eBattleRoundState.ReadyState)
            {
                BattleAction tmp = new BattleAction();
                tmp.src = r.BattleScene.FindFighter(p.Unpack8(2), p.Unpack8(3));
                tmp.dst = r.BattleScene.FindFighter(p.Unpack8(4), p.Unpack8(5));
                tmp.skill = new DataManagement.DataFiles.Skill();
                var c1 = cGlobal.gSkillManager.Get_Skill((ushort)p.Unpack32(6));
                var c2 = cGlobal.gSkillManager.Get_Skill((ushort)p.Unpack32(6));
                if (c1 != null)
                {
                    tmp.skill = new DataManagement.DataFiles.Skill(c1.GetData());
                    tmp.skill.Grade = c1.Grade;
                    //tmp.skill.Proficiency = c1.Proficiency;
                }
                else
                    tmp.skill = new DataManagement.DataFiles.Skill(c2.GetData());

                tmp.unknownbyte = p.Unpack8(8);
                tmp.unknownbyte2 = p.Unpack8(9);
                r.BattleScene.PLayer_BattleAction(tmp);
            }
        }
        //void Send_1(cFighter src, ushort skill, cFighter dst, bool miss, uint dmg, cCharacter target)
        //{
        //    cSendPacket p = new cSendPacket(g);
        //    p.Header(50, 1);
        //    p.AddWord(17);
        //    p.AddByte(src.gridx); p.AddByte(src.gridy);
        //    p.AddWord(skill);
        //    p.AddByte(0); p.AddByte(1);
        //    p.AddByte(dst.gridx); p.AddByte(dst.gridy);
        //    p.AddByte(1);
        //    p.AddByte(0);
        //    p.AddByte(1);
        //    if (!miss)
        //        p.AddByte(25);
        //    else
        //        p.AddByte(0);
        //    //Second part is damage calculation
        //    p.AddDWord(dmg);
        //    p.AddByte(1);
        //    p.cCharacter = target;
        //    p.SetSize();
        //    p.Send();
        //}
        //public void Send_6(cFighter f, byte val, cCharacter target)
        //{
        //    cSendPacket p = new cSendPacket(g);
        //    p.Header(50, 6);
        //    p.AddByte(f.gridx); p.AddByte(f.gridy);
        //    p.AddByte(val);
        //    p.SetSize();
        //    p.cCharacter = target;
        //    p.Send();
        //}

    }
}
