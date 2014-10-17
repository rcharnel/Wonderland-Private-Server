using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Utilities.Task
{
    public class ApplicationUpdate_Task : taskItem
    {

        public ApplicationUpdate_Task():base("Application Update",new TimeSpan(0,5,25))
        {
        }
        protected override void TaskWrk()
        {
            try
            {
                if (cGlobal.GClient != null)
                    cGlobal.GClient.CheckFor_Update();
            }
            catch (Exception y) { Utilities.LogServices.Log(y); status = "Failed"; }
        }

    }
}
