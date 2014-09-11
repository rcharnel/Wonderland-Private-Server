using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Maps
{
    public class MapObject
    {
        //public NpcEntries ObjInfo { private get; set; }
        //public InteractableObj DataOverride { private get; set; }
        //public InteractableObjType ObjType { get; set; }
        public ushort CickID { get; set; }
        public bool IsVisible { get; set; }
        public bool CanAttack(Player p)
        {
            return false;
        }


        public void Walk()
        {
        }

        public void Interact(ref Player p, int? answer = null)
        {

        }
    }
}
