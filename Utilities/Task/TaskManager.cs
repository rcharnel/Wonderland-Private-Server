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

            foreach (var y in Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsClass && c.IsPublic && c.IsSubclassOf(typeof(taskItem))))
            {

                TaskItems.Add((Activator.CreateInstance(y) as taskItem));
            }
        }

        public void onUpdateTick()
        {
            foreach(var t in TaskItems)
                t.onRefresh();
        }
        public void onUpdateGuiTick()
        {
            foreach (var t in TaskItems)
                t.onRefreshGui();
        }
    }
}
