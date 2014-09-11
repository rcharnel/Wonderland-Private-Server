using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.DataManagement.DataBase
{
    class GameDataBase
    {
        const string DBServer = "GameDataBase";
        DBConnector.DBOAuth DBAssist;

        public GameDataBase()
        {
            DBAssist = new DBConnector.DBOAuth();
        }
        public bool Can_Connect { get { return (DBAssist == null) ? false : DBAssist.VerifyConnection(); } }
        public void VerifySetup()
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;
        }
        //public void LoadFinalData(ref Player c)
        //{
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); return; }


        //    #region Inventory
        //    cmd = new MySqlCommand("SELECT * FROM inventory where charID = '" + c.ID + "'", conn);
        //        try { reader = cmd.ExecuteReader(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + c.ID + "'" + " Successful");
        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count > 0)
        //        {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //            ushort id;

        //            for (int i = 0; i < rows.Length; i++)
        //            {
        //                id = ushort.Parse(rows[i]["itemID"].ToString());
        //                switch (uint.Parse(rows[i]["storID"].ToString()))
        //                {
        //                    case 0:
        //                        if (id != 0)
        //                        {
        //                            var data = new InvItemCell();
        //                            data.CopyFrom(myhost.ItemDataBase.GetItem(id));
        //                            data.Ammt = byte.Parse(rows[i]["qty"].ToString());
        //                            data.Damage = byte.Parse(rows[i]["dmg"].ToString());
        //                            c.Inv[byte.Parse(rows[i]["pos"].ToString())] = data;
        //                            //rows[i]["socketID"].ToString(), rows[i]["bombID"].ToString(),rows[i]["sewID"].ToString(),rows[i]["dmg"].ToString(),rows[i]["forge"].ToString(), , });
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //        if (!reader.IsClosed) reader.Close();
        //        src = null;
        //    #endregion

        //    #region Tent
        //    #endregion

        //    #region Friends
        //        cmd = new MySqlCommand("SELECT * FROM charactersextdata where charID = '" + c.ID + "'", conn);
        //        try
        //        {
        //            reader = cmd.ExecuteReader();
        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + c.ID + "'" + " Successful");
        //        }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //            src = new DataTable();
        //            src.Load(reader);
        //            if (src.Rows.Count > 0)
        //                c.LoadFriends(src.Rows[0]["Friends"].ToString());

        //        if (!reader.IsClosed) reader.Close();
        //        src = null;
        //    #endregion

        //    #region Mail
        //        cmd = new MySqlCommand("SELECT * FROM charactersextdata where charID = '" + c.ID + "'", conn);
        //        try
        //        {
        //            reader = cmd.ExecuteReader();
        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + c.ID + "'" + " Successful");
        //        }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
              
        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count > 0)
        //        {
        //            foreach(string m in src.Rows[0]["Mail"].ToString().Split('&'))
        //            {
        //                if (m == "none") break;
        //                Mail tmp = new Mail();
        //                tmp.Load(m);
        //                var re = myhost.GameWorld.Find(tmp.targetid);
        //                if (tmp.type == "Send")
        //                {
        //                    if (!tmp.isSent && re != null)
        //                    {
        //                         c.MailBox.Add(tmp);
        //                        re.RecvMailfrom(c,tmp.message,tmp.when);
        //                    }
        //                    else
        //                        c.MailBox.Add(tmp);
        //                }
        //                else
        //                    c.MailBox.Add(tmp);
        //            }
        //        }
        //    #endregion

        //    #region Skills

        //    #endregion

        //    #region Settings
        //        cmd = new MySqlCommand("SELECT * FROM charactersextdata where charID = '" + c.ID + "'", conn);
        //        try { reader = cmd.ExecuteReader(); Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + c.ID + "'" + " Successful"); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count > 0)
        //            c.Settings.Load_SystemFlag(src.Rows[0]["Settings"].ToString());

        //        if (!reader.IsClosed) reader.Close();
        //        src = null;
        //    #endregion

        //    #region Quests
        //    #endregion

        //    #region SideBar
        //    #endregion

        //}
        //public ushort GetBattleBG(ushort map)
        //{
        //    return 140;
        //}
        //public bool WriteStorage(ref Player src, Storagetype writetype, List<long[]> Data)
        //{
        //    if (src == null || src.ID == 0) return false;

            
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable table = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); return false; }

        //    foreach (var r in Data)
        //    {
        //        cmd = new MySqlCommand(string.Format("SELECT * FROM {0} where {1}", "inventory", "charID = '" + src.ID + " AND storID = '" + (byte)writetype + "'"), conn);

        //        try
        //        {
        //            reader = cmd.ExecuteReader();
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); }

        //        if (reader.HasRows)
        //        {
        //            cmd = new MySqlCommand(string.Format("INSERT INTO inventory (invIdx,charID,storID,itemID,dmg,qty,pos,socketID,bombID,sewID,forge) VALUES {0}", string.Format("('{0}','{1}','1','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),",
        //                    Data[0], src.ID, Data[1], Data[2], Data[3], Data[4], Data[5], Data[6], Data[7], Data[8])), conn);

        //            try
        //            {
        //                cmd.ExecuteNonQuery();
        //                Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //            }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
        //        }
        //        else
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE inventory SET {0} where {1}", string.Format("itemID = '{0}', dmg = '{1}', qty = '{2}', pos = '{3}', socketID = '{4}', bombID = '{5}', sewID = '{6}', forge = '{7}'",
        //                 Data[1], Data[2], Data[3], Data[4], Data[5], Data[6], Data[7], Data[8]), "charID ='" + src.ID + "' AND storID ='0' AND invIdx = '" + Data[0] + "'"), conn);

        //            try
        //            {
        //                cmd.ExecuteNonQuery();
        //                Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //            }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
        //        }

        //    }
        //    return true;
        //}
    }
}
