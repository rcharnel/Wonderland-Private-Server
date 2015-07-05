using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GupdtSrv;

namespace Server.Task
{
    public class ApplicationCheckUpdate_Task : taskItem
    {
        public System.Windows.Forms.Form MainFrm { internal get; set; }
        public System.Windows.Forms.FlowLayoutPanel UpdtPanel { internal get; set; }

        public ApplicationCheckUpdate_Task(TimeSpan src)
            : base("Application Update", src)
        {
            systemTask = true;
            cGlobal.GitClient.onError += client_onError;
            cGlobal.GitClient.infopipe += client_infopipe;
            cGlobal.GitClient.onNewUpdate += client_onNewUpdate;
        }

        void client_onNewUpdate(object sender, Octokit.Release e)
        {
            //switch ((Server.Config.UpdtSetting)GitUptOption.SelectedIndex)
            //{
            //    case Server.Config.UpdtSetting.Auto:
            //        {
            //            DateTime tomorrow = new DateTime(DateTime.Now.Year, DateTime.Now.AddDays(1).Month, DateTime.Now.AddDays(1).Day, 1, 0, 0);
            //            //cGlobal.ApplicationTasks.CreateTask("Updating Application", tomorrow - DateTime.Now);
            //        } break;
            //    case Server.Config.UpdtSetting.AutoandForce:
            //        {

            //        } break;
            //}
        }

        void client_infopipe(object sender, string e)
        {
            DebugSystem.Write(DebugItemType.Info_Heavy, "GitUpdate: " + e);
        }

        void client_onError(object sender, Exception e)
        {
            DebugSystem.Write(new ExceptionData(e));
        }

        protected override void TaskWrk()
        {

            if (cGlobal.GitClient != null)
            {
                DebugSystem.Write(DebugItemType.Info_Heavy, "Checking for Update");
                cGlobal.GitClient.CheckFor_Update();
            }
        }

    }

    public class Application_Update_Warning:taskItem
    {
        public Application_Update_Warning(TimeSpan src):base("Application Update Notification",src)
        {

        }
        protected override void TaskWrk()
        {
            //try
            //{

            //    if (cGlobal.WLO_World != null)
            //    {
            //    }
            //}
            //catch (Exception y) { DebugSystem.Write(y); status = "Failed"; }
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
            //try
            //{
                
            //}
            //catch (Exception y) { DebugSystem.Write(y); status = "Failed"; }
        }
    }
}
