using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Maps
{
    /// <summary>
    /// Class for Quest related Npcs or Npcs that respond in game to interactions
    /// 
    /// Uses EvaluateQuestData to handle packets related to position and visibility per player
    /// 
    /// </summary>
    public class QuestNpc : InteractableObjects
    {
        public virtual string Name { get { return ""; } }

        public virtual void EvaluateQuestData(Player src)
        {
        }

        public virtual void Interact(Player src)
        {
            throw new NotImplementedException();
        }
        public virtual void Interact(Player src, byte? answer = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Interact(Player src, byte? answer = null, params Code.ShoppingCart[] items)
        {
            throw new NotImplementedException();
        }
    }
}
