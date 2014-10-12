using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.DataManagement.DataFiles;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Skilllist
    {
        Player y;
        List<Skill> myskills = new List<Skill>();

        public Skilllist(Player y)
        {
            this.y = y;
        }

        public void LoadSkills(string skill)
        {
            var words = skill.Split('&');
            foreach (string k in words)
            {
                var sec = k.Split(' ');
                if (sec[0] == "none") break;
                //Skill newskill = new Skill(globals.SkillManager.Get_Skill(UInt16.Parse(sec[0])));
                //newskill.Grade = byte.Parse(sec[1]);
                //newskill.Proficiency = byte.Parse(sec[2]);
                //myskills.Add(newskill);
            }
        }
        public string SaveSkills()
        {
            string str = "";
            int ct = 0;
            foreach (Skill h in myskills)
            {
                ct++;
                string sk = "";
                sk += h.SkillID.ToString() + " ";
                sk += h.Grade.ToString() + " ";
                sk += h.Exp.ToString() + " ";
                if (ct < myskills.Count)
                    sk += "&";
                str += sk;
            }
            if (str == "")
                str += "none";
            return str;
        }
        public void RecieveSkill(Skill skill)
        {
            Skill tmp = new Skill(skill);
            myskills.Add(tmp);
            Send_1(110, 1, tmp.SkillID);
        }
        public int Count { get { return myskills.Count; } }
        public void AddSkill(SkillInfo ID)
        {
            Skill tmp = new Skill(ID);
            tmp.Grade = 1;
            tmp.Exp = 1;
            myskills.Add(tmp);
        }
        public void RemSkill(ushort ID)
        {
        }

        public Skill GetSkillByID(ushort id)
        {
            foreach (Skill h in myskills)
                if (h.SkillID == id)
                    return h;
            return null;
        }
        //public byte[] GetSkillData()
        //{
        //    SPacket data = new SPacket();
        //    for (int a = 0; a < myskills.Count; a++)
        //    {
        //        data.AddWord(myskills[a].SkillTableOrder);
        //        data.AddByte(myskills[a].Grade);
        //        data.AddDWord(myskills[a].Proficiency);
        //    }
        //    return data.data.ToArray();
        //}

        //public void CheckforUpdate()
        //{

        //    foreach (Skill h in globals.SkillManager.Get_Acquirable_Skills(y.MyStats.element))
        //    {
        //        /*if (h.hasRequirements(y) && !myskills.Exists(c=>c.SkillID == h.SkillID))
        //        {
        //            RecieveSkill(h);//TODO finish
        //            break;
        //        }*/
        //    }
        //}

        public void Send_1(byte stat, UInt32 ammt, UInt32 skill)
        {
            //SPacket p = new SPacket();
            //p.Header(8, 1);
            //p.AddByte(stat);
            //p.AddByte(1);
            //p.AddDWord(ammt);
            //p.AddDWord(skill);
            //p.SetSize();
            //globals.SendPacket(globals.FindPlayerby_CharacterTemplateID(y.CharacterTemplateID), p);
        }
    }

}
