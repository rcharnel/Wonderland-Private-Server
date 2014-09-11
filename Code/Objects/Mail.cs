using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Objects
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
}
