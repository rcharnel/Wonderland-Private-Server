using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace Game.Battle
{

    public delegate void BattleRoundInfo(List<Game.Battle.Fighter> fighters_on_my_side, List<Game.Battle.Fighter> fighters_on_other_side);
         
    /// <summary>
    /// Object that will house information pertaining to battle
    /// 
    /// this information is based on which side the player is on and who is on the same side
    /// </summary>
    public class BattleScene
    {
        readonly Battle _battleref;
        readonly BattleRole role;

        int round;

        List<Fighter> fighterlist; public IReadOnlyList<Fighter> FighterList { get { return fighterlist; } }


        public event BattleRoundInfo onRoundStart;
        public event EventHandler onBattleOver;

        public BattleScene(BattleRole sideRole, Battle owner)
        {
            _battleref = owner;
            role = sideRole;

            round = 0;
            fighterlist = new List<Fighter>();
        }

        public int Total_Fighters { get { return fighterlist.Count; } }
        public int Total_Fighters_Alive { get { return fighterlist.Count(c => c.CurHP > 0); } }

        #region Methods
        /// <summary>
        /// Used to Poll if all fighters have finished what they need to do
        /// </summary>
        public bool EveryoneReady
        {
            get
            {
                if (fighterlist.Count(c => c.ActionDone) == fighterlist.Count)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool AvailableSlot(bool IsPlayer, bool IsPet, bool IsMob, out byte[] location)
        {
            byte x, y;

            if (IsPlayer)
            {
                x = 0;
                y = 2;
                switch (role)
                {
                    case BattleRole.Defending: x = 1; break;
                    case BattleRole.Attacking: x = 4; break;
                }
                switch (fighterlist.Count)
                {
                    case 0: break;
                    case 1: y += 1; break;
                    case 2: y -= 1; break;
                    case 3: y += 2; break;
                }
                location = new byte[] { x, y };
                return HasFighter(location);
            }

            location = new byte[0];
            return false;
        }

        /// <summary>
        /// Detmines if a a specific location has a fighter
        /// </summary>
        /// <param name="location">Location to check</param>
        /// <returns> true if theres a fighter</returns>
        bool HasFighter(byte[] location)
        {
            return false;
        }
        /// <summary>
        /// Detmines if a a specific location has a fighter
        /// </summary>
        /// <param name="location">Location to check</param>
        /// <param name="IsAlive">is the fighter alive</param>
        /// <returns> true if theres a fighter</returns>
        bool HasFighter(byte[] location, out bool IsAlive)
        {
            IsAlive = false;
            return false;
        }


       public void OnNewRound()
        {
            if (onRoundStart != null)
            {
                BattleRole rndptr = (role == BattleRole.Attacking) ? BattleRole.Defending : BattleRole.Attacking;
                onRoundStart(fighterlist, _battleref[rndptr].fighterlist);
            }
        }


        bool OnNewFighter(Fighter src)
        {
            src.OnNewBattle(this);

            byte[] loc;

            if (src.TypeofFighter == eFighterType.player && AvailableSlot(true, false, false, out loc))
            {
                src.GridX = loc[0];
                src.GridY = loc[1];

                //if (src.Pets.BattlePet != null)
                //{
                //    //Fighter pet = new Fighter(g);
                //    //pet.SetFrom(fighter.playerinfo.pets.GetPetinBattleMode());//finish later
                //    //pet.actionDone = false;
                //    //if (fighter.starter)
                //    //    pet.starter = true;
                //    //pet.myBattleType = Fighter.eFighterType.Pet;
                //    //pet.GridX = (byte)(fighter.GridX - 1);
                //    //pet.GridY = fighter.GridY;
                //    //pet.myplayerInfo = fighter.playerinfo;
                //    //pet.ownerID = fighter.ID;
                //    //pet.clickID = fighter.clickID;//not the right one
                //    //fighterlist.Add(pet);
                //}
                fighterlist.Add(src);
            }
            else if (src.TypeofFighter == eFighterType.Npc_Mob && AvailableSlot(false, false, true, out loc))
            {
                src.GridX = loc[0];
                src.GridY = loc[1];
                fighterlist.Add(src);
            }

            return false;
        }

        void FighterLeft(eBattleLeaveType exit, Fighter fighter)
        {

        }



        #endregion







        public void ProcessSocket(RecievePacket g)
        {
        }

    }
}
