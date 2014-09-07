using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Config
{
    static class Repository
    {

        public static void Save()
        {
            var config_location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +"\\WonderlandOnline-PrivateServer";

            if (!System.IO.Directory.Exists(config_location))
                System.IO.Directory.CreateDirectory(config_location);



        }
    }
}
