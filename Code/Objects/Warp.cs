using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Objects
{
    public class WarpData
    {
        public ushort ID;
        public ushort DstMap;
        public ushort DstX_Axis;
        public ushort DstY_Axis;
    }
    public class WarpPortal
    {
        public ushort ID;
        public int Dst;
        protected int accessLvl;
        public virtual bool Enter(ref Player src)
        {
            //portal Requirements
            return false;
        }
    }
}
