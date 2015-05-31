using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using Game;

namespace DataBase
{
    public class GameDataBase:RCLibrary.Core.DataBase
    {
        const string DBServer = "GameDataBase";
        //DBConnector.DBOAuth DBAssist;

        public GameDataBase()
        {
            //DBAssist = new DBConnector.DBOAuth();
        }

        public void VerifySetup()
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            DataTable src = null;

        }
        //public void LoadFinalData(ref Player c)
        //{
        //    DataTable src = null;
            
        //    #region Inventory
        //    src = cGlobal.gDataBaseConnection.GetDataTable("SELECT * FROM inventory where charID = '" + c.ID + "'");

        //    if (src.Rows.Count > 0)
        //    {
        //        ushort id;

        //        for (int i = 0; i < src.Rows.Count; i++)
        //        {
        //            id = ushort.Parse(src.Rows[i]["itemID"].ToString());
        //            switch (uint.Parse(src.Rows[i]["storID"].ToString()))
        //            {
        //                case 0:
        //                    if (id != 0)
        //                    {
        //                        var data = new InvItem();
        //                        data.CopyFrom(cGlobal.gItemManager.GetItem(id));
        //                        data.Ammt = byte.Parse(src.Rows[i]["qty"].ToString());
        //                        data.Damage = byte.Parse(src.Rows[i]["dmg"].ToString());
        //                        c.Inv[byte.Parse(src.Rows[i]["pos"].ToString())] = data;
        //                        //rows[i]["socketID"].ToString(), rows[i]["bombID"].ToString(),rows[i]["sewID"].ToString(),rows[i]["dmg"].ToString(),rows[i]["forge"].ToString(), , });
        //                    }
        //                    break;
        //            }
        //        }
        //    }

        //    src = null;
        //    #endregion

        //    #region Tent
        //    #endregion

        //    #region Friends
        //    src = cGlobal.gDataBaseConnection.GetDataTable("SELECT * FROM charactersextdata where charID = '" + c.ID + "'");

        //    if (src.Rows.Count > 0)
        //        c.LoadFriends(src.Rows[0]["Friends"].ToString());

        //    src = null;
        //    #endregion

        //    #region Mail
        //    src = cGlobal.gDataBaseConnection.GetDataTable("SELECT * FROM charactersextdata where charID = '" + c.ID + "'");
            
        //    if (src.Rows.Count > 0)
        //    {
        //        //foreach (string m in src.Rows[0]["Mail"].ToString().Split('&'))
        //        //{
        //        //    if (m == "none") break;
        //        //    Mail tmp = new Mail();
        //        //    tmp.Load(m);
        //        //    var re = cGlobal.WLO_World.GetPlayer(tmp.targetid);
        //        //    if (tmp.type == "Send")
        //        //    {
        //        //        if (!tmp.isSent && re != null)
        //        //        {
        //        //            c.MailBox.Add(tmp);
        //        //            re.RecvMailfrom(c, tmp.message, tmp.when);
        //        //        }
        //        //        else
        //        //            c.MailBox.Add(tmp);
        //        //    }
        //        //    else
        //        //        c.MailBox.Add(tmp);
        //        //}
        //    }
        //    #endregion

        //    #region Skills

        //    #endregion

        //    #region Settings
        //    src = cGlobal.gDataBaseConnection.GetDataTable("SELECT * FROM charactersextdata where charID = '" + c.ID + "'");

        //    if (src.Rows.Count > 0)
        //        c.Settings.Load_SystemFlag(src.Rows[0]["Settings"].ToString());

        //    src = null;
        //    #endregion

        //    #region Quests
        //    #endregion

        //    #region SideBar
        //    #endregion

        //}

        public ushort GetBattleBG(ushort map)
        {
            return 140;
        }

        public void SetupMap(ref Game.Maps.GameMap src)
        {

        }
        public void WriteTent(Game.Code.PlayerRelated.Tent src)
        {

        }
        
        //public bool WriteStorage(ref Player src, Storagetype writetype, List<long[]> Data)
        //{
        //    if (src == null || src.ID == 0) return false;


        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable table = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { DebugSystem.Write(f); return false; }

        //    foreach (var r in Data)
        //    {
        //        cmd = new MySqlCommand(string.Format("SELECT * FROM {0} where {1}", "inventory", "charID = '" + src.ID + " AND storID = '" + (byte)writetype + "'"), conn);

        //        try
        //        {
        //            reader = cmd.ExecuteReader();
        //            DebugSystem.Write(DBServer + cmd.CommandText + " Successful");
        //        }
        //        catch (MySqlException ex) { DebugSystem.Write(ex); }

        //        if (reader.HasRows)
        //        {
        //            cmd = new MySqlCommand(string.Format("INSERT INTO inventory (invIdx,charID,storID,itemID,dmg,qty,pos,socketID,bombID,sewID,forge) VALUES {0}", string.Format("('{0}','{1}','1','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),",
        //                    Data[0], src.ID, Data[1], Data[2], Data[3], Data[4], Data[5], Data[6], Data[7], Data[8])), conn);

        //            try
        //            {
        //                cmd.ExecuteNonQuery();
        //                DebugSystem.Write(DBServer + cmd.CommandText + " Successful");
        //            }
        //            catch (MySqlException ex) { DebugSystem.Write(ex); return false; }
        //        }
        //        else
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE inventory SET {0} where {1}", string.Format("itemID = '{0}', dmg = '{1}', qty = '{2}', pos = '{3}', socketID = '{4}', bombID = '{5}', sewID = '{6}', forge = '{7}'",
        //                 Data[1], Data[2], Data[3], Data[4], Data[5], Data[6], Data[7], Data[8]), "charID ='" + src.ID + "' AND storID ='0' AND invIdx = '" + Data[0] + "'"), conn);

        //            try
        //            {
        //                cmd.ExecuteNonQuery();
        //                DebugSystem.Write(DBServer + cmd.CommandText + " Successful");
        //            }
        //            catch (MySqlException ex) { DebugSystem.Write(ex); return false; }
        //        }

        //    }
        //    return true;
        //}
    }
}
