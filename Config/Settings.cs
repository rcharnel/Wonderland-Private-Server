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
        public UpdateSetting Update;
        
        public Settings()
        {
            diskio = new XmlSerializer(this.GetType());
            Update = new UpdateSetting();
        }
        public void SaveSettings(string location)
        {
            Utilities.LogServices.Log("Saving Settings");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\");
            using (StreamWriter file = new StreamWriter(location))
                diskio.Serialize(file, this);
        }
    }
}
