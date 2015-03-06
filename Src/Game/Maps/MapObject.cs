using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

namespace Game.Maps
{
    public class MapObject
    {
        //public NpcEntries ObjInfo { private get; set; }
        //public InteractableObj DataOverride { private get; set; }
        //public InteractableObjType ObjType { get; set; }

        public virtual MapObjType Type { get { return MapObjType.None; } }
        public virtual ushort CickID { get; set; }
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
