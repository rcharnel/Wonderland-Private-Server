using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using Server.Task;

namespace Server
{
    
   
    public class TaskManager
    {
        public BindingList<taskItem> TaskItems;

        public TaskManager()
        {
            TaskItems = new BindingList<taskItem>();
        }

        public void CreateTask(taskItem Task)
        {
            DebugSystem.Write(DebugItemType.Info_Heavy, "Creating new Task - " + Task.TaskName + " with interval -" + Task.Interval);

            if (TaskItems.Count(c => c.TaskName == Task.TaskName) == 0) TaskItems.Add(Task);
        }
        public void ChangeInterval(string name, TimeSpan src)
        {
            //DebugSystem.Write("Changing interval on Task - " + name + " with new interval -" + src, LogType.TASK);
            //for (int a = 0; a < TaskItems.Count; a++)
            //    if (TaskItems[a].TaskName == name)
            //        TaskItems[a].interval = src;
        }
        public void EndTask(string name)
        {
            DebugSystem.Write(DebugItemType.Info_Heavy, "Ending Task - " + name);
            if (TaskItems.Count(c => c.TaskName == name) > 0)
                TaskItems.Remove(TaskItems.Single(c => c.TaskName == name));
        }
        public void onUpdateTick()
        {
            for (int a = 0; a < TaskItems.Count; a++)

                if (!TaskItems[a].SystemTask && TaskItems[a].EndofLife <= DateTime.Now)
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
