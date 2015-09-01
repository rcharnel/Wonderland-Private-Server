using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Maps
{
    public class MapObject
    {
        public virtual MapObjType Type { get { return MapObjType.None; } }
        public virtual ushort CickID { get { return 0; } }
        public bool IsVisible { get; set; }

        public virtual void Process()
        {
        }
    }
}
