using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnector
{
    public interface IDBConnector
    {
        string MySQLConnection_String { get; }
        bool VerifyPassword(string check, string with);
    }

    public sealed class DBOAuth:IDBConnector
    {
        //Not all settings might be used depending on type of DB
        const string User = "";
        const string Pass = "";
        const string DataBase = "";
        const string Port = "";
        const string ServerIP = "";

        public string MySQLConnection_String { get { return string.Format("Server = {0}; Port = {1}; Database = {2}; Uid = {3}; Pwd = {4}", ServerIP, Port, DataBase, User, Pass); } }

        public bool VerifyPassword(string check, string with)
        {
            //here is where you implement your own code to verify Database passwords

            return false;
        }
    }
}
