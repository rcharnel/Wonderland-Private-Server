using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;


namespace DBConnector
{
   public enum DBType
    {
        Undefined,
        Sqlite,
        MySQl,
        SQl,
    }
    public interface IDBConnector
    {
        bool VerifyPassword(string check, string with);
    }

    public sealed class DBOAuth:IDBConnector
    {

        //Not all settings might be used depending on type of DB
        //Server Type 

        readonly DBType ServType = DBType.Sqlite;

        //MySql/SQl Settings
        readonly string User = "";
        readonly string Pass = "";
        readonly string DataBase = "";
        readonly string Port = "";
        readonly string ServerIP = "";
        
        //SQLite Settings
        //Change to location of DB File
        readonly string FileLocation = "C:\\PServerFiles\\WonderlandPServer.s3db";

        //Override File used in debugging 
        readonly string OverrideFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.database.txt";

        public DBOAuth()
        {
            if (System.IO.File.Exists(OverrideFile))
            {

                try
                {
                    string line = "";
                    using (StreamReader file = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.database.txt"))
                        while ((line = file.ReadLine()) != null)
                        {
                            switch (line.Split('|')[0])
                            {
                                case "Type": ServType = (DBType)byte.Parse(line.Split('|')[1]); break;
                                case "User": User = line.Split('|')[1]; break;
                                case "Pass": Pass = line.Split('|')[1]; break;
                                case "DB": DataBase = line.Split('|')[1]; break;
                                case "Port": Port = line.Split('|')[1]; break;
                                case "IP": ServerIP = line.Split('|')[1]; break;
                                case "File": FileLocation = line.Split('|')[1]; break;
                            }
                        }
                }
                catch { }
            }
        }

        public DBType TypeofDB { get { return ServType; } }
        public bool VerifyConnection()
        {
            switch(ServType)
            {
                case DBType.Sqlite:
                    {
                        if (System.IO.File.Exists(FileLocation))
                        {
                            SQLiteConnection cnn = new SQLiteConnection(Connection_String);
                            cnn.Open();
                            cnn.Close();
                            return true;
                        }
                    }break;
            }
            return false;
        }
        
        string Connection_String
        {
            get
            {

                if (ServType == DBType.MySQl)
                    return string.Format("Server = {0}; Port = {1}; Database = {2}; Uid = {3}; Pwd = {4}", ServerIP, Port, DataBase, User, Pass);
                else if (ServType == DBType.Sqlite)
                    return string.Format("Data Source={0};Version=3;", FileLocation);
                else if (ServType == DBType.SQl)
                    return string.Format("Server={0};Database={1};User Id={2};Password={3};", ServerIP, DataBase, User, Pass);
                else
                    return "";
            }

        }

        public DataTable GetDataTable(string query,KeyValuePair<string,string>[] parameters = null)
        {

            DataTable dt = new DataTable();

            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(Connection_String);
                        try
                        {
                            cnn.Open();
                            SQLiteCommand mycommand = new SQLiteCommand(cnn);
                            mycommand.CommandText = query;
                            if (parameters != null)
                                foreach (var t in parameters)
                                    mycommand.Parameters.AddWithValue(t.Key, t.Value);
                            SQLiteDataReader reader = mycommand.ExecuteReader();
                            dt.Load(reader);
                            reader.Close();
                            cnn.Close();
                            return dt;
                        }
                        catch { cnn.Close(); }
                    }break;
            }

            return new DataTable();
        }
        public DataTable GetDataTable(string Table, string where, KeyValuePair<string, string>[] parameters = null)
        {

            DataTable dt = new DataTable();

            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(Connection_String);
                        cnn.Open();
                        SQLiteCommand mycommand = new SQLiteCommand(cnn);
                        mycommand.CommandText = string.Format("select* from {0} where {1}", Table, where);
                        if (parameters != null)
                            foreach (var t in parameters)
                                mycommand.Parameters.AddWithValue(t.Key, t.Value);
                        SQLiteDataReader reader = mycommand.ExecuteReader();
                        dt.Load(reader);
                        reader.Close();
                        cnn.Close();
                    } break;
            }
            return dt;
        }
        public int ExecuteNonQuery(string sql, KeyValuePair<string, string>[] parameters = null)
        {
            int rowsUpdated = 0;
            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(Connection_String);
                        cnn.Open();
                        SQLiteCommand mycommand = new SQLiteCommand(cnn);
                        mycommand.CommandText = sql;
                        if (parameters != null)
                            foreach (var t in parameters)
                                mycommand.Parameters.AddWithValue(t.Key, t.Value);
                        rowsUpdated = mycommand.ExecuteNonQuery();
                        cnn.Close();
                    } break;
            }
            return rowsUpdated;
        }

        //public string ExecuteScalar(string sql)
        //{
        //    SQLiteConnection cnn = new SQLiteConnection(dbConnection);
        //    cnn.Open();
        //    SQLiteCommand mycommand = new SQLiteCommand(cnn);
        //    mycommand.CommandText = sql;
        //    object value = mycommand.ExecuteScalar();
        //    cnn.Close();
        //    if (value != null)
        //    {
        //        return value.ToString();
        //    }
        //    return "";
        //}

        public void Update(String tableName, Dictionary<string, string> data, String where, KeyValuePair<string, string>[] parameters = null)
        {
            String vals = "";
            if (data.Count >= 1)
            {
                foreach (KeyValuePair<String, String> val in data)
                {
                    vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                }
                vals = vals.Substring(0, vals.Length - 1);
            }
            this.ExecuteNonQuery(String.Format("update {0} set {1} where {2};", tableName, vals, where), parameters);

        }

        public void Delete(String tableName, String where, KeyValuePair<string, string>[] parameters = null)
        {
            this.ExecuteNonQuery(String.Format("delete from {0} where {1};", tableName, where, parameters));
        }

        public void Insert(String tableName, Dictionary<String, String> data)
        {
            String columns = "";
            String values = "";

            foreach (KeyValuePair<String, String> val in data)
            {
                columns += String.Format(" {0},", val.Key.ToString());
                values += String.Format(" '{0}',", val.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);

            this.ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
        }

        //public bool ClearDB()
        //{
        //    DataTable tables;
        //    try
        //    {
        //        tables = this.GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
        //        foreach (DataRow table in tables.Rows)
        //        {
        //            this.ClearTable(table["NAME"].ToString());
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        g.LogError(this.ToString(), e.Message);
        //        return false;
        //    }
        //}

        //public bool ClearTable(String table)
        //{
        //    g.Log(cActionType.DatabaseAction, "Clearing " + table);
        //    try
        //    {

        //        this.ExecuteNonQuery(String.Format("delete from {0};", table));
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        g.LogError(this.ToString(), e.Message);
        //        return false;
        //    }
        //}


        /// <summary>
        /// Verify Passwords
        /// </summary>
        /// <param name="check"></param>
        /// <param name="with"></param>
        /// <returns></returns>
        public bool VerifyPassword(string check, string with)
        {
            //here is where you implement your own code to verify Database passwords
            //leave alone if you have no hashing/security invovled with your passwords

            return (check == with);
        }
    }
}
