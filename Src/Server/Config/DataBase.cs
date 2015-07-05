using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Config
{
    public class DataBaseConfig 
    {
        //return the following information

        //MySql/SQl Settings
        /// <summary>
        /// Username for authencation
        /// </summary>
        public string User;
        /// <summary>
        /// Pasword for authencation
        /// </summary>
        public string Pass;
        /// <summary>
        /// The Database we want to use
        /// </summary>
        public string DataBase;
        /// <summary>
        /// Port we are connecting to
        /// </summary>
        public int Port;
        /// <summary>
        /// IP Address of the Server
        /// </summary>
        public string ServerIP;
        /// <summary>
        /// Type of Server we are connecting to
        /// </summary>
        public RCLibrary.Core.DataBaseTypes Server_Type;
    }
}
