using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

namespace Wonderland_Private_Server.Config
{

    [Serializable]
    public class SettingObj 
    {
    }

    public class Settings
    {
        readonly System.Xml.Serialization.XmlSerializer diskio;
        readonly object m_Lock = new object();
        
        public Settings()
        {
            diskio = new XmlSerializer(this.GetType());
        }
        public bool LoadSettings()
        {
            try
            {
                string line = ""; string ver = "";
                //using (StreamReader file = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.settings.wlo"))
                //    this = (Settings)diskio.Deserialize(file);
                //    while ((line = file.ReadLine()) != null)
                //    {
                //        switch (line)
                //        {
                //            case "[Version]":
                //                {
                //                    ver = file.ReadLine();
                //                    if (ver != "V9") return false;
                //                } break;
                //            case "[Settings]":
                //                {
                //                    if (ver == "") return false;
                //                    Config.Clear();
                //                    while ((line = file.ReadLine()) != ";")
                //                        Config.Add(line.Split('|')[0], line.Split('|')[1]);
                //                } break;
                //        }
                //    }
                return true;
            }
            catch (Exception ex) {  Utilities.LogServices.Log(ex.Message); }
            return false;
        }
        public void SaveSettings(string location)
        {
            Utilities.LogServices.Log("Saving Settings");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\");
            using (StreamWriter file = new StreamWriter(location))
                diskio.Serialize(file, this);
            //{
            //    file.WriteLine("WLO Private Server Settings");
            //    file.WriteLine("[Version]");
            //    file.WriteLine("V8");
            //    file.WriteLine(";");
            //    file.WriteLine(";");
            //    file.WriteLine(";");
            //    file.WriteLine(";");
            //    file.WriteLine("[Settings]");
            //    foreach (var y in )
            //    {

            //    }
            //    file.WriteLine(";");
            //}
        }
    }
}
