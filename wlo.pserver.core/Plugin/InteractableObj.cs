using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace Plugin
{
    public enum InteractableObjType
    {
        Npc,
        Chest,
        Object,//rest of them :D
        Machine,
        SpecialNpc,//shopkeeper,witch doctor, event giving npc
        Door,
        Mob,
    }
    public interface InteractableObj
    {
        InteractableObjType ObjType { get; }
        ushort[] MapLoc { get; }
        ushort clickID { get; }
        bool isVisible { get; }
        void Interact(ref Player p, int? answer = null);

    }

    public interface Mob : InteractableObj
    {
        void Walk();
        bool canAttack(Player p);

        
    }
}
