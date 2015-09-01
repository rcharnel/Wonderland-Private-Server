using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Maps
{
    public class InteractableObjects : MapObject
    {
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
