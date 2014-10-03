using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Wonderland_Private_Server.Config
{
    public class Settings
    {
        readonly object m_Lock = new object();
        Dictionary<string, string> Config
        {
            get
            {
                var test =  new Dictionary<string, string>() ;
                return test;
            }
        }
        public string this[string key]
        {
            get
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, return its value.
                    if (Config.ContainsKey(key))
                    {
                        // The key was found; return its value. 
                        return Config[key];
                    }
                    return "";
                }
            }

            set
            {
                lock (m_Lock)
                {
                    // If this key is in the dictionary, change its value. 
                    if (Config.ContainsKey(key))
                    {
                        // The key was found; change its value.
                        Config[key] = value;
                    }
                    else
                    {
                        Config.Add(key, value);
                    }
                }
            }
        }

        public Settings()
        {
            Reset();
        }
        public void Reset()
        {
            Config.Clear();
        }
        public bool LoadSettings()
        {
            try
            {
                string line = ""; string ver = "";
                using (StreamReader file = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.wlo"))
                    while ((line = file.ReadLine()) != null)
                    {
                        switch (line)
                        {
                            case "[Version]":
                                {
                                    ver = file.ReadLine();
                                    if (ver != "V9") return false;
                                } break;
                            case "[Settings]":
                                {
                                    if (ver == "") return false;
                                    Config.Clear();
                                    while ((line = file.ReadLine()) != ";")
                                        Config.Add(line.Split('|')[0], line.Split('|')[1]);
                                } break;
                        }
                    }
                return true;
            }
            catch (Exception ex) {  Utilities.LogServices.Log(ex.Message); }
            return false;
        }
        public void SaveSettings()
        {
            Utilities.LogServices.Log("Saving Settings");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\");
            using (StreamWriter file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.wlo"))
            {
                file.WriteLine("WLO Private Server Settings");
                file.WriteLine("[Version]");
                file.WriteLine("V8");
                file.WriteLine(";");
                file.WriteLine(";");
                file.WriteLine(";");
                file.WriteLine(";");
                file.WriteLine("[Settings]");
                foreach (var f in Config.Keys)
                {
                    file.WriteLine(string.Format("{0}|{1}", f, Config[f]));
                }
                file.WriteLine(";");
            }
        }
    }
}
