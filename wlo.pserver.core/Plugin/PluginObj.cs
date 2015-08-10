using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;
using System.ComponentModel;

namespace Plugin
{

    public abstract class PluginObj
    {
        protected PluginHost myhost;
        protected byte[] hash;
        protected System.IO.FileInfo filedata;

        public PluginObj()
        {
        }
        public PluginObj(System.IO.FileInfo src)
        {
            filedata = src;
            hash = new SHA1Managed().ComputeHash(System.IO.File.ReadAllBytes(src.FullName));
        }

        public string FileID { get { return (filedata != null) ? filedata.Name : ""; } }
        public DateTime Lastwrite { get { return (filedata != null) ? filedata.LastWriteTime : new DateTime(); } }
        [Browsable(false)]
        public byte[] HashVal { get { return hash; } }

        public T Copy<T>()
        {
            try
            {
                object[] param = new object[] { myhost, filedata };
                return (T)Activator.CreateInstance(this.GetType(), param);
            }
            catch (Exception y)
            {
               DebugSystem.Write(new ExceptionData(y));
            }
            return default(T);
        }
    }

}
