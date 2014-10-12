using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Config
{
    public enum UpdtSetting
    {
        Never,
        Auto,
        AutoandForce,
    }
    public static class Update
    {

        public static UpdtSetting UpdtControl;
    }
}
