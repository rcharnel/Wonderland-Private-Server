using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace Wonderland_Private_Server.DataManagement.DataBase
{
    class UserDataBase
    {
        const string DBServer = "UserDataBase";
        DBConnector.DBOAuth DBAssist;

        public UserDataBase()
        {
            DBAssist = new DBConnector.DBOAuth();
        }

        MySqlConnection GenerateConn()
        {
            return new MySqlConnection(DBAssist.MySQLConnection_String);
        }
        public bool Connect()
        {
            MySqlConnection conn = GenerateConn();

            try
            {
                conn.Open();
                Utilities.LogServices.Log(DBServer + " Connected parameters are correct");
                return true;
            }
            catch (Exception r) { Utilities.LogServices.Log(new Exception("Unable to connect to DataBase")); }
            finally { conn.Close(); }
            return false;
        }
        bool isValidAccount(string username)
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;
            MySqlConnection conn = GenerateConn();

            try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); throw;}
                        
                cmd = new MySqlCommand("SELECT * FROM forums_users where username_clean =@user", conn);
                cmd.Parameters.AddWithValue("@user", username);
                try { reader = cmd.ExecuteReader(); }catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
            
            conn.Close();
                src = new DataTable();
                src.Load(reader);
                return (src.Rows.Count > 0);
        }
        public uint isValidAccount(string username, string pass)
        {
            uint rem = 0;
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;
            DataRow[] rows = new DataRow[0];
            MySqlConnection conn = GenerateConn();

            try { conn.Open(); }
            catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

            cmd = new MySqlCommand("SELECT * FROM forums_users where username_clean = @user", conn);
            cmd.Parameters.AddWithValue("@user", username);
            try { reader = cmd.ExecuteReader(); }
            catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
           
            src = new DataTable();
            src.Load(reader);
            if (src.Rows.Count > 0)
            {
                rows = new DataRow[src.Rows.Count];
                src.Rows.CopyTo(rows, 0);

                if (DBAssist.VerifyPassword(pass, rows[0]["user_password"].ToString()))
                    uint.TryParse(rows[0]["user_id"].ToString(), out rem);
                else
                    rem = 0;
            }
            else
                rem = 0;
            conn.Close();
            return rem;
        }
        public bool Update_Player_ID(uint user, UInt32 id, byte slot)
        {
            if (user == 0) return false;

            MySqlCommand cmd = null;
            DataRow[] rows = new DataRow[0];
            MySqlConnection conn = GenerateConn();

             try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); throw;}

                string col = "";
                switch (slot)
                {
                    case 1: { col = "charID_1"; } break;
                    case 2: { col = "charID_2"; } break;
                }
                cmd = new MySqlCommand("UPDATE forums_users SET " + col + " = '" + id + "' where user_id = '" + user + "'", conn);

              try { cmd.ExecuteNonQuery(); }
            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
                Utilities.LogServices.Log(DBServer + " UPDATE CMD=> UPDATE forums_users SET " + col + " = '" + id + "' where user_id = '" + user + "'" + " Successful");

                return true;
        }
        public string[] GetUserData(uint user, string pass)
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;
            DataRow[] rows = new DataRow[0];
            MySqlConnection conn = GenerateConn();

            try{conn.Open();}catch(MySqlException f){Utilities.LogServices.Log(f); return null;}
            
                cmd = new MySqlCommand("SELECT * FROM forums_users WHERE user_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", user);
               try { reader = cmd.ExecuteReader(); }
            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return null; }

               Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM forums_users where user_id = '" + user + "'" + " Successful");
                src = new DataTable();
                src.Load(reader);
                if (src.Rows.Count > 0)
                {
                    rows = new DataRow[src.Rows.Count];
                    src.Rows.CopyTo(rows, 0);
                    if (DBAssist.VerifyPassword(pass, rows[0]["user_password"].ToString()))
                    {
                        string ch = ""; string ch2 = "0";
                        if (rows[0]["char_delete_code"] != DBNull.Value)
                            ch = rows[0]["char_delete_code"].ToString();
                        if (rows[0]["gmlvl"] != DBNull.Value)
                            ch2 = rows[0]["gmlvl"].ToString();

                        return new string[] { rows[0]["username_clean"].ToString(), rows[0]["charID_1"].ToString(), rows[0]["charID_2"].ToString(), ch, ch2, rows[0]["IM"].ToString() };
                    }
                }
            return null;
        }
        public int GetIMPoints(uint user)
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;
            DataRow[] rows = new DataRow[0];
            MySqlConnection conn = GenerateConn();

            try { conn.Open(); }
            catch (MySqlException f) { Utilities.LogServices.Log(f); return 0; }

            cmd = new MySqlCommand("SELECT * FROM forums_users where user_id = '" + user + "'", conn);
            try { reader = cmd.ExecuteReader(); }
            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return 0; }
            conn.Close();
            src = new DataTable();
            src.Load(reader);
            if (src.Rows.Count > 0)
            {
                rows = new DataRow[src.Rows.Count];
                src.Rows.CopyTo(rows, 0);
                Utilities.LogServices.Log("Sql Command success " + cmd.CommandText);
                return int.Parse(rows[0]["IM"].ToString());
            }
            return 0;
        }
        public bool UpdateUser(uint user, string delete = null, object im = null, object char1 = null, object char2 = null)
        {
            if (user == 0) return false;

            MySqlCommand cmd = null;
            DataRow[] rows = new DataRow[0];
            MySqlConnection conn = GenerateConn();

            try { conn.Open(); }
            catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

            Dictionary<string, string> str = new Dictionary<string, string>();
            if (delete != null) str.Add("char_delete_code", delete);
            if (im != null) str.Add("IM", im.ToString());
            if (char1 != null) str.Add("charID_1", char1.ToString());
            if (char2 != null) str.Add("charID_2", char2.ToString());
            string vals = "";
            if (str.Count >= 1)
            {
                foreach (KeyValuePair<String, String> val in str)
                {
                    vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                }
                vals = vals.Substring(0, vals.Length - 1);
            }

            cmd = new MySqlCommand(string.Format("UPDATE forums_users SET {0} where user_id = '" + user + "'", vals), conn);
            try { cmd.ExecuteNonQuery(); }
            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
            conn.Close();
            return true;
        }

    }

}
