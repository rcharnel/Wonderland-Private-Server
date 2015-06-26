using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;

namespace Server
{
    public class taskItem : INotifyPropertyChanged
    {
        bool update;
        public event PropertyChangedEventHandler PropertyChanged;

        protected string taskname;
        public TimeSpan interval;
        public DateTime Createdat,EndofLife;
        protected DateTime RanAt, nextExec,guidelay;
        protected string status;
        protected bool systemTask;
        Thread tskwrk;
        public taskItem(string name,TimeSpan time = new TimeSpan())
        {
            Createdat = DateTime.Now;
            taskname = name;
            if (time == new TimeSpan())
                interval = new TimeSpan(0, 0, 30);
            else
                interval = time;

            nextExec = DateTime.Now.Add(interval);
        }

        #region Properties
        [Browsable(false)]
        public bool SystemTask
        {
            get { return systemTask; }
        }
        public string TaskName
        {
            get { return taskname; }
            set { taskname = value; }
        }
        public string Interval
        {
            get { return string.Format("Every {0}",TimeStringFormat(interval)); }
        }
        public string LastExecution
        {
            get {
                if (RanAt == new DateTime())
                    return "Did not Run";
                else
                    return RanAt.ToShortTimeString();
            }
        }
        public string NextExecution
        {
            get { return string.Format("{0}",TimeStringFormat(TimeDifference(DateTime.Now,nextExec))); }
        }
        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        #endregion

        public void onRefreshGui()
        {
            if (guidelay < DateTime.Now)
            {
                guidelay = DateTime.Now.AddSeconds(1);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Interval"));
                    PropertyChanged(this, new PropertyChangedEventArgs("LastExecution"));
                    PropertyChanged(this, new PropertyChangedEventArgs("NextExecution"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        public virtual void onRetry()
        {
            if (tskwrk != null) { MessageBox.Show("Task already Running"); return; }
            nextExec = new DateTime();
            RanAt = DateTime.Now;
            tskwrk = new Thread(new ThreadStart(TaskWrk));
            tskwrk.Name = taskname;
            tskwrk.Init();
        }
        public virtual void onCancel()
        {
            if (tskwrk == null) { MessageBox.Show("Task not running"); return; }
            if (tskwrk.IsAlive)
                tskwrk.Abort();
        }
        public virtual void onRefresh()
        {
            if (update) return;
            update = true;            

            if(nextExec <= DateTime.Now && tskwrk == null)
            {
                RanAt = DateTime.Now;
                tskwrk = new Thread(new ThreadStart(TaskWrk));
                tskwrk.Name = taskname;
                tskwrk.Init();
                status = "Running";
            }
            else if(tskwrk != null)
            {
                if (tskwrk.ThreadState == ThreadState.AbortRequested || tskwrk.ThreadState == ThreadState.StopRequested || tskwrk.ThreadState == ThreadState.Aborted)
                {
                    status = "Cancelled";
                    tskwrk = null;
                    nextExec = DateTime.Now.Add(interval);
                }
                else if (tskwrk.ThreadState == ThreadState.Running)
                    status = "Running";
                else if (tskwrk.ThreadState == ThreadState.Stopped)
                {
                    status = "Completed";
                    tskwrk = null;
                    nextExec = DateTime.Now.Add(interval);
                }
            }
            update = false;
        }

        TimeSpan TimeDifference(DateTime old, DateTime cur)
        {
            return (cur - old);
        }
        string TimeStringFormat(TimeSpan src)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", (src.Hours > 0) ? src.Hours.ToString() : "", (src.Hours > 0) ? "h" : "", (src.Minutes > 0 && src.Hours > 0) ? ":" : "", (src.Minutes > 0) ? src.Minutes.ToString() : "", (src.Minutes > 0) ? "m" : "", (src.Seconds > 0 && src.Minutes > 0) ? ":" : "", (src.Seconds > 0) ? src.Seconds.ToString() : "", (src.Seconds > 0) ? "s" : "");
        }

        protected virtual void TaskWrk()
        {

        }
    }

}
