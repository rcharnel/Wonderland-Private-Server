using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Maps.Islands.NorthIsland.NorthIsland60000.Portals
{
    public class toSB : WarpData
    {
        public override ushort CickID
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }
        public override ushort DstMap
        {
            get
            {
                return 11016;
            }
            set
            {
            }
        }
        public override ushort DstX_Axis
        {
            get
            {
                return 1181;
            }
            set
            {
            }
        }
        public override ushort DstY_Axis
        {
            get
            {
                return 243;
            }
            set
            {
            }
        }
    }

    public class toSB_Portal:WarpPortal
    {
        public override ushort CickID
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }
        public override int Dst
        {
            get
            {
                return 1;
            }
        }
    }
}
