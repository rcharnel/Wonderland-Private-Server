using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Wonderland_Private_Server.Utilities.Task
{
    public class TaskManager
    {
        public BindingList<taskItem> TaskItems;

        public TaskManager()
        {
            TaskItems = new BindingList<taskItem>();
        }

        public void CreateTask(string name,TimeSpan interval)
        {
            DebugSystem.Write("Creating new Task - " + name + " with interval -" + interval, LogType.TASK);
            switch (name)
            {
                case "Application Update": if (TaskItems.Count(c => c.TaskName == name) == 0) TaskItems.Add(new ApplicationCheckUpdate_Task(interval)); break;
                case "Updating Application":
                    {
                        if (cGlobal.WLO_World != null && TaskItems.Count(c => c.TaskName == "Application Update Notification") == 0)
                        {
                            var time = DateTime.Now.Add(cGlobal.SrvSettings.Update.AutoUpdt_Schedule).Subtract(new TimeSpan(0, 5, 0));

                            var tmp = new Application_Update_Warning(new TimeSpan(0, (int)time.Minute / 2, 0));
                            tmp.EndofLife = time;
                            TaskItems.Add(tmp);
                        }

                        if (TaskItems.Count(c => c.TaskName == name) == 0) TaskItems.Add(new ApplicationCheckUpdate_Task(interval)); 
                    } break;
            }
        }
        public void ChangeInterval(string name, TimeSpan src)
        {
            DebugSystem.Write("Changing interval on Task - " + name + " with new interval -" + src, LogType.TASK);
            for (int a = 0; a < TaskItems.Count; a++)
                if (TaskItems[a].TaskName == name)
                    TaskItems[a].interval = src;
        }
        public void EndTask(string name)
        {
            DebugSystem.Write("Ending Task - " + name, LogType.TASK);
            if (TaskItems.Count(c => c.TaskName == name) > 0)
                TaskItems.Remove(TaskItems.Single(c => c.TaskName == name));
        }
        public void onUpdateTick()
        {
            for (int a = 0; a < TaskItems.Count; a++)
                if (TaskItems[a].EndofLife <= DateTime.Now)
                    EndTask(TaskItems[a].TaskName);

            foreach (var t in TaskItems)
                t.onRefresh();
        }
        public void onUpdateGuiTick()
        {
            foreach (var t in TaskItems)
                t.onRefreshGui();
        }
    }
}
