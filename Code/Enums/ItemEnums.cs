using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Enums
{
    public enum eWearSlot
    {
        none = 0,
        head = 1,
        body = 2,
        hand = 3,
        arm = 4,
        feet = 5,
        special = 6

    }
    public enum eItemType
    {
        unknown = -1,
        none = 0,
        single_edge = 1,
        sword = 2,
        spear = 3,
        bow = 4,
        wand = 5,
        claw = 6,
        axe = 8,
        pestle = 9,
        Fan = 10,
        Gun = 11,
        bodyarmor = 12,
        headwear = 13,
        armitem = 14,
        footwear = 15,
        tent = 27,

    }

    public enum eSpecialStatus
    {
        Monster_Records = 101,
        Critical_Increase = 137,
    }
}
