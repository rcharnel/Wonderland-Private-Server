using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Utilities.Task
{
    public class ApplicationCheckUpdate_Task : taskItem
    {

        public ApplicationCheckUpdate_Task(TimeSpan src):base("Application Update",src)
        {
        }
        protected override void TaskWrk()
        {
            try
            {
                if (cGlobal.GClient != null)
                {
                    DebugSystem.Write("Checking for Update", LogType.UPDT);
                    cGlobal.GClient.CheckFor_Update();
                }
            }
            catch (Exception y) { DebugSystem.Write(y); status = "Failed"; }
        }

    }

    public class Application_Update_Warning:taskItem
    {
        public Application_Update_Warning(TimeSpan src):base("Application Update Notification",src)
        {

        }
        protected override void TaskWrk()
        {
            try
            {

                if (cGlobal.WLO_World != null)
                {
                }
            }
            catch (Exception y) { DebugSystem.Write(y); status = "Failed"; }
        }
    }

    public class Application_UpdateTask:taskItem
    {
        public Application_UpdateTask(TimeSpan src)
            : base("Updating Application", src)
        {

        }
        protected override void TaskWrk()
        {
            try
            {
                
            }
            catch (Exception y) { DebugSystem.Write(y); status = "Failed"; }
        }
    }
}
