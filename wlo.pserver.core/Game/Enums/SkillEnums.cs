using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum SkillTarget
    {
        Any_Enemy = 1,
        Any_Ally = 2,
        Self = 3,
        Pet = 4,
        Any_Ally_but_Self = 5,
    }
    public enum SkillEffectType
    {
    }
    public enum Seal_Shield
    {
        None,
        Sealing_Skill,
        Shielding_Skill,
    }
    public enum EffectLayer
    {
        None,
        Physical = 1,
        Magical = 2,
        Seals1,
        Buffs1 = 4,
        Debuffs1,
        Mana,
        Healing1,
        Revival,
        Defend = 10,
        Flee = 12,
        Seals2,
    }
    public enum EffectType
    {
        None,
    }
    public enum AdditionalEf
    {
        Mess = 1,
        HitSelf = 2,
        Freeze,

    }
    public enum AttackType
    {
        Near = 1,
        Distance1 = 2,
        Distance2 = 3,
        Outside_of_Battle = 4,
    }
    public enum AttackPattern
    {
        Single,
        HorizontalLine,
        VerticalLine,

    }
    public enum ReqWeaponType
    {
        None = 0,
        Arrow = 1,
        Gun = 2,
        undefined = 242,
    }

    public enum SkillType
    {
        None,
    }
}
