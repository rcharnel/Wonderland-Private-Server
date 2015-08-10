using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using Game;
using System.Threading;
using RCLibrary.Core;
using Game.Code;

namespace DataBase
{
    public enum GMStatus
    {
        None,
    }

    

    public sealed class UserDataBase : RCLibrary.Core.DataBase
    {
        readonly object mylock = new object();

        //Used to provide flexibility to alter columns name and match them with the correct value
        public string TableName = "user";
        public string Username_Ref = "username";
        public string Password_Ref = "password";
        public string DataBaseID_Ref = "userID";
        public string CharacterID1_Ref = "character1ID";
        public string CharacterID2_Ref = "character2ID";
        public string IM_Ref = "IM";
        public string Char_Delete_Code_Ref = "char_delete_code";
        public VerifyPassType PassVerification;


        List<User> GOnlineUsers;

        bool shutdown = false;

        public UserDataBase()
        {

        }
        
        public int Count() { return GOnlineUsers.Count; }

        public bool isLoggedin(string user)
        {
            bool resp = (GOnlineUsers.Count(c => c.UserName == user) > 0);
            DebugSystem.Write(DebugItemType.Info_Heavy, "Checking if User '{0}' is Online... [Resp]: {1}", DebugItemType.Info_Heavy, user, resp);
            return resp;
        }

        public override bool VerifyPassword(string check, string with)
        {
            return base.VerifyPassword(check, with);
        }
        
        public override bool VerifySaltedPassword(string password, string salt, string with)
        {
            return (hashMD5(hashMD5(salt) + hashMD5(password))== with);
        }

        public bool Update_Player_ID(uint user, UInt32 id, byte slot)
        {
            if (user == 0) return false;

            string col = "";
            switch (slot)
            {
                case 1: { col = CharacterID1_Ref; } break;
                case 2: { col = CharacterID2_Ref; } break;
            }

            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add(col, id.ToString());

            //try { Update(TableName, cols, UserID_Ref + " = '" + user + "'"); }
            //catch (MySqlException ex) { DebugSystem.Write(ex); return false; }

            return true;
        }

        public bool GetUserData(string user, string pass,out uint userID,out string[] userData)
        {
            DataRow[] rows = new DataRow[0];

            var src = GetDataTable("SELECT * FROM " + TableName + " WHERE " + Username_Ref + " = @id", new DbParam("@id", user));

            if (src.Rows.Count > 0)
            {
                rows = new DataRow[src.Rows.Count];
                src.Rows.CopyTo(rows, 0);

                switch (PassVerification)
                {
                    case VerifyPassType.None:
                         if (VerifyPassword(pass, rows[0][Password_Ref].ToString()))
                        {
                            string ch = "";
                            if (rows[0][Char_Delete_Code_Ref] != DBNull.Value)
                                ch = rows[0][Char_Delete_Code_Ref].ToString();
                            uint.TryParse(rows[0][DataBaseID_Ref].ToString(), out userID);
                            userData = new string[] { rows[0][Username_Ref].ToString(), ch, (rows[0][IM_Ref].ToString() == "") ? "0" : rows[0][IM_Ref].ToString() };
                            return true;
                        } break;
                    //if (BCrypt.Net.BCrypt.Verify(pass, rows[0][Password_Ref].ToString()))
                    //{
                    //    string ch = "0";
                    //    if (rows[0][Char_Delete_Code_Ref] != DBNull.Value)
                    //        ch = rows[0][Char_Delete_Code_Ref].ToString();

                    //    return new string[] { rows[0][Username_Ref].ToString(), ch, (rows[0][IM_Ref].ToString() == "")?"0":rows[0][IM_Ref].ToString()};
                    //}
                    case VerifyPassType.IPBoard_3x:
                        if (VerifySaltedPassword(pass, rows[0]["members_pass_salt"].ToString(), rows[0][Password_Ref].ToString()))
                        {
                            string ch = "";
                            if (rows[0][Char_Delete_Code_Ref] != DBNull.Value)
                                ch = rows[0][Char_Delete_Code_Ref].ToString();
                            uint.TryParse(rows[0][DataBaseID_Ref].ToString(), out userID);
                            userData = new string[] { rows[0][Username_Ref].ToString(), ch, (rows[0][IM_Ref].ToString() == "") ? "0" : rows[0][IM_Ref].ToString() };
                            return true;
                        } break;
                }
            }
            userID = 0;
            userData = null;
            return false;
        }

        public int GetIMPoints(uint user)
        {
            DataTable src = null;
            DataRow[] rows = new DataRow[0];

            try
            {
                src = GetDataTable("SELECT * FROM " + TableName + " where " + DataBaseID_Ref + " = '" + user + "'"
                    );
            }
            catch (MySqlException ex) { DebugSystem.Write(new ExceptionData(ex)); return 0; }

            if (src.Rows.Count > 0)
            {
                rows = new DataRow[src.Rows.Count];
                src.Rows.CopyTo(rows, 0);
                return int.Parse(rows[0][IM_Ref].ToString());
            }
            return 0;
        }

        public bool UpdateUser(uint user, string delete = null, object im = null, object char1 = null, object char2 = null)
        {
            if (user == 0) return false;

            Dictionary<string, string> str = new Dictionary<string, string>();
            if (delete != null) str.Add(Char_Delete_Code_Ref, delete);
            if (im != null) str.Add(IM_Ref, im.ToString());
            if (char1 != null) str.Add(CharacterID1_Ref, char1.ToString());
            if (char2 != null) str.Add(CharacterID2_Ref, char2.ToString());

            //try { Update(TableName, str, UserID_Ref + " = '" + user + "'"); }
            //catch (MySqlException ex) { DebugSystem.Write(new ExceptionData(ex)); return false; }

            return true;
        }

        public bool OnLogin(User usr)
        {
            return true;
        }
        public void OnLogOff(User usr)
        {
        }
    }


}
