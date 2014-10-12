using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Objects
{
    public class WarpData : Maps.MapObject
    {
        public override Enums.MapObjType Type
        {
            get
            {
                return Enums.MapObjType.WarpData;
            }
        }
        public virtual ushort DstMap { get; set; }
        public virtual ushort DstX_Axis { get; set; }
        public virtual ushort DstY_Axis { get; set; }
    }

    public class WarpPortal:Maps.MapObject
    {
        public virtual int Dst { get { return 0; } }
        public override Enums.MapObjType Type
        {
            get
            {
                return Enums.MapObjType.WarPortal;
            }
        }
        protected virtual int accessLvl { get { return 0; } }        
        public virtual bool Enter(ref Player src)
        {
            //portal Requirements
            return true;
        }
    }
}
