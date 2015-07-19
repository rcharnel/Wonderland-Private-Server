using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GupdtSrv;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Net;

namespace Server.System
{
    
    public class ApplicationUpdateCheck_Task : taskItem
    {

        public ApplicationUpdateCheck_Task(TimeSpan src)
            : base("Application Update", src)
        {
            systemTask = true;
        }        

        protected override void TaskWrk()
        {

            if (cGlobal.Update_System != null)
            {
                DebugSystem.Write(DebugItemType.Info_Heavy, "Checking for Application Update");
                cGlobal.Update_System.CheckForUpdates_Application();
            }
        }

    }
    public class MapUpdateCheck_Task : taskItem
    {

        public MapUpdateCheck_Task(TimeSpan src)
            : base("Map Update", src)
        {
            systemTask = true;
        }

        protected override void TaskWrk()
        {

            if (cGlobal.Update_System != null)
            {
                DebugSystem.Write(DebugItemType.Info_Heavy, "Checking for Map Updates");
                cGlobal.Update_System.CheckForUpdates_Map();
            }
        }

    }


    public class UpdateSystem
    {
        public Form MainFrm { internal get; set; }
        public Version AppVer;
        public FlowLayoutPanel AppUpdtPanel { internal get; set; }
        public FlowLayoutPanel MapUpdtPanel { internal get; set; }
        gitClient GitClient;

        WebClient webclient;

        Octokit.Release LatestVer;

        public UpdateSystem()
        {
            GitClient = new gitClient();
            GitClient.onError += client_onError;
            GitClient.infopipe += client_infopipe;
            GitClient.onNewUpdate +=GitClient_onNewUpdate;
            CheckForUpdates_Application();
            CheckForUpdates_Map();

            DebugSystem.Write("[Init] - Creating Update Tasks");
            cGlobal.ApplicationTasks.CreateTask(new ApplicationUpdateCheck_Task(new TimeSpan(0, 10, 0)));
            cGlobal.ApplicationTasks.CreateTask(new MapUpdateCheck_Task(new TimeSpan(0, 5, 30)));
            AppVer = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).FileVersion);

        }

        

        public void CheckForUpdates_Map()
        {
            //GitClient.
        }

        public async void CheckForUpdates_Application()
        {
            if (await GitClient.CheckFor_Update("Wonderland-Private-Server", "Develop", AppVer))
            {
                Thread.Sleep(3000);
                if (cGlobal.SrvSettings.Update.EnableSchedUpdate)
                {
                }
                else if (cGlobal.gLoginServer.Count == 0)
                {
                    cGlobal.gLoginServer.Kill();
                    cGlobal.Run = false;
                    DebugSystem.Write("Preparing for update");
                    DebugSystem.Write("Launching Updater");
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = Environment.CurrentDirectory + "\\WloPSrvUpdater.exe";
                    start.Arguments = "Develop" + " " + AppVer + " " + LatestVer.TagName;
                    Process.Start(start);         
                }
            }
        }

        void GitClient_onNewUpdate(string repo, Octokit.Release latest, List<Octokit.Release> listofupdates)
        {
            switch (repo)
            {
                case "Wonderland-Private-Server":
                    {

                        MainFrm.BeginInvoke(new Action(() =>
                        {
                            AppUpdtPanel.SuspendLayout();
                            AppUpdtPanel.Controls.Clear();

                            LatestVer = latest;

                            foreach (var y in listofupdates.Where(c => c.TagName != null).OrderByDescending(c => new Version(c.TagName)))
                                AppUpdtPanel.Controls.Add(new Gui.Update.GitUpdateItem(AppVer, y));
                            AppUpdtPanel.ResumeLayout();
                            AppUpdtPanel.Refresh();
                        }));
                    } break;
            }
            //    //switch ((Server.Config.UpdtSetting)GitUptOption.SelectedIndex)
            //    //{
            //    //    case Server.Config.UpdtSetting.Auto:
            //    //        {
            //    //            DateTime tomorrow = new DateTime(DateTime.Now.Year, DateTime.Now.AddDays(1).Month, DateTime.Now.AddDays(1).Day, 1, 0, 0);
            //    //            //cGlobal.ApplicationTasks.CreateTask("Updating Application", tomorrow - DateTime.Now);
            //    //        } break;
            //    //    case Server.Config.UpdtSetting.AutoandForce:
            //    //        {

            //    //        } break;
            //    //}
        }

        void client_infopipe(object sender, string e)
        {
            DebugSystem.Write(DebugItemType.Info_Heavy, "GitUpdate: " + e);
        }

        void client_onError(object sender, Exception e)
        {
            DebugSystem.Write(new ExceptionData(e));
        }
    }
}
