using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Wonderland_Private_Server.Utilities
{
    public enum LogType
    {
        SYS,
        ERR,
        NET,
        UPDT,
        DB,

    }
    public struct LogItem
    {
        public DateTime happenat;
        public LogType eventtype;
        public string message;
        public string where;
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public static class LogServices
    {
        public static ConcurrentQueue<LogItem> LogHistory = new ConcurrentQueue<LogItem>();

        /// <summary>
        /// Logs a system/Error event
        /// </summary>
        /// <param name="info"></param>
        public static void Log(object info, LogType stype = LogType.SYS)
        {
            if (info is string)
            {
                var sys = new LogItem() { happenat = DateTime.Now, eventtype = stype, message = info.ToString() };
                LogHistory.Enqueue(sys);
            }
            else if (info is Exception)
            {
                //Handle Exceptions differently
                LogItem error = new LogItem();
                error.message = (info as Exception).Message;
                error.where = (info as Exception).StackTrace;
                error.eventtype = LogType.ERR;
                error.happenat = DateTime.Now;
                LogHistory.Enqueue(error);
            }

        }

    }
}
