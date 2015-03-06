using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace Wonderland_Private_Server.Code.Interface
{
    public interface Fighter
    {
        uint ID { get; }
        BattleSide BattlePosition { get; set; }
        eFighterType TypeofFighter { get; }
        FighterState BattleState { get; }
        Game.Code.PetRelated.PetList Pets { get; }
        //BattleAction myAction { get; set; }
        UInt16 ClickID { get; set; }
        UInt32 OwnerID { get; set; }
        byte Level { get; }
        byte GridX { get; set; }
        byte GridY { get; set; }
        bool ActionDone { get; }
        Int32 CurHP { get; set; }
        Int32 CurSP { get; set; }
        DateTime RdEndTime { set; }
        Int32 MaxHP { get; }
        Int16 MaxSP { get; }
        Affinity Element { get; }
        RebornJob Job { get; }
        //Skill SkillEffect { get; set; }
        bool Reborn { get; }

        Int32 FullMatk { get; }
        Int32 FullAtk { get; }
        Int32 FullDef { get; }
        Int32 FullMdef { get; }
        Int32 FullSpd { get; }
    }
}
