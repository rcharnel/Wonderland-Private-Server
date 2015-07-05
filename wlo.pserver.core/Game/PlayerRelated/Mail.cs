using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Game.Code.PlayerRelated
{
    public class Mail
    {
        public uint id;
        public string message;
        public string type;
        public double when;
        public bool isSent;
        public uint targetid;
        public void Load(string r)
        {
            var words = r.Split(' ');
            id = (UInt16)(Int64.Parse(words[0]));
            type = (words[4]);
            targetid = (UInt16)(Int64.Parse(words[1]));
            when = (ulong)(Int64.Parse(words[2]));
            isSent = bool.Parse(words[5]);
            message = words[3];
        }
    }

    public class MailManager
    {
        List<Mail> myMail = new List<Mail>();

        Player own;
        public MailManager(Player owner)
        {
            own = owner;
        }

        public void LoadMail(string mail)
        {
            string[] query = mail.Split('&');
            foreach (string n in query)
            {
                if (n != "none")
                {
                    Mail tmp = new Mail();
                    tmp.Load(n);
                    myMail.Add(tmp);
                }
            }
        }
        public string SaveMail()
        {
            string query = "";
            for (int a = 0; a < myMail.Count; a++)
            {
                query += myMail[a].id + " " + myMail[a].message + " " + myMail[a].type + " " + myMail[a].targetid;
                if (a < myMail.Count)
                    query += "&";
            }
            if (query == "")
                query += "none";
            return query;
        }

        public void SendTo(Player t, string msg, double when = 0)
        {
            Mail a = new Mail();
            a.message = msg;
            a.id = own.CharID;
            a.targetid = t.CharID;
            a.type = "send";
            a.when = (when == 0) ? DateTime.Now.ToOADate() : when;

            if (t == null)
                a.isSent = false;
            else
            {
                t.Mail.Recvfrom(own, a.message, a.when);
                a.isSent = true;
            }
            myMail.Add(a);
        }
        public void Recvfrom(Player t, string msg, double when)
        {
            Mail a = new Mail();
            a.message = msg;
            a.id = own.CharID;
            a.targetid = t.CharID;
            a.type = "Recv";
            myMail.Add(a);
            SendPacket p = new SendPacket();
            p.Pack((byte)14);
            p.Pack((byte)1);
            p.Pack(t.CharID);
            p.Pack((ulong)when);
            p.Pack(msg,false);
            p.SetHeader();
            own.SendPacket(p);
        }
        public List<Mail> GetmyMail()
        {
            return myMail;
        }
    }
}
