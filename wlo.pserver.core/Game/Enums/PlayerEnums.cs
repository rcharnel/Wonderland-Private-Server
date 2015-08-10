using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum SendMode
    {
        Normal,
        Multi,
    }
    /// <summary>
        /// Flags that tell what a player is doing
        /// </summary>
        public enum PlayerFlag
        {
            Creating_Character,
            Logging_into_Map,
            Warping,
            InGame,
            InMap,
            InTent,
        }

}
