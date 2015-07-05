using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    public class WarpData
    {
        public virtual ushort DstMap { get; set; }
        public virtual ushort DstX_Axis { get; set; }
        public virtual ushort DstY_Axis { get; set; }
        public WarpData(WarpInfo src = null)
        {
            if (src != null)
            {
                DstMap = src.mapID;
                DstX_Axis = (ushort)src.x;
                DstY_Axis = (ushort)src.y;
            }
        }
    }

    //public class WarpPortal
    //{
    //    public virtual int Dst { get { return 0; } }
    //    protected virtual int accessLvl { get { return 0; } }        
    //    public virtual bool Enter(ref Player src)
    //    {
    //        //portal Requirements
    //        return true;
    //    }
    //}
}
