using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Wonderland_Private_Server.Utilities
{
    struct LogItem
    {
        public DateTime happenat;
        public string eventtype;
        public string message;
        public string where;
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public static class LogServices
    {
        static ConcurrentQueue<LogItem> LogHistory;


        /// <summary>
        /// Logs a system/Error event
        /// </summary>
        /// <param name="info"></param>
        public static void Log(object info, string stype = "")
        {
            if (info is string)
                LogHistory.Enqueue(new LogItem() { happenat = DateTime.Now, eventtype = (stype == "") ? "System" : stype, message = info.ToString() });
            else if (info is Exception)
            {
                //Handle Exceptions differently
                LogItem error = new LogItem();
                error.message = (info as Exception).Message;
                error.where = (info as Exception).StackTrace;
                error.eventtype = "Error";
                error.happenat = DateTime.Now;
                LogHistory.Enqueue(error);
            }

        }

    }
}
