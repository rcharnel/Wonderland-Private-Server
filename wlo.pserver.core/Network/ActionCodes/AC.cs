using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Game;
using Network;


namespace Network.ActionCodes
{
    public class AC
    {
        public virtual int ID { get { return 0; } }
        public virtual void ProcessPkt(Player c, RecievePacket p)
        {
            switch (p.B)
            {
                default: DebugSystem.Write("Action Code " + p.A + "," + p.B + "has not been coded"); break;
            }
        }

        static readonly object mlock = new object();
        static Dictionary<int, AC> AcList = new Dictionary<int, AC>(100);

        public static AC GetAction(int ID)
        {

            if (AcList.ContainsKey(ID))
                return AcList[ID];

            lock (mlock)
            {
                AC resp = null;
                if (resp == null)
                {
                    foreach (var y in (from c in Assembly.GetEntryAssembly().GetTypes()
                                       where c.IsClass && !c.IsAbstract && c.IsPublic && c.IsSubclassOf(typeof(AC))
                                       select c))
                    {
                        AC m = null;
                        try
                        {
                            m = (Activator.CreateInstance(y) as AC);
                            if (m.ID == ID)
                            {
                                AcList.Add(m.ID, m);
                                return m;
                            }                                
                        }
                        catch { DebugSystem.Write(new ExceptionData(ExceptionSeverity.Warning, "failed to load AC " + m.ID)); m = null; }
                    }
                }
                return resp;
            }
        }
    }
}
