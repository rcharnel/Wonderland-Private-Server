using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;


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


        const DBType ServType = DBType.Undefined;

        //MySql/SQl Settings
        const string User = "";
        const string Pass = "";
        const string DataBase = "";
        const string Port = "";
        const string ServerIP = "";
        
        //SQLite Settings
        const string FileLocation = "";

        public DBType TypeofDB { get { return ServType; } }
        public bool VerifyConnection()
        {
            switch(ServType)
            {
                case DBType.Sqlite: return System.IO.File.Exists(FileLocation);
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

        public DataTable GetDataTable(string query)
        {

            DataTable dt = new DataTable();

            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(FileLocation);
                        cnn.Open();
                        SQLiteCommand mycommand = new SQLiteCommand(cnn);
                        mycommand.CommandText = query;
                        SQLiteDataReader reader = mycommand.ExecuteReader();
                        dt.Load(reader);
                        reader.Close();
                        cnn.Close();
                        return dt;
                    }
            }
            return null;
        }

        public DataTable GetDataTable(string Table, string where)
        {

            DataTable dt = new DataTable();

            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(FileLocation);
                        cnn.Open();
                        SQLiteCommand mycommand = new SQLiteCommand(cnn);
                        mycommand.CommandText = string.Format("select* from {0} where {1}", Table, where);
                        SQLiteDataReader reader = mycommand.ExecuteReader();
                        dt.Load(reader);
                        reader.Close();
                        cnn.Close();
                    } break;
            }
            return dt;
        }
        public int ExecuteNonQuery(string sql)
        {
            int rowsUpdated = 0;
            switch (ServType)
            {
                case DBType.Sqlite:
                    {
                        SQLiteConnection cnn = new SQLiteConnection(FileLocation);
                        cnn.Open();
                        SQLiteCommand mycommand = new SQLiteCommand(cnn);
                        mycommand.CommandText = sql;
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

        //public bool Update(String tableName, Dictionary<string, string> data, String where)
        //{
        //    g.Log(cActionType.DatabaseAction, "Updating " + tableName);
        //    String vals = "";
        //    Boolean returnCode = true;
        //    if (data.Count >= 1)
        //    {
        //        foreach (KeyValuePair<String, String> val in data)
        //        {
        //            vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
        //        }
        //        vals = vals.Substring(0, vals.Length - 1);
        //    }
        //    try
        //    {
        //        this.ExecuteNonQuery(String.Format("update {0} set {1} where {2};", tableName, vals, where));
        //    }
        //    catch (Exception e)
        //    {
        //        g.LogError(this.ToString(), e.Message);
        //        returnCode = false;
        //    }
        //    return returnCode;
        //}

        //public bool Delete(String tableName, String where)
        //{
        //    g.Log(cActionType.DatabaseAction, "Deleting from " + tableName);
        //    Boolean returnCode = true;
        //    try
        //    {
        //        this.ExecuteNonQuery(String.Format("delete from {0} where {1};", tableName, where));
        //    }
        //    catch (Exception e)
        //    {
        //        g.LogError(this.ToString(), e.Message);
        //        returnCode = false;
        //    }
        //    return returnCode;
        //}

        //public bool Insert(String tableName, Dictionary<String, String> data)
        //{
        //    g.Log(cActionType.DatabaseAction, "Inserting into " + tableName);
        //    String columns = "";
        //    String values = "";
        //    Boolean returnCode = true;
        //    foreach (KeyValuePair<String, String> val in data)
        //    {
        //        columns += String.Format(" {0},", val.Key.ToString());
        //        values += String.Format(" '{0}',", val.Value);
        //    }
        //    columns = columns.Substring(0, columns.Length - 1);
        //    values = values.Substring(0, values.Length - 1);
        //    try
        //    {
        //        this.ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
        //    }
        //    catch (Exception e)
        //    {
        //        g.LogError(this.ToString(), e.Message);
        //        returnCode = false;
        //    }
        //    return returnCode;
        //}

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
