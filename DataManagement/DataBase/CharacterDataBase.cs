using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MySql.Data.MySqlClient;
using System.Data;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.DataManagement.DataBase
{
    public sealed class CharacterDataBase
    {
        const string DBServer = "CharacterDataBase";
        DBConnector.DBOAuth DBAssist;

        List<string> client_requested_names = new List<string>();
       
        /// <summary>
        /// saves a cache of a character
        /// </summary>
        ConcurrentDictionary<int, Character> Cache = new ConcurrentDictionary<int,Character>();
        
        Dictionary<string, uint> CharNames = new Dictionary<string, uint>();
        public CharacterDataBase()
        {
            DBAssist = new DBConnector.DBOAuth();
        }

        void Setup()
        {
            #region Restricted Name
            //CharNames.Add("Tatsuya", 3322);//Jay
            //CharNames.Add("Compass", 2);//Compass
            //CharNames.Add("Sharky", 55);//Sharky
            //CharNames.Add("rcharnel", 69);//rcharnel
            //CharNames.Add("Petron", 234);//petron
            //CharNames.Add("Nipple", 3322);//Johnathan
            //CharNames.Add("Dragon", 54);//Dragon
            //CharNames.Add("Maganda", 92);//Maganda
            //CharNames.Add("Flurrih", 3132);//Flurrih
            #endregion
        }

        public bool Can_Connect { get { return (DBAssist == null) ? false : DBAssist.VerifyConnection(); } }
        public void VerifySetup()
        {

            #region characters Columns
            Dictionary<string, string> col = new Dictionary<string, string>();
            col.Add("charID", "int(11) NOT NULL");
            col.Add("head", "int(11) DEFAULT NULL");
            col.Add("body", "int(11) DEFAULT NULL");
            col.Add("name", "text");
            col.Add("name_clean", "text");
            col.Add("nickname", "text");
            col.Add("location_map", "int(11) DEFAULT NULL");
            col.Add("location_x", "int(11) DEFAULT NULL");
            col.Add("location_y", "int(11) DEFAULT NULL");
            col.Add("haircolor", "int(11) DEFAULT NULL");
            col.Add("skincolor", "int(11) DEFAULT NULL");
            col.Add("clothingcolor", "int(11) DEFAULT NULL");
            col.Add("eyecolor", "int(11) DEFAULT NULL");
            col.Add("gold", "int(11) DEFAULT NULL");
            col.Add("element", "int(11) DEFAULT NULL");
            col.Add("rebirth", "int(11) DEFAULT NULL");
            col.Add("job", "int(11) DEFAULT NULL");
            col.Add("online", "int(11) DEFAULT NULL");
            col.Add("PRIMARY KEY", "PRIMARY KEY (charID)");

            #endregion
            #region charextdata Columns
            Dictionary<string, string> extdtcol = new Dictionary<string, string>();
            extdtcol.Add("charID", "int(11) NOT NULL");
            extdtcol.Add("Settings", "varchar(150) DEFAULT NULL");
            extdtcol.Add("Friends", "text");
            extdtcol.Add("Guild", "varchar(100) DEFAULT NULL");
            extdtcol.Add("Mail", "text");
            extdtcol.Add("PRIMARY KEY", "PRIMARY KEY (charID)");
            #endregion
            #region chartent Columns
            Dictionary<string, string> chartent = new Dictionary<string, string>();
            chartent.Add("charID", "int(11) NOT NULL");
            chartent.Add("locked", "int(11)");
            chartent.Add("enlarged", "int(11)");
            chartent.Add("tenttype", "int(11)");
            chartent.Add("floor1Color", "int(11)");
            chartent.Add("floor1wallpaper", "int(11)");
            chartent.Add("floor2Color", "int(11)");
            chartent.Add("floor2wallpaperr", "int(11)");
            chartent.Add("PRIMARY KEY", "PRIMARY KEY (charID)");
            #endregion
            #region charquest Columns
            Dictionary<string, string> charquest = new Dictionary<string, string>();
            charquest.Add("pri_key", "int(11) NOT NULL");
            charquest.Add("charID", "int(11) NOT NULL");
            charquest.Add("quest_started", "int(11)");
            charquest.Add("quest_pos", "int(11)");
            charquest.Add("PRIMARY KEY", "PRIMARY KEY (pri_key)");
            #endregion
            #region charunlocks Columns
            Dictionary<string, string> charunlocks = new Dictionary<string, string>();
            charunlocks.Add("pri_key", "int(11) NOT NULL");
            charunlocks.Add("charID", "int(11) NOT NULL");
            charunlocks.Add("maploc", "int(11)");
            charunlocks.Add("clickID", "int(11)");
            charunlocks.Add("PRIMARY KEY", "PRIMARY KEY (pri_key)");
            #endregion


            #region characters table Verification
            Utilities.LogServices.Log("Checking characters table");
        retry:

            if(DBAssist.GetDataTable("SELECT * FROM characters") == null) goto exist;

            Utilities.LogServices.Log("Setuping up characters table");
            string cmstr = "create table characters (";
            switch (DBAssist.TypeofDB)
            {
                case DBConnector.DBType.MySQl:
                    {

                        foreach (var t in col)
                        {
                            if (t.Key != "PRIMARY KEY")
                                cmstr += string.Format("{0} {1},", t.Key, t.Value);
                            else
                                cmstr += string.Format("{0},", t.Value);
                        }
                        cmstr = cmstr.Substring(0, cmstr.Length - 1);
                        cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                    } break;
            }
            DBAssist.ExecuteNonQuery(cmstr);

        exist:
            //table exists verify columns  
            foreach (string h in col.Keys.Where(c => c != "PRIMARY KEY"))
            {
                if (DBAssist.GetDataTable("select " + h + " from characters") == null)
                {
                    Utilities.LogServices.Log("Reseting characters table");
                    DBAssist.ExecuteNonQuery("drop table if exists characters");
                    goto retry;
                }
            }
            #endregion

        //    #region charactersextdata Verification
        //Utilities.LogServices.Log("Checking charactersextdata table");

        //retry2:
        //    cmd = new MySqlCommand("SELECT * FROM charactersextdata", conn);
        //    try { reader = cmd.ExecuteReader(); reader.Close(); goto exist2; }
        //    catch { }

        //    Utilities.LogServices.Log("Setuping up charactersextdata table");
        //    cmstr = "create table charactersextdata (";
        //    foreach (var t in extdtcol)
        //    {
        //        if (t.Key != "PRIMARY KEY")
        //            cmstr += string.Format("{0} {1},", t.Key, t.Value);
        //        else
        //            cmstr += string.Format("{0},", t.Value);
        //    }
        //    cmstr = cmstr.Substring(0, cmstr.Length - 1);
        //    cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        //    cmd = new MySqlCommand(cmstr, conn);
        //    cmd.ExecuteNonQuery();

        //exist2:
        //    //table exists verify columns               

        //    foreach (string h in extdtcol.Keys.Where(c => c != "PRIMARY KEY"))
        //    {
        //        cmd = new MySqlCommand("select " + h + " from charactersextdata ", conn);
        //        try { reader = cmd.ExecuteReader(); reader.Close(); }
        //        catch
        //        {

        //            Utilities.LogServices.Log("Reseting charactersextdata table");
        //            cmd = new MySqlCommand("drop table if exists charactersextdata", conn);
        //            cmd.ExecuteNonQuery();
        //            goto retry2;
        //        }
        //    }
        //    #endregion

        //    #region chartent Verification
        //Utilities.LogServices.Log("Checking chartent table");
        //retry3:
        //    cmd = new MySqlCommand("SELECT * FROM chartent", conn);
        //    try { reader = cmd.ExecuteReader(); reader.Close(); goto exist3; }
        //    catch { }

        //    Utilities.LogServices.Log("Setuping up chartent table");
        //    cmstr = "create table chartent (";
        //    foreach (var t in extdtcol)
        //    {
        //        if (t.Key != "PRIMARY KEY")
        //            cmstr += string.Format("{0} {1},", t.Key, t.Value);
        //        else
        //            cmstr += string.Format("{0},", t.Value);
        //    }
        //    cmstr = cmstr.Substring(0, cmstr.Length - 1);
        //    cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        //    cmd = new MySqlCommand(cmstr, conn);
        //    cmd.ExecuteNonQuery();

        //exist3:
        //    //table exists verify columns               

        //    foreach (string h in extdtcol.Keys.Where(c => c != "PRIMARY KEY"))
        //    {
        //        cmd = new MySqlCommand("select " + h + " from chartent ", conn);
        //        try { reader = cmd.ExecuteReader(); reader.Close(); }
        //        catch
        //        {

        //            Utilities.LogServices.Log("Reseting chartent table");
        //            cmd = new MySqlCommand("drop table if exists chartent", conn);
        //            cmd.ExecuteNonQuery();
        //            goto retry3;
        //        }
        //    }
        //    #endregion

        //    #region charquest Verification
        //Utilities.LogServices.Log("Checking charquest table");
        //retry4:
        //    cmd = new MySqlCommand("SELECT * FROM charquest", conn);
        //    try { reader = cmd.ExecuteReader(); reader.Close(); goto exist4; }
        //    catch { }

        //    Utilities.LogServices.Log("Setuping up charquest table");
        //    cmstr = "create table charquest (";
        //    foreach (var t in extdtcol)
        //    {
        //        if (t.Key != "PRIMARY KEY")
        //            cmstr += string.Format("{0} {1},", t.Key, t.Value);
        //        else
        //            cmstr += string.Format("{0},", t.Value);
        //    }
        //    cmstr = cmstr.Substring(0, cmstr.Length - 1);
        //    cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        //    cmd = new MySqlCommand(cmstr, conn);
        //    cmd.ExecuteNonQuery();

        //exist4:
        //    //table exists verify columns               

        //    foreach (string h in extdtcol.Keys.Where(c => c != "PRIMARY KEY"))
        //    {
        //        cmd = new MySqlCommand("select " + h + " from charquest ", conn);
        //        try { reader = cmd.ExecuteReader(); reader.Close(); }
        //        catch
        //        {

        //            Utilities.LogServices.Log("Reseting charquest table");
        //            cmd = new MySqlCommand("drop table if exists charquest", conn);
        //            cmd.ExecuteNonQuery();
        //            goto retry4;
        //        }
        //    }
        //    #endregion

        //    #region charunlocks Verification
        //Utilities.LogServices.Log("Checking charunlocks table");
        //retry5:
        //    cmd = new MySqlCommand("SELECT * FROM charunlocks", conn);
        //    try { reader = cmd.ExecuteReader(); reader.Close(); goto exist5; }
        //    catch { }

        //    Utilities.LogServices.Log("Setuping up charunlocks table");
        //    cmstr = "create table charunlocks (";
        //    foreach (var t in extdtcol)
        //    {
        //        if (t.Key != "PRIMARY KEY")
        //            cmstr += string.Format("{0} {1},", t.Key, t.Value);
        //        else
        //            cmstr += string.Format("{0},", t.Value);
        //    }
        //    cmstr = cmstr.Substring(0, cmstr.Length - 1);
        //    cmstr += ") ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        //    cmd = new MySqlCommand(cmstr, conn);
        //    cmd.ExecuteNonQuery();

        //exist5:
        //    //table exists verify columns               

        //    foreach (string h in extdtcol.Keys.Where(c => c != "PRIMARY KEY"))
        //    {
        //        cmd = new MySqlCommand("select " + h + " from charunlocks ", conn);
        //        try { reader = cmd.ExecuteReader(); reader.Close(); }
        //        catch
        //        {

        //            Utilities.LogServices.Log("Reseting charunlocks table");
        //            cmd = new MySqlCommand("drop table if exists charunlocks", conn);
        //            cmd.ExecuteNonQuery();
        //            goto retry5;
        //        }
        //    }
        //    #endregion
        }
        //public bool LockName(uint dbacc, string name)
        //{
        //    if (name == null) return false;
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];

        //    MySqlConnection conn = GenerateConn();

        //    if (!client_requested_names.Contains(name))
        //    {

        //        try { conn.Open(); }
        //        catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //        cmd = new MySqlCommand("SELECT * FROM characters where name = @myname", conn);
        //        cmd.Parameters.AddWithValue("@myname", name);
        //        try { reader = cmd.ExecuteReader(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //        Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM characters where name = '" + name + "'" + " Successful");
        //        conn.Close();
        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count == 0)
        //        {

        //            if (CharNames.ContainsKey(name))
        //            {
        //                if (CharNames[name] != dbacc) return false;
        //                else
        //                {
        //                    client_requested_names.Add(name); return true;
        //                }
        //            }
        //            else
        //                client_requested_names.Add(name); return true;
        //        }
        //        return false;
        //    }
        //    else
        //        return false;
        //}
        //public void unLockName(string name)
        //{
        //    if (name == null) return;
        //    if (client_requested_names.Contains(name))
        //        client_requested_names.Remove(name);
        //}
        //public bool NameTaken(string name,uint target)
        //{
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];

        //    MySqlConnection conn = GenerateConn();
        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //    cmd = new MySqlCommand("SELECT * FROM characters where name = @myname", conn);
        //    cmd.Parameters.AddWithValue("@myname", name);
        //    try { reader = cmd.ExecuteReader(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }          
        //    src = new DataTable();
        //    src.Load(reader);
        //    if (src.Rows.Count != 0)
        //    {
        //        cmd = new MySqlCommand("SELECT * FROM characters where charID =" + target, conn);
        //        cmd.Parameters.AddWithValue("@myname", name);
        //        try { reader = cmd.ExecuteReader(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        src = new DataTable();
        //        src.Load(reader);
        //        if (src.Rows.Count != 0)
        //            return !(src.Rows[0]["name_clean"].ToString() == name.ToLower());
        //        else
        //            return true;
        //    }            
        //    conn.Close();
        //    return false;
        //}
        //public bool ApplyNameChange(uint charID, string name, out Exception err)
        //{
        //    MySqlCommand cmd = null;
        //    DataRow[] rows = new DataRow[0];

        //    MySqlConnection conn = GenerateConn();
        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); err = f; return false; }

        //    cmd = new MySqlCommand("UPDATE characters SET name = @myname, name_clean=@myname2 where charID ='" + charID + "'", conn);
        //    cmd.Parameters.AddWithValue("@myname", name);
        //    cmd.Parameters.AddWithValue("@myname2", name.ToLower());
        //    try { err = null; return (cmd.ExecuteNonQuery() == 1); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); err = ex; return false; }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}
        //public void DeleteCharacter(UInt32 ID)
        //{
        //    MySqlConnection conn = GenerateConn();
        //    MySqlCommand cmd = null;

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //        cmd = new MySqlCommand(("DELETE FROM characters where charID = '" + ID + "';"), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //        Utilities.LogServices.Log("Delete CMD=> DELETE FROM characters where charID = '" + ID + "';" + " Successful");


        //        cmd = new MySqlCommand(("DELETE FROM stats where charID ='" + ID + "';"), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log("Delete CMD=> DELETE FROM stats where charID ='" + ID + "';" + " Successful");


        //        cmd = new MySqlCommand(("DELETE FROM inventory where charID ='" + ID + "';"), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log("Delete CMD=> DELETE FROM stats where charID ='" + ID + "';" + " Successful");
        //        conn.Close();

        //        if (Cache.ContainsKey((int)ID))
        //        {
        //            Character t;
        //            Cache.TryRemove((int)ID,out t);
        //        }
        //}
        //public Character GetCharacterData(uint charID)
        //{
        //    bool good = true;
        //    if (charID == 0) return null;
        //    if (Cache.ContainsKey((int)charID))
        //        return Cache[(int)charID];

        //    Character t = new Character();
        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows;

        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //            cmd = new MySqlCommand("SELECT * FROM characters where charID = '" + charID + "'", conn);

        //            try { reader = cmd.ExecuteReader(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM characters where charID = '" + charID + "'" + " Successful");
        //            src = new DataTable();
        //            src.Load(reader);
                    
        //            if (src.Rows.Count > 0)
        //            {
        //                rows = new DataRow[src.Rows.Count];
        //                src.Rows.CopyTo(rows, 0);
        //                t.ID = uint.Parse(rows[0]["charID"].ToString());
        //                t.Head = byte.Parse(rows[0]["head"].ToString());
        //                t.Body = (BodyStyle)uint.Parse(rows[0]["body"].ToString());
        //                t.m_name = rows[0]["name"].ToString();
        //                t.Nickname = rows[0]["nickname"].ToString();
        //                t.LoginMap = ushort.Parse(rows[0]["location_map"].ToString());
        //                t.X = ushort.Parse(rows[0]["location_x"].ToString());
        //                t.Y = ushort.Parse(rows[0]["location_y"].ToString());
        //                t.HairColor = ushort.Parse(rows[0]["haircolor"].ToString());
        //                t.SkinColor = ushort.Parse(rows[0]["skincolor"].ToString());
        //                t.ClothingColor = ushort.Parse(rows[0]["clothingcolor"].ToString());
        //                t.EyeColor = ushort.Parse(rows[0]["eyecolor"].ToString());
        //                t.Gold = uint.Parse(rows[0]["gold"].ToString());
        //                t.Element = (ElementType)byte.Parse(rows[0]["element"].ToString());
        //                t.Job = (RebornJob)byte.Parse(rows[0]["job"].ToString());
        //            }
        //            else
        //            {
        //                Utilities.LogServices.Log(DBServer + "No Character for " + charID);
        //                return null;
        //            }

        //        //load stat data
        //            cmd = new MySqlCommand("SELECT * FROM stats where charID = '" + charID + "'", conn);
        //            reader = cmd.ExecuteReader();
        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM stats where charID = '" + charID + "'" + " Successful");
        //            src = new DataTable();
        //            src.Load(reader);
        //    if(src.Rows.Count > 0)
        //    {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //            List<long[]> tmp2 = new List<long[]>();
        //            if (t == null) t = new Character();
        //            foreach (var row in rows)
        //            {
        //                switch (long.Parse(row["statID"].ToString()))
        //                {
        //                    case 25: t.CurHP = int.Parse(row["statVal"].ToString()); break;
        //                    case 26: t.CurSP = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 38: t.SkillPoints = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 36: t.TotalExp = long.Parse(row["statVal"].ToString()); break;
        //                    case 28: t.Str = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 29: t.Con = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 30: t.Agi = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 27: t.Int = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 33: t.Wis = ushort.Parse(row["statVal"].ToString()); break;
        //                }
        //            }
        //    }

        //        //load equips
        //            cmd = new MySqlCommand("SELECT * FROM inventory where charID = '" + charID + "' AND storID =1", conn);
        //            reader = cmd.ExecuteReader();
        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + charID + "' AND storID =1" + " Successful");
        //            src = new DataTable();
        //            src.Load(reader);
                    
        //            if (src.Rows.Count > 0)
        //            {
        //                rows = new DataRow[src.Rows.Count];

        //                src.Rows.CopyTo(rows, 0);

        //                ushort id;

        //                for (int i = 0; i < rows.Length; i++)
        //                {
        //                    id = ushort.Parse(rows[i]["itemID"].ToString());
        //                    if (id != 0)
        //                    {
        //                        t[byte.Parse(rows[i]["pos"].ToString())].CopyFrom(myhost.ItemDataBase.GetItem(id));
        //                        t[byte.Parse(rows[i]["pos"].ToString())].Ammt = 1;
        //                        t[byte.Parse(rows[i]["pos"].ToString())].Damage = byte.Parse(rows[i]["dmg"].ToString());
        //                        //                    tmp4.Add((byte)i, new string[]{id.ToString(), rows[i]["socketID"].ToString(), rows[i]["bombID"].ToString(),rows[i]["sewID"].ToString(), 
        //                        //rows[i]["dmg"].ToString(),rows[i]["forge"].ToString(), });
        //                    }
        //                }
        //            }

        //        return t;
        //}
        //public void GetCharacterData(uint charID,ref Player t)
        //{
        //    if (charID == 0) return;
        //    if (Cache.ContainsKey((int)charID))
        //    {
        //        t.ID = Cache[(int)charID].ID;
        //        t.Head = Cache[(int)charID].Head;
        //        t.Body = Cache[(int)charID].Body;
        //        t.CharacterName = Cache[(int)charID].m_name;
        //        t.Nickname = Cache[(int)charID].Nickname;
        //        t.LoginMap = Cache[(int)charID].LoginMap;
        //        t.X = Cache[(int)charID].X;
        //        t.Y = Cache[(int)charID].Y;
        //        t.HairColor = Cache[(int)charID].HairColor;
        //        t.SkinColor = Cache[(int)charID].SkinColor;
        //        t.ClothingColor = Cache[(int)charID].ClothingColor;
        //        t.EyeColor = Cache[(int)charID].EyeColor;
        //        t.Gold = Cache[(int)charID].Gold;
        //        t.Element = Cache[(int)charID].Element;
        //        t.Job = Cache[(int)charID].Job;
        //        return;
        //    }

        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();


        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); throw; }

        //            cmd = new MySqlCommand("SELECT * FROM characters where charID = '" + charID + "'", conn);
        //            try { reader = cmd.ExecuteReader(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM characters where charID = '" + charID + "'" + " Successful");

        //            src = new DataTable();
        //            src.Load(reader);
        //            if(src.Rows.Count > 0)
        //            {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //                t.ID = uint.Parse(rows[0]["charID"].ToString());
        //                t.Head = byte.Parse(rows[0]["head"].ToString());
        //                t.Body = (BodyStyle)uint.Parse(rows[0]["body"].ToString());
        //                t.CharacterName = rows[0]["name"].ToString();
        //                t.Nickname = rows[0]["nickname"].ToString();
        //                t.LoginMap = ushort.Parse(rows[0]["location_map"].ToString());
        //                t.X = ushort.Parse(rows[0]["location_x"].ToString());
        //                t.Y = ushort.Parse(rows[0]["location_y"].ToString());
        //                t.HairColor = ushort.Parse(rows[0]["haircolor"].ToString());
        //                t.SkinColor = ushort.Parse(rows[0]["skincolor"].ToString());
        //                t.ClothingColor = ushort.Parse(rows[0]["clothingcolor"].ToString());
        //                t.EyeColor = ushort.Parse(rows[0]["eyecolor"].ToString());
        //                t.Gold = uint.Parse(rows[0]["gold"].ToString());
        //                t.Element = (ElementType)byte.Parse(rows[0]["element"].ToString());
        //                t.Job = (RebornJob)byte.Parse(rows[0]["job"].ToString());
        //            }
        //            else
        //            {
        //                Utilities.LogServices.Log(new Exception("Character not found for " + charID));
        //                throw new Exception("Character not found for "+charID);
        //            }
        //        //load stat data
                
        //            cmd = new MySqlCommand("SELECT * FROM stats where charID = '" + charID + "'", conn);

        //            try { reader = cmd.ExecuteReader(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //           Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM stats where charID = '" + charID + "'" + " Successful");
        //            src = new DataTable();
        //            src.Load(reader);
        //            if(src.Rows.Count >0)
        //            {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //            List<long[]> tmp2 = new List<long[]>();
        //            foreach (var row in rows)
        //            {
        //                switch (long.Parse(row["statID"].ToString()))
        //                {
        //                    case 25: t.CurHP = int.Parse(row["statVal"].ToString()); break;
        //                    case 26: t.CurSP = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 38: t.SkillPoints = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 36: t.TotalExp = long.Parse(row["statVal"].ToString()); break;
        //                    case 28: t.Str = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 29: t.Con = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 30: t.Agi = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 27: t.Int = ushort.Parse(row["statVal"].ToString()); break;
        //                    case 33: t.Wis = ushort.Parse(row["statVal"].ToString()); break;
        //                }
        //            }
        //            }

        //        //load equips
                
        //            cmd = new MySqlCommand("SELECT * FROM inventory where charID = '" + charID + "' AND storID =1", conn);

        //            try { reader = cmd.ExecuteReader(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }

        //            Utilities.LogServices.Log(DBServer + " GetRows CMD=>  SELECT * FROM inventory where charID = '" + charID + "' AND storID =1" + " Successful");

        //            src = new DataTable();
        //            src.Load(reader);
        //            if(src.Rows.Count > 0)
        //            {
        //            rows = new DataRow[src.Rows.Count];
        //            src.Rows.CopyTo(rows, 0);
        //        ushort id;

        //        for (int i = 0; i < rows.Length; i++)
        //        {
        //            id = ushort.Parse(rows[i]["itemID"].ToString());
        //            if (id != 0)
        //            {
        //                t[byte.Parse(rows[i]["pos"].ToString())].CopyFrom(myhost.ItemDataBase.GetItem(id));
        //                t[byte.Parse(rows[i]["pos"].ToString())].Ammt = 1;
        //                t[byte.Parse(rows[i]["pos"].ToString())].Damage = byte.Parse(rows[i]["dmg"].ToString());
        //                //                    tmp4.Add((byte)i, new string[]{id.ToString(), rows[i]["socketID"].ToString(), rows[i]["bombID"].ToString(),rows[i]["sewID"].ToString(), 
        //                //rows[i]["dmg"].ToString(),rows[i]["forge"].ToString(), });
        //            }
        //        }
        //            }

        //        conn.Close();
        //}
        //public bool WriteNewPlayer(uint charID, Player player)
        //{
        //    if (charID == 0) return false;

        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];
        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); return false; }

        //    #region write character Data

        //    Character c = (Character)player;
        //    Dictionary<string, string> insert = new Dictionary<string, string>();
        //    insert.Add("charID", player.ID.ToString());
        //    insert.Add("head", c.Head.ToString());
        //    insert.Add("body", ((byte)c.Body).ToString());
        //    insert.Add("name", c.m_name);
        //    insert.Add("name_clean", c.m_name.ToLower());
        //    insert.Add("nickname", c.Nickname);
        //    insert.Add("location_map", c.LoginMap.ToString());
        //    insert.Add("location_x", c.X.ToString());
        //    insert.Add("location_y", c.Y.ToString());
        //    insert.Add("haircolor", c.HairColor.ToString());
        //    insert.Add("skincolor", c.SkinColor.ToString());
        //    insert.Add("clothingcolor", c.ClothingColor.ToString());
        //    insert.Add("eyecolor", c.EyeColor.ToString());
        //    insert.Add("gold", c.Gold.ToString());
        //    insert.Add("element", ((byte)c.Element).ToString());
        //    insert.Add("rebirth", BitConverter.GetBytes(c.Reborn)[0].ToString());
        //    insert.Add("job", ((byte)c.Job).ToString());//fix
        //    insert.Add("online", BitConverter.GetBytes(false)[0].ToString());
        //    string vals = "(";
        //    if (insert.Count >= 1)
        //    {
        //        foreach (KeyValuePair<String, String> val in insert)
        //        {
        //            vals += String.Format(" {0} ,", val.Key.ToString());
        //        }
        //        vals = vals.Substring(0, vals.Length - 1);
        //        vals += ") VALUES (";
        //        foreach (KeyValuePair<String, String> val in insert)
        //        {
        //            vals += String.Format(" '{0}' ,", val.Value.ToString());
        //        }
        //        vals = vals.Substring(0, vals.Length - 1);
        //        vals += ");";

        //    }
        //    cmd = new MySqlCommand(string.Format("INSERT INTO characters {0}", vals), conn);
        //    try { var rowschng = cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }

        //    Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    #endregion

        //    #region Write Extr Data
        //    cmd = new MySqlCommand(string.Format("SELECT * FROM {0} where {1}", "charactersExtData", "charID = '" + charID + "'"), conn);
        //    try { reader = cmd.ExecuteReader(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }

        //    Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    src = new DataTable();
        //    src.Load(reader);
        //    rows = new DataRow[src.Rows.Count];
        //    src.Rows.CopyTo(rows, 0);
        //    if (rows.Length == 0)
        //    {
        //        cmd = new MySqlCommand(string.Format("INSERT INTO charactersExtData {0}", string.Format("(charID,Settings,Friends,Guild,Mail) VALUES('{0}','{1}','{2}','{3}','{4}');", charID, player.Settings.Get_SysytemFlag(), player.GetFriends_Flag, "0", player.GetMailboxFlags())), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    }
        //    else
        //    {
        //        cmd = new MySqlCommand(string.Format("UPDATE charactersExtData SET {0} where charID = '" + charID + "';", string.Format(" Settings = '{0}', Friends = '{1}', Guild = '{2}', Mail = '{3}'", player.Settings.Get_SysytemFlag(), player.GetFriends_Flag, "0", player.GetMailboxFlags())), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    }
        //    #endregion

        //    #region write stats

        //    foreach (var u in player.Stat_toSave)
        //    {

        //        cmd = new MySqlCommand(string.Format("SELECT * FROM {0} where {1}", "stats", "charID = '" + charID + "' AND statID = '" + u[0] + "'"), conn);
        //        try { reader = cmd.ExecuteReader(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 468", ex); }
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        src = new DataTable();
        //        src.Load(reader);
        //        rows = new DataRow[src.Rows.Count];
        //        src.Rows.CopyTo(rows, 0);
        //        if (rows.Length == 0)
        //        {
        //            cmd = new MySqlCommand(string.Format("INSERT INTO stats {0}", string.Format("(statIdx,charID,statID,statVal,potential) VALUES('0','{0}','{1}','{2}','{3}');", charID, u[0], u[1], player.Potential)), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 477", ex); throw; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //        else
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE stats SET {0} where charID = '" + charID + "'AND statID = '" + u[0] + "';", string.Format(" statID = '{0}',statVal ='{1}',potential ='{2}'", u[0], u[1], player.Potential)), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 483", ex); throw; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //    }
        //    #endregion

        //    #region predelete inv
        //    cmd = new MySqlCommand(("DELETE FROM inventory where charID ='" + charID + "';"), conn);
        //    try { cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //    Utilities.LogServices.Log("Delete CMD=> DELETE FROM stats where charID ='" + charID + "';" + " Successful");
        //    #endregion

        //    #region write inv
        //    if (player.Inv.InventoryDBData != null)
        //    {
        //        string t = "";
        //        foreach (var u in player.Inv.InventoryDBData)
        //        {
        //            t += string.Format("('{0}','{1}','0','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),",
        //                 u.Key, charID, u.Value[0], u.Value[1], u.Value[2], u.Value[3], u.Value[4], u.Value[5], u.Value[6], u.Value[7]);
        //        }
        //        t = t.Substring(0, t.Length - 1);
        //        t += ";";
        //        cmd = new MySqlCommand(string.Format("INSERT INTO inventory (invIdx,charID,storID,itemID,dmg,qty,pos,socketID,bombID,sewID,forge) VALUES {0}", t), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    }
        //    #endregion

        //    #region write eqs

        //    if (player.EqData != null)
        //    {
        //        string t = "";
        //        foreach (var u in player.EqData)
        //        {
        //            t += string.Format("('{0}','{1}','1','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),",
        //                    u.Key, charID, u.Value[0], u.Value[1], u.Value[2], u.Value[3], u.Value[4], u.Value[5], u.Value[6], u.Value[7]);
        //        }
        //        t = t.Substring(0, t.Length - 1);
        //        t += ";";
        //        cmd = new MySqlCommand(string.Format("INSERT INTO inventory (invIdx,charID,storID,itemID,dmg,qty,pos,socketID,bombID,sewID,forge) VALUES {0}", t), conn);
        //        try { cmd.ExecuteNonQuery(); }
        //        catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    }

        //    conn.Close();

        //    if (Cache.ContainsKey((int)charID) && !player.BlockSave)
        //        Cache[(int)charID] = player;
        //    #endregion

        //    return true;
        //}
        //public bool WritePlayer(uint charID, Player player)
        //{
        //    bool rem = false;
        //    if (charID == 0 || player.BlockSave) return rem;

        //    if (Cache.ContainsKey((int)charID))
        //        Cache[(int)charID] = player;

        //    MySqlCommand cmd = null;
        //    MySqlDataReader reader = null;
        //    DataTable src = null;
        //    DataRow[] rows = new DataRow[0];

        //    MySqlConnection conn = GenerateConn();

        //    try { conn.Open(); }
        //    catch (MySqlException f) { Utilities.LogServices.Log(f); return false; }


        //    #region write character Data
        //    Character c = player;
        //    Dictionary<string, string> insert = new Dictionary<string, string>();
        //    insert.Add("head", c.Head.ToString());
        //    insert.Add("body", ((byte)c.Body).ToString());
        //    insert.Add("nickname", c.Nickname);
        //    insert.Add("location_map", (c.CurrentMap.TypeofMap == MapType.Regular) ? c.CurrentMap.MapID.ToString() : player.PrevMap.DstMap.ToString());
        //    insert.Add("location_x", (c.CurrentMap.TypeofMap == MapType.Regular) ? c.X.ToString() : player.PrevMap.DstX_Axis.ToString());
        //    insert.Add("location_y", (c.CurrentMap.TypeofMap == MapType.Regular) ? c.Y.ToString() : player.PrevMap.DstY_Axis.ToString());
        //    insert.Add("haircolor", c.HairColor.ToString());
        //    insert.Add("skincolor", c.SkinColor.ToString());
        //    insert.Add("clothingcolor", c.ClothingColor.ToString());
        //    insert.Add("eyecolor", c.EyeColor.ToString());
        //    insert.Add("gold", c.Gold.ToString());
        //    insert.Add("element", ((byte)c.Element).ToString());
        //    insert.Add("rebirth", BitConverter.GetBytes(c.Reborn)[0].ToString());
        //    insert.Add("job", ((byte)c.Job).ToString());//fix
        //    insert.Add("online", BitConverter.GetBytes(false)[0].ToString());
        //    string vals = "";
        //    if (insert.Count >= 1)
        //    {
        //        foreach (KeyValuePair<String, String> val in insert)
        //        {
        //            vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
        //        }
        //        vals = vals.Substring(0, vals.Length - 1);
        //    }
        //    cmd = new MySqlCommand(string.Format("UPDATE characters SET {0} where charID = '" + charID + "'", vals), conn);
        //    try { cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); return false; }
        //    //DLogger.DataBaseLog(DBServer + cmd.CommandText + " Successful");
        //    #endregion

        //    #region write stats

        //    foreach (var u in player.Stat_toSave)
        //    {

        //        cmd = new MySqlCommand(string.Format("SELECT * FROM {0} where {1}", "stats", "charID = '" + charID + "' AND statID = '" + u[0] + "'"), conn);
        //        reader = cmd.ExecuteReader();
        //        Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        src = new DataTable();
        //        src.Load(reader);
        //        rows = new DataRow[src.Rows.Count];
        //        src.Rows.CopyTo(rows, 0);
        //        if (rows.Length == 0)
        //        {
        //            cmd = new MySqlCommand(string.Format("INSERT INTO stats {0}", string.Format("(statIdx,charID,statID,statVal,potential) VALUES('0','{0}','{1}','{2}','{3}');", charID, u[0], u[1], player.Potential)), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 610", ex); return false; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //        else
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE stats SET {0} where charID = '" + charID + "'", string.Format(" statID = '{0}',statVal ='{1}',potential ='{2}'", u[0], u[1], player.Potential)), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 616", ex); return false; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //    }
        //    #endregion

        //    #region write inv

        //    string str = "";
        //    if (player.Inv.InventoryDBData != null)
        //        foreach (var u in player.Inv.InventoryDBData)
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE inventory SET {0} where {1};", string.Format("itemID = '{0}', dmg = '{1}', qty = '{2}', pos = '{3}', socketID = '{4}', bombID = '{5}', sewID = '{6}', forge = '{7}'",
        //                 u.Value[0], u.Value[1], u.Value[2], u.Value[3], u.Value[4], u.Value[5], u.Value[6], u.Value[7]), "charID ='" + charID + "' AND storID ='0' AND invIdx = '" + u.Key + "'"), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 634", ex); return false; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //    #endregion

        //    #region write eqs

        //    str = "";
        //    if (player.EqData != null)
        //        foreach (var u in player.EqData)
        //        {
        //            cmd = new MySqlCommand(string.Format("UPDATE inventory SET {0} where {1};", string.Format("itemID = '{0}', dmg = '{1}', qty = '{2}', pos = '{3}', socketID = '{4}', bombID = '{5}', sewID = '{6}', forge = '{7}'",
        //                 u.Value[0], u.Value[1], u.Value[2], u.Value[3], u.Value[4], u.Value[5], u.Value[6], u.Value[7]), "charID ='" + charID + "' AND storID ='1' AND invIdx = '" + u.Key + "'"), conn);
        //            try { cmd.ExecuteNonQuery(); }
        //            catch (MySqlException ex) { Utilities.LogServices.Log("Wonderland_Private_Server.Code.Enums", DBServer + " line 650", ex); return false; }
        //            Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //        }
        //    if (Cache.ContainsKey((int)charID) && !player.BlockSave)
        //        Cache[(int)charID] = player;

        //    conn.Close();
        //    #endregion

        //    #region write ext data

        //    cmd = new MySqlCommand(string.Format("UPDATE charactersExtData SET {0} where charID = '" + charID + "';", string.Format(" Settings = '{0}', Friends = '{1}', Guild = '{2}', Mail = '{3}'", player.Settings.Get_SysytemFlag(), player.GetFriends_Flag, "0", player.GetMailboxFlags())), conn);
            
        //    try { cmd.ExecuteNonQuery(); }
        //    catch (MySqlException ex) { Utilities.LogServices.Log(ex); throw; }
        //    Utilities.LogServices.Log(DBServer + cmd.CommandText + " Successful");
        //    #endregion

        //    return true;
        //}
    }
}
