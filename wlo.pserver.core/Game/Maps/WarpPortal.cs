using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Maps
{
    [Flags]
    public enum AccessFlags
    {
        None,
        Any = 1,
    }
    public class WarpPortal
    {
        public int clickID;
        public int DstID;
        public int x, y;
        public AccessFlags accessBy;
        public bool Enter(ref Player src)
        {
            //portal Requirements
            return true;
        }
    }
}
