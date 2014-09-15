using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace Wonderland_Private_Server.DataManagement.DataBase
{
    public sealed class UserDataBase
    {
        const string DBServer = "UserDataBase";

        //Used to provide alt columns name and match them with the correct value
        const string TableName = "user";

        const string Username_Ref = "username";
        const string Password_Ref = "password";
        const string UserID_Ref = "userID";
        const string CharacterID1_Ref = "character1ID";
        const string CharacterID2_Ref = "character2ID";
        const string IM_Ref = "IM";
        const string GM_Ref = "GM";
        const string Char_Delete_Code_Ref = "char_delete_code";

        public UserDataBase()
        {
        }

        public void VerifySetup()
        {
            //DataBase Setup
            Dictionary<string, string> col = new Dictionary<string, string>();
            col.Add(UserID_Ref, "int/NN/PK");
            col.Add(Username_Ref, "text");
            col.Add(Password_Ref, "text");
            col.Add(CharacterID1_Ref, "int/NN");
            col.Add(CharacterID2_Ref, "int/NN");
            col.Add(IM_Ref, "int/NN");
            col.Add(GM_Ref, "int/NN");
            col.Add(Char_Delete_Code_Ref, "text");

            #region UserDataBase table Verification
            Utilities.LogServices.Log("Checking " + TableName + " table");
        retry:

            if (cGlobal.gDataBaseConnection.GetDataTable("SELECT * FROM "+TableName) != null) goto exist;

            Utilities.LogServices.Log("Setuping up "+TableName+" table");

            string nonsqlite_prikey = "";
            string cmstr = "create table "+TableName+" (";
            
                        foreach (var t in col)
                        {
                            var str = "";
                            var att = t.Value.Split('/');

                            switch(cGlobal.gDataBaseConnection.TypeofDB)
                            {
                                case DBConnector.DBType.MySQl:
                                    {
                                        foreach (var a in att)
                                            switch (a)
                                            {
                                                case "int": str += "int(11) "; break;
                                                case "NN": str += "NOT NULL "; break;
                                                case "PK": nonsqlite_prikey ="PRIMARY KEY ("+t.Key+")"; break;
                                            }
                                    } break;
                                case DBConnector.DBType.Sqlite:
                                    {
                                        if (att.Count(c => c == "pk") > 0 && att.Count(c => c == "NN") > 0)
                                            att = att.Where(c => c != "NN").ToArray();

                                        foreach (var a in att)
                                            switch (a)
                                            {
                                                case "int": str += "INTEGER "; break;
                                                case "NN": str += "NOT NULL "; break;
                                                case "PK": str += "PRIMARY KEY "; break;
                                            }
                                    } break;
                            }
                            
                            cmstr += string.Format("{0} {1},", t.Key, str);
                        }

                            if (nonsqlite_prikey !="")
                                    cmstr += string.Format("{0},", nonsqlite_prikey);

                        cmstr = cmstr.Substring(0, cmstr.Length - 1);
                        if (cGlobal.gDataBaseConnection.TypeofDB == DBConnector.DBType.MySQl)
                            cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                        else if (cGlobal.gDataBaseConnection.TypeofDB == DBConnector.DBType.Sqlite)
                            cmstr += ");";
        

            cGlobal.gDataBaseConnection.ExecuteNonQuery(cmstr);

        exist:

            //table exists verify columns  
            foreach (string h in col.Keys)
            {
                if (cGlobal.gDataBaseConnection.GetDataTable("select " + h + " from "+TableName) == null)
                {
                    Utilities.LogServices.Log("Recreating "+TableName+" table");

                    cGlobal.gDataBaseConnection.ExecuteNonQuery("drop table if exists "+TableName);
                    goto retry;
                }
            }
            #endregion
        }



        //bool isValidAccount(string username)
        //{
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    MySqlConnection conn = GenerateConn();

        //    try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); throw;}
                        
        //        cmd = new MySqlCommand("SELECT * FROM forums_users where username_clean =@user", conn);
        //        cmd.Parameters.AddWithValue("@user", username);
        //        try { reader = cmd.ExecuteReader(); }catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
            
        //    conn.Close();
        //        src = new DataTable();
        //        src.Load(reader);
        //        return (src.Rows.Count > 0);
        //}
        //public uint isValidAccount(string username, string pass)
        //{
        //    uint rem = 0;
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //    cmd = new MySqlCommand("SELECT * FROM forums_users where username_clean = @user", conn);
        //    cmd.Parameters.AddWithValue("@user", username);
        //    try { reader = cmd.ExecuteReader(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
           
        //    src = new DataTable();
        //    src.Load(reader);
        //    if (src.Rows.Count > 0)
        //    {
        //        rows = new DataRow[src.Rows.Count];
        //        src.Rows.CopyTo(rows, 0);

        //        if (DBAssist.VerifyPassword(pass, rows[0]["user_password"].ToString()))
        //            uint.TryParse(rows[0]["user_id"].ToString(), out rem);
        //        else
        //            rem = 0;
        //    }
        //    else
        //        rem = 0;
        //    conn.Close();
        //    return rem;
        //}
        //public bool Update_Player_ID(uint user, UInt32 id, byte slot)
        //{
        //    if (user == 0) return false;

        //    MySqlCommand cmd = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //     try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); throw;}

        //        string col = "";
        //        switch (slot)
        //        {
        //            case 1: { col = "charID_1"; } break;
        //            case 2: { col = "charID_2"; } break;
        //        }
        //        cmd = new MySqlCommand("UPDATE forums_users SET " + col + " = '" + id + "' where user_id = '" + user + "'", conn);

        //      try { cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
        //      Utilities.LogServices.Log(DBServer + " UPDATE CMD=> UPDATE forums_users SET " + col + " = '" + id + "' where user_id = '" + user + "'" + " Successful", "DataBase");

        //        return true;
        //}
        //public string[] GetUserData(uint user, string pass)
        //{
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); return null;}
            
        //        cmd = new MySqlCommand("SELECT * FROM forums_users WHERE user_id = @id", conn);
        //        cmd.Parameters.AddWithValue("@id", user);
        //       try { reader = cmd.ExecuteReader(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return null; }

        //       Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM forums_users where user_id = '" + user + "'" + " Successful", "DataBase");
        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count > 0)
        //        {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //            if (DBAssist.VerifyPassword(pass, rows[0]["user_password"].ToString()))
        //            {
        //                string ch = ""; string ch2 = "0";
        //                if (rows[0]["char_delete_code"] != DBNull.Value)
        //                    ch = rows[0]["char_delete_code"].ToString();
        //                if (rows[0]["gmlvl"] != DBNull.Value)
        //                    ch2 = rows[0]["gmlvl"].ToString();

        //                return new string[] { rows[0]["username_clean"].ToString(), rows[0]["charID_1"].ToString(), rows[0]["charID_2"].ToString(), ch, ch2, rows[0]["IM"].ToString() };
        //            }
        //        }
        //    return null;
        //}
        //public int GetIMPoints(uint user)
        //{
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); return 0; }

        //    cmd = new MySqlCommand("SELECT * FROM forums_users where user_id = '" + user + "'", conn);
        //    try { reader = cmd.ExecuteReader(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return 0; }
        //    conn.Close();
        //    src = new DataTable();
        //    src.Load(reader);
        //    if (src.Rows.Count > 0)
        //    {
        //        rows = new DataRow[src.Rows.Count];
        //        src.Rows.CopyTo(rows, 0);
        //        Utilities.LogServices.Log("Sql Command success " + cmd.CommandText,"DataBase");
        //        return int.Parse(rows[0]["IM"].ToString());
        //    }
        //    return 0;
        //}
        //public bool UpdateUser(uint user, string delete = null, object im = null, object char1 = null, object char2 = null)
        //{
        //    if (user == 0) return false;

        //    MySqlCommand cmd = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //    Dictionary<string, string> str = new Dictionary<string, string>();
        //    if (delete != null) str.Add("char_delete_code", delete);
        //    if (im != null) str.Add("IM", im.ToString());
        //    if (char1 != null) str.Add("charID_1", char1.ToString());
        //    if (char2 != null) str.Add("charID_2", char2.ToString());
        //    string vals = "";
        //    if (str.Count >= 1)
        //    {
        //        foreach (KeyValuePair<String, String> val in str)
        //        {
        //            vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
        //        }
        //        vals = vals.Substring(0, vals.Length - 1);
        //    }

        //    cmd = new MySqlCommand(string.Format("UPDATE forums_users SET {0} where user_id = '" + user + "'", vals), conn);
        //    try { cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
        //    conn.Close();
        //    return true;
        //}

    }

}
