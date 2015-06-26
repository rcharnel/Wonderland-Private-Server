using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Code
{
    public class User
    {
        readonly object mlock = new object();

        DateTime imupdate; public DateTime last_imUpdate { get { lock (mlock)return imupdate; } set { lock (mlock)imupdate = value; } }
        string username, cipher;
        int databaseID;
        int im;
        int gmlvl; public int GMlvl { private get { return gmlvl; } set { lock (mlock)gmlvl = value; } }
        //public GMStatus GMRank
        //{
        //    get
        //    {
        //        lock (mlock)
        //            return GMStatus.None;
        //    }
        //}
        int character1ID, character2ID;

        public int Character1ID
        {
            get
            {
                lock (mlock) return character1ID;
            }
            set
            {
                lock (mlock) character1ID = value;
            }

        }
        public int Character2ID
        {
            get
            {
                lock (mlock) return character2ID;
            }
            set
            {
                lock (mlock) character2ID = value;
            }

        }
        public string Cipher
        {
            get { lock (mlock)return cipher; }
            set { lock (mlock)cipher = value; }
        }
        public string UserName
        {
            get { lock (mlock)return username; }
            set { lock (mlock)username = value; }
        }
        public int IM
        {
            get { lock (mlock)return im; }
            set { lock (mlock)im = value; }
        }
        public int DataBaseID
        {
            get { lock (mlock)return databaseID; }
            set { lock (mlock)databaseID = value; }
        }

        public void Clear()
        {
            imupdate = new DateTime();
            username = "";
            databaseID = im = GMlvl = character1ID = character2ID = 0;

        }

    }
}
