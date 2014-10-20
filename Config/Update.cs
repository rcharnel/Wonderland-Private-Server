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
    public class UpdateSetting
    {

        public UpdtSetting UpdtControl;
        public TimeSpan UpdtChk_Interval;
        public TimeSpan AutoUpdt_Schedule;
        public string GitBranch;

        public string[] Data
        {
            get
            {
                return new string[]
                {

                };
            }
        }
    }
}
