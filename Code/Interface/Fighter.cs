using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.Code.Interface
{
    public interface Fighter
    {
        uint ID { get; }
        BattleSide BattlePosition { get; set; }
        eFighterType TypeofFighter { get; }
        PlayerState State { get; }
        cPetList Pets { get; }
        //BattleAction myAction { get; set; }
        UInt16 ClickID { get; set; }
        UInt16 OwnerID { get; set; }
        byte Level { get; }
        byte GridX { get; set; }
        byte GridY { get; set; }
        bool ActionDone { get; }
        Int32 CurHP { get; set; }
        Int32 CurSP { get; set; }
        DateTime RdEndTime { set; }
        Int32 MaxHP { get; }
        Int16 MaxSP { get; }
        ElementType Element { get; }
        RebornJob Job { get; }
        bool Reborn { get; }

    }
    public interface PetFighter
    {
        uint ID { get; set; }
        BattleSide BattlePosition { get; set; }
        eFighterType TypeofFighter { get; }
        PetFighterState State { get; }
        DateTime RdEndTime { set; }
        UInt16 GridX { get; set; }
        UInt16 GridY { get; set; }
        bool ActionDone { get; }
        Int32 CurHP { get; set; }
        Int32 CurSP { get; set; }
    }
}
