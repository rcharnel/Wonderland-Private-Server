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
        
        string username, cipher;
        uint databaseID;
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

        public uint UserID
        {
            get { return DataBaseID + 10000; }
        }

        public uint Character1ID
        {
            get
            {
                return UserID;
            }

        }

        public uint Character2ID
        {
            get
            {
                return UserID + 4500000;
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

        public uint DataBaseID
        {
            get { lock (mlock)return databaseID; }
            set { lock (mlock)databaseID = value; }
        }

        public void Clear()
        {
            username = "";
            databaseID = 0;
            im = 0;
            GMlvl = 0;

        }

    }
}
