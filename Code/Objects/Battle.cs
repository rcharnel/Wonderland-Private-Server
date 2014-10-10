using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Interface;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    /*
     * 
     * 50,1 s 50,1(player x,y,enermy x,y,skill ushort, timer,0
( (flag:gb)  50.1 s (player x, y, enermy x, y, ushort skill, timer 0 )
     * 
     * */
    public class BattleAction
    {
        public Fighter src;
        public Fighter dst;
        public Skill skill;
        public byte unknownbyte;
        public byte unknownbyte2;
    }

    public class Battle
    {
        readonly object mylock = new object();
        bool blockupdt;

        #region Definitions
        DateTime roundend_time;

        public Dictionary<byte, BattleArea> Side;

        public eBattleType TypeofBattle;
        public eBattleState BattleState;
        public eBattleRoundState RoundState;

        public UInt16 Background,battleID;
        public Fighter startedby;
        
        #endregion
        public UInt16 BattleID { get { return battleID; } }
        public BattleArea this[BattleSide key]
        {
            get
            {
                lock (mylock)
                {
                    // If this key is in the dictionary, return its value.
                    if (Side.Values.Count(c=>c.Position == key) > 0)
                    {
                        // The key was found; return its value. 
                        return Side.Values.Single(c=>c.Position == key);
                    }
                    else
                    {
                        // The key was not found; return null. 
                        return null;
                    }
                }
            }
        }
        

        public Battle(UInt16 BG,int BattleID)
        {
            Background = BG;
            Side = new Dictionary<byte, BattleArea>();
            Side.Add(2, new BattleArea(2, this));
            Side.Add(4, new BattleArea(4, this));
            Side.Add(5, new BattleArea(5, this));
        }

        #region BattleCmd Order 

        public bool HasOrders { get { return ((Side[2].Fighters_Alive.Count(c => c.myAction != null) > 0) || (Side[5].Fighters_Alive.Count(c => c.myAction != null) > 0)); } }
        public List<BattleAction> NextAction
        {
            get
            {
                List<Fighter> tmp = new List<Fighter>();
                List<BattleAction> cmdtmp = new List<BattleAction>();
                List<Fighter> fighters = new List<Fighter>();
                fighters.AddRange(Side[2].Fighters_Alive); fighters.AddRange(Side[5].Fighters_Alive);
                var fighters_with_cmds = fighters.Where(c => c.myAction != null).ToList();
                var orderedlist = fighters_with_cmds.OrderByDescending(c => c.Eqs.FullSpd);
                var trgt = orderedlist.First();//get fastest person
                fighters_with_cmds.Remove(trgt);
                tmp.Add(trgt);
                // we will go through n add possible combo players

                for (int a = 0; a < fighters_with_cmds.Count; a++)
                {
                    orderedlist = fighters_with_cmds.OrderByDescending(c => c.Eqs.FullSpd);
                    var chk = orderedlist.First();//get fastest person
                    fighters_with_cmds.Remove(chk);
                    //if (trgt.BattlePosition == chk.BattlePosition && inSpdRange(chk, tmp) && trgt.myAction.dst == chk.myAction.dst)
                    //{
                    //    tmp.Add(chk);
                    //}
                    //else
                    //    fighters_with_cmds.Add(chk);
                }
                foreach (Fighter y in tmp)
                {
                    BattleAction tmpe = y.myAction;
                    y.myAction = null;
                    cmdtmp.Add(tmpe);

                }
                return cmdtmp;
            }
        }

        #endregion

        public int FighterCnt
        {
            get
            {
                return 0;// Side[2].Count + Side[5].Count;
            }
        }

        public IEnumerable<List<Fighter>> AllFighters
        {
            get
            {
                return (from list in (from cell in Side.Values select cell.fighterlist) select list);
            }
        }
        bool AllReady { get { return (Side[2].AllReady && Side[5].AllReady); } }

        #region Processing

        public void Process()
        {
            if (blockupdt) return;
            blockupdt = true;
            if (BattleState == eBattleState.Active)//battle has activated
            {
                //check if each side has players that are alive
                if (!(Side[2].Fighters_Alive.Count() > 0 && Side[5].Fighters_Alive.Count() > 0) && RoundState != eBattleRoundState.CalculatingState)
                {
                    BattleState = eBattleState.Ended;
                    EndBattle(eBattleLeaveType.BattleFinished); return;
                }
                //check if every1 sent a command during ready round (
                if (AllReady && !HasOrders && RoundState == eBattleRoundState.ReadyState)//every1 ready no action
                    RoundState = eBattleRoundState.EndedState;
                else if (RoundState == eBattleRoundState.EndedState && HasOrders)//action finished more left
                    RoundState = eBattleRoundState.ReadyState;
                else if (RoundState == eBattleRoundState.EndedState && !HasOrders)//Round over no action
                    StartRound();
                else if (roundend_time < DateTime.Now && RoundState == eBattleRoundState.PrepState || AllReady && RoundState == eBattleRoundState.PrepState)//Planning stage Every1 ready/timefinished
                    RoundState = eBattleRoundState.ReadyState;
                else if (AllReady && HasOrders && RoundState == eBattleRoundState.ReadyState)//every1 ready has orders ready to do action i guess
                    Calculate();
            }
            blockupdt = false;
        }
        
        //Send StartRd info
        public void StartRound()
        {
            RoundState = eBattleRoundState.PrepState;
            Side[2].onRoundStart();
            foreach (var h in Side[2].Fighters_Alive)
                h.RdEndTime = DateTime.Now.AddSeconds(20);
            Side[5].onRoundStart();
            foreach (var h in Side[5].Fighters_Alive)
                h.RdEndTime = DateTime.Now.AddSeconds(20);
        }
        //Rcv Attk
        public void PLayer_BattleAction(BattleAction data)
        {
            data.unknownbyte = 1;
            if (Side[(byte)BattleSide.Attacking].BattleActionRecieved(data) || Side[(byte)BattleSide.Defending].BattleActionRecieved(data))
            {
                SendPacket p = new SendPacket();
                p.PackArray(new byte[]{53, 5});
                p.Pack8(data.src.GridX);
                p.Pack8(data.src.GridY);

                foreach (Player gr in Side[(byte)BattleSide.Attacking].fighterlist.Where(c => c is Player))
                    gr.Send(p);
                foreach (Player gr in Side[(byte)BattleSide.Defending].fighterlist.Where(c => c is Player))
                    gr.Send(p);
            }
        }

        //public void NPC_BattleAction(BattleAction data)
        //{
        //    if (Rightside.BattleActionRecieved(data) || Leftside.BattleActionRecieved(data))
        //    {
        //        SendPacket p = new SendPacket();
        //        p.Header(53, 5);
        //        p.Pack8(data.src.GridX);
        //        p.Pack8(data.src.GridY);
        //        p.SetSize();
        //        foreach (Player gr in Rightside.fighterlist.Where(c => c is Player))
        //        {
        //            g.SendPacket(gr.playerinfo, p);
        //        }
        //        foreach (Player gr in Leftside.fighterlist.Where(c => c is Player))
        //        {
        //            g.SendPacket(gr.playerinfo, p);
        //        }
        //    }
        //}
        //End Battle for all players

        public void StartBattle()
        {
            foreach (Player fighter in Side[2].fighterlist.Where(c => c is Player))
            {
                if (fighter != null && fighter.TypeofFighter == eFighterType.player)
                {
                    fighter.DataOut = SendType.Multi;
                    fighter.Send8_1();
                    Send_11_250(Background,Side[2].fighterlist, fighter);
                    Send_11_5(fighter);
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 11, 10 });
                    p.Pack8(1);
                    fighter.Send(p);
                }
            }
            foreach (Player fighter in Side[5].fighterlist.Where(c => c is Player))
            {
                if (fighter != null && fighter.TypeofFighter == eFighterType.player)
                {
                    (fighter as Player).DataOut = SendType.Multi;
                    (fighter as Player).Send8_1();
                    Send_11_250(Background,Side[5].fighterlist, fighter);
                    Send_11_5(fighter);
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 11, 10 });
                    p.Pack8(1);
                    fighter.Send(p);
                }
            }
            foreach (Player fighter in Side[2].fighterlist.Where(c => c is Player))
                fighter.DataOut = SendType.Normal;
            foreach (Player fighter in Side[5].fighterlist.Where(c => c is Player))
                fighter.DataOut = SendType.Normal;
            BattleState = eBattleState.Active;
        }

        public void EndBattle(eBattleLeaveType t)
        {
            BattleState = eBattleState.Ended;
        }
    
        #endregion



        #region Battle processing


        public void Calculate()
        {
            RoundState = eBattleRoundState.CalculatingState;
            ushort skillid = 0;
            bool flee = false;
            byte dmtype = 0;//miss = 0,hpdmg = 25,spdmg = 26,debuff = 223,sealing = 221,healing = 232,buff = 225,
            uint[] dmg = new uint[2];
            bool miss = false;
            //19 per attacker
            //Second part is the Attk/miss
            //get list for next move which is already reordered by spd
            var e = NextAction;
            SendPacket moveData = new SendPacket(false,true);
            List<Fighter> ppl_involved = new List<Fighter>();
            foreach (var q in e)
            {
                skillid = q.skill.SkillID;

                var pat = q.skill.PatternofAttack();
                if (q.skill.EffectLayer == EffectLayer.Flee) pat.Set(1, 1, 1);

                var re = AttackableTargets(pat.GetTargets(new byte[] { q.dst.GridX, q.dst.GridY }, q.dst.BattlePosition), q.dst.BattlePosition);

                List<uint[]> targets = new List<uint[]>();
                if (re.Count > 1)
                    moveData.Pack16(28);
                else if (re.Count == 1)
                {
                    moveData.Pack16((byte)Atktype(q.skill.EffectLayer));
                    moveData.Pack8(q.src.GridX); moveData.Pack8(q.src.GridY);
                    moveData.Pack16(skillid);
                    moveData.Pack8(0); //affect by poison? switch to 1
                    moveData.Pack8((byte)re.Count);
                    foreach (var y in re)
                    {
                        if (SucessRate(q))
                        {
                            dmg = GetDamage(q, y);
                            if (q.skill.EffectLayer != EffectLayer.Flee)
                            {}//q.src.myRewards.AddFrom(y.Attackedby(q.src, dmg[0], q.skill));
                            else
                                flee = true;
                        }
                        else
                        {
                            switch (q.skill.EffectLayer)
                            {
                                case EffectLayer.Physical: { miss = true; } break;
                                case EffectLayer.Flee: { miss = true; skillid = 60046; } break;
                            }
                        }
                        var et = GetState(miss, q.skill, y);

                        moveData.Pack8(y.GridX);
                        moveData.Pack8(y.GridY);
                        moveData.Pack8(et[0]);
                        moveData.Pack8(et[1]);
                        moveData.Pack8(et[2]);
                        moveData.Pack8(et[3]);//miss
                        moveData.Pack32(dmg[0]);
                        moveData.Pack16((ushort)dmg[1]);
                    }

                    ppl_involved.Add(q.src);

                }
            }
            //if (e[0].dst.CurHP == 0 && e[0].dst.itemreward != null)
            //{
            //    foreach (var s in ppl_involved)
            //    {
            //        if (s is PetFighter)
            //        {

            //        }
            //        else if (s is Player)
            //        {
            //            //(s as Player).playerinfo.character.MyInventory.RecieveItem(
            //        }
            //    }
            //}
            if (flee && !miss)
                foreach (var s in ppl_involved)
                    RemFighter(eBattleLeaveType.RunAway, s);
            if (ppl_involved.Count > 0)
            {
                Side[(byte)BattleSide.Defending].Send_Attack(ppl_involved, moveData.Data.ToArray());
                Side[(byte)BattleSide.Attacking].Send_Attack(ppl_involved, moveData.Data.ToArray());
                Side[(byte)BattleSide.Watching].Send_Attack(ppl_involved, moveData.Data.ToArray());
            }
            RoundState = eBattleRoundState.EndedState;
            //delay = DateTime.Now.AddSeconds(2.4);
        }

        #endregion

        #region BattleSide Control
        public void onJoinBattle(Fighter f)
        {
            Side[(byte)f.BattlePosition].Add(TypeofBattle, f);
            if (f.TypeofFighter == eFighterType.player)
            {
                (f as Player).DataOut = SendType.Multi;
                //Send_11_250_BattlePost((f as Player));

                SendPacket p = new SendPacket();
                p.PackArray(new byte[] { 11, 10 });
                p.Pack8(1);
                (f as Player).Send(p);
            }
            //Send_11_4n5_InBattle(f, AllFighters);
            if (f.TypeofFighter == eFighterType.player)
                (f as Player).DataOut = SendType.Normal;
        }
        //Watching Battle
        public void onWatchBattle(Fighter f)
        {
            f.BattlePosition = BattleSide.Watching;
            Side[4].Add(TypeofBattle, f);
            (f as Player).DataOut = SendType.Multi;
            //Send_11_250_BattlePost((f as Player));
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 11, 10 });
            p.Pack8(1);
            (f as Player).Send(p);
            (f as Player).DataOut = SendType.Normal;
        }

        public void RemFighter(eBattleLeaveType o, Fighter src)
        {
            if (Side[(byte)BattleSide.Defending].Fighters_Alive.Count(c => c != src) == 0 || Side[(byte)BattleSide.Attacking].Fighters_Alive.Count(c => c != src) == 0)
            {
                Side[(byte)src.BattlePosition].onFighterLeft(this, src.BattlePosition, o, src);
                if (o == eBattleLeaveType.Dced)
                    EndBattle(eBattleLeaveType.RunAway);
                else if (o == eBattleLeaveType.Spawn)
                    EndBattle(eBattleLeaveType.BattleFinished);
                else
                    EndBattle(o);
            }
            else
                Side[(byte)src.BattlePosition].onFighterLeft(this, src.BattlePosition, o, src);
        }
        public void RemFighter(eBattleLeaveType o, uint ID)
        {
            RemFighter(o, FindFighter(ID));
        }

        public Fighter FindFighter(uint ID)
        {
            foreach (var t in Side.Values.ToList())
                foreach (Player r in t.fighterlist)
                    if (r.ID == ID)
                        return r;
            return null;
        }
        public Fighter FindFighter(byte x, byte y)
        {
            foreach (var t in Side.Values.ToList())
                foreach (Player r in t.fighterlist)
                    if ((r as Fighter).GridX == x && (r as Fighter).GridY == y)
                        return r;
            return null;
        }
        #endregion

        #region Calculations
        double GetAtkDamage(ushort atk_matk, ushort SkillPower, ushort Def_mdef, float elementCorr)
        {
            var est = 0.0;
            if (atk_matk >= Def_mdef)
            {
                est = Math.Round(((atk_matk * (.5 + new Random().NextDouble()) + SkillPower) - (Def_mdef * 0.98)) * elementCorr);
            }
            else if (atk_matk <= Def_mdef)
            {
                est = ((atk_matk * (.5 + new Random().NextDouble()) + SkillPower) - ((int)Def_mdef * 0.98)) * elementCorr;
                if (est < 0) est = 1;
            }
            if (est < 0)
                return Def_mdef / 3;
            else return est;
        }
        double GetMatkDamage(int atk_matk, int SkillPower, int Def_mdef, float elementCorr)
        {
            var est = 0.0;
            if (atk_matk >= Def_mdef)
            {
                est = ((atk_matk * (1 + new Random().NextDouble()) + SkillPower) - ((int)Def_mdef * 0.98)) * elementCorr;
            }
            else if (atk_matk <= Def_mdef)
            {
                est = ((atk_matk * (1.1 + new Random().NextDouble()) + SkillPower) - ((int)Def_mdef * 1.3)) * elementCorr;
            }
            if (est < 0)
                return Def_mdef / 3;
            else return est;
        }
        double GetElementCorrection(ElementType hitter, ElementType target)
        {
            switch (hitter)
            {
                case ElementType.Fire:
                    {
                        switch (target)
                        {
                            case ElementType.Normal: return 1.0;
                            case ElementType.Fire: return 1.0;
                            case ElementType.Earth: return 1.0;
                            case ElementType.Water: return 0.6;
                            case ElementType.Wind: return 1.5;
                        }
                    } break;
                case ElementType.Earth:
                    {
                        switch (target)
                        {
                            case ElementType.Normal: return 1;
                            case ElementType.Fire: return 1.0;
                            case ElementType.Earth: return 1.0;
                            case ElementType.Water: return 1.7;
                            case ElementType.Wind: return 0.6;
                        }
                    } break;
                case ElementType.Water:
                    {
                        switch (target)
                        {
                            case ElementType.Normal: return 1;
                            case ElementType.Fire: return 1.7;
                            case ElementType.Earth: return 0.6;
                            case ElementType.Water: return 1.0;
                            case ElementType.Wind: return 1.0;
                        }
                    } break;
                case ElementType.Wind:
                    {
                        switch (target)
                        {
                            case ElementType.Normal: return 1;
                            case ElementType.Fire: return 0.4;
                            case ElementType.Earth: return 1.7;
                            case ElementType.Water: return 1.0;
                            case ElementType.Wind: return 1.0;
                        }
                    } break;
                case ElementType.Normal:
                    {
                        switch (target)
                        {
                            case ElementType.Normal: return 1;
                            case ElementType.Fire: return 1.3;
                            case ElementType.Earth: return 1.3;
                            case ElementType.Water: return 1.3;
                            case ElementType.Wind: return 1.3;
                        }
                    } break;
            }
            return 0;
        }
        bool CanFlee(byte srclvel, byte dstlevl)
        {
            if (srclvel < dstlevl)
            {
                int y = dstlevl / 6;
                return Succuss_miss(75 - y, 100, 0);
            }
            else
                return true;
        }
        bool Apply_Crit(byte crit_perc)
        {
            return true;
        }
        bool Succuss_miss(double pert, int outof, double bonus)
        {
            if ((new Random().Next(0, outof) + new Random().NextDouble() + bonus) >=
                (outof - (double)((pert * 100) / outof)))
            {
                return true;
            }
            return false;
        }
        #endregion

        //#region Quick Send/GEt
        void Send_11_5(Player target)
        {
            List<Fighter> flist = new List<Fighter>();
            if (target != startedby)
            {
                flist.AddRange(Side[(byte)BattleSide.Attacking] .fighterlist.Where(h => h.TypeofFighter != eFighterType.player));
                flist.AddRange(Side[(byte)BattleSide.Defending].fighterlist.Where(h => h.TypeofFighter != eFighterType.player));
            }
            else
            {
                flist.AddRange(Side[(byte)BattleSide.Attacking].fighterlist.Where(h => h != startedby && h.BattlePosition != startedby.BattlePosition));
                flist.AddRange(Side[(byte)BattleSide.Defending].fighterlist.Where(h => h != startedby && h.BattlePosition != startedby.BattlePosition));
            }


            if (flist.Count > 0)
                foreach (Fighter f in flist)
                {
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 11, 5 });
                    p.Pack8((byte)f.BattlePosition);//p.Pack8(3); 
                    p.Pack8((byte)f.TypeofFighter);
                    p.Pack32(f.ID);
                    p.Pack16(f.ClickID); p.Pack32(f.OwnerID);
                    p.Pack8(f.GridX); p.Pack8(f.GridY);
                    p.Pack32((uint)f.MaxHP); p.Pack16((ushort)f.MaxSP);
                    p.Pack32((uint)f.CurHP); p.Pack16((ushort)f.CurSP);
                    p.Pack8((byte)f.Level);
                    p.Pack8((byte)f.Element);
                    p.Pack8(Convert.ToByte(f.Reborn)); p.Pack8((byte)f.Job);
                    target.Send(p);
                }

        }
        //void Send_11_4n5_InBattle(Fighter data, List<Fighter> targets)
        //{
        //    foreach (Player d in targets)
        //    {
        //        if (d != data)
        //        {
        //            Send11_5(data, d.playerinfo);
        //            Send11_4(2, data.Fighter_ID, 0, (byte)data.Position, d.playerinfo);
        //        }
        //    }
        //}
        //void Send_11_250_BattlePrep(Fighter fighter, Player target)
        //{
        //    List<Fighter> flist = new List<Fighter>();
        //    if (!fighter.StartedBattle)
        //    {
        //        flist.Add(fighter);
        //        flist.AddRange(Rightside.fighterlist.Where(c => c.TypeofFighter == eFighterType.player && c != fighter));
        //        flist.AddRange(Leftside.fighterlist.Where(c => c.TypeofFighter == eFighterType.player && c != fighter));
        //        Send_11_250(Background, flist, target);
        //    }
        //    else
        //    {
        //        switch (fighter.Position)
        //        {
        //            case BattleSide.right:
        //                {
        //                    flist.AddRange(Rightside.fighterlist.Where(c => c.TypeofFighter == eFighterType.player && c.Position == fighter.Position));
        //                } break;
        //            case BattleSide.left:
        //                {
        //                    flist.AddRange(Leftside.fighterlist.Where(c => c.TypeofFighter == eFighterType.player && c.Position == fighter.Position));
        //                } break;
        //        }
        //        Send_11_250(Background, flist, target);
        //    }
        //}
        public void Send_11_250(UInt16 background, List<Fighter> flist, Player target)
        {
            //this is sent to the player entering combat, and lists all other players already in
            if (flist.Count > 0)
            {
                SendPacket p = new SendPacket();
                p.PackArray(new byte[]{11, 250});
                p.Pack16(background);
                foreach (Fighter f in flist)
                {
                    p.Pack8((byte)f.BattlePosition);
                    p.Pack8((byte)f.TypeofFighter);
                    p.Pack32(f.ID);
                    p.Pack16(f.ClickID); p.Pack32(f.OwnerID);
                    p.Pack8(f.GridX); p.Pack8(f.GridY);
                    p.Pack32((uint)f.MaxHP); p.Pack16((ushort)f.MaxSP);
                    p.Pack32((uint)f.CurHP); p.Pack16((ushort)f.CurSP);
                    p.Pack8((byte)f.Level);
                    p.Pack8((byte)f.Element); p.PackBoolean(f.Reborn); p.Pack8((byte)f.Job);
                }
                target.Send(p);
            }
        }

        //void Send_11_250_BattlePost(Player watcher)
        //{
        //    List<Fighter> tmp = new List<Fighter>();
        //    tmp.Add(watcher);
        //    tmp.AddRange(Rightside.fighterlist.Where(c => c != watcher));
        //    tmp.AddRange(Leftside.fighterlist.Where(c => c != watcher));
        //    SendPacket ot = new SendPacket();
        //    ot.Header(11, 250);
        //    ot.Pack16(this.Background);
        //    for (int a = 0; a < tmp.Count; a++)
        //    {
        //        if (tmp[a].TypeofFighter == eFighterType.player || tmp[a].TypeofFighter == eFighterType.Watcher)
        //        {
        //            if (tmp[a].BattlePosition == BattleSide.Watching)
        //                ot.Pack8(4);
        //            else
        //                if (tmp[a].StartedBattle)
        //                    ot.Pack8(2);
        //                else if (tmp[a] == watcher)
        //                    switch (tmp[a].BattlePosition)
        //                    {
        //                        case BattleSide.left: ot.Pack8(2); break;
        //                        case BattleSide.right: ot.Pack8(5); break;
        //                    }
        //                else
        //                    ot.Pack8(100);
        //            ot.Pack8((byte)tmp[a].TypeofFighter);
        //            ot.Pack32(tmp[a].ID);
        //            ot.Pack16(tmp[a].clickID); ot.Pack32(tmp[a].ownerID);
        //            if (tmp[a].Position == BattleSide.watch)
        //            {
        //                ot.Pack8(255); ot.Pack8(255);
        //            }
        //            else
        //            { ot.Pack8(tmp[a].GridX); ot.Pack8(tmp[a].GridY); }
        //            ot.Pack32(tmp[a].Stats.MaxHP); ot.Pack16((ushort)tmp[a].Stats.MaxSP);
        //            ot.Pack32(tmp[a].CurHP); ot.Pack16((ushort)tmp[a].CurSP);
        //            ot.Pack8((byte)tmp[a].Stats.Level);
        //            ot.Pack8((byte)tmp[a].Stats.element);
        //            ot.Pack8(BitConverter.GetBytes(tmp[a].Stats.Reborn)[0]); ot.Pack8((byte)tmp[a].Stats.job);
        //        }
        //    }
        //    ot.SetSize();
        //    g.SendPacket(watcher.playerinfo, ot);
        //}
        //void Send11_4(byte bval1, UInt32 id, UInt16 wval, byte bval2, Player target)
        //{
        //    SendPacket p = new SendPacket(target);
        //    p.Header(11, 4);
        //    p.Pack8(bval1);
        //    p.Pack32(id);
        //    p.Pack16(wval);
        //    p.Pack8(bval2);
        //    p.SetSize();
        //    p.Send();
        //}

        //void Send11_5(Fighter f, Player target)
        //{
        //    SendPacket p = new SendPacket(target);
        //    p.Header(11, 5);
        //    p.Pack8((byte)f.Position);//p.Pack8(3); 
        //    p.Pack8((byte)f.TypeofFighter);
        //    p.Pack32(f.Fighter_ID);
        //    p.Pack16(f.clickID); p.Pack32(f.ownerID);
        //    p.Pack8(f.GridX); p.Pack8(f.GridY);
        //    p.Pack32(f.Stats.MaxHP); p.Pack16((ushort)f.Stats.MaxSP);
        //    p.Pack32(f.CurHP); p.Pack16((ushort)f.CurSP);
        //    p.Pack8((byte)f.Stats.Level);
        //    p.Pack8((byte)f.Stats.element);
        //    p.Pack8(BitConverter.GetBytes(f.Stats.Reborn)[0]); p.Pack8((byte)f.Stats.job);
        //    p.SetSize();
        //    p.Send();
        //}
        List<Fighter> AttackableTargets(List<byte[]> kl, BattleSide loc)
        {
            List<Fighter> tmp = new List<Fighter>();
            foreach (var k in kl)
            {
                var y = FindFighter(k[0], k[1]);
                if (y != null && y.BattlePosition == loc && y.State != PlayerState.InGame_Battling_Dead)
                    tmp.Add(y);
            }
            if (tmp.Count == 0)
                switch (loc)
                {
                    case BattleSide.Defending:
                        {
                            foreach (Fighter u in Side[(byte)BattleSide.Attacking].Fighters_Alive)
                            {
                                tmp.Add(u); break;
                            }
                        } break;
                    case BattleSide.Attacking:
                        {
                            foreach (Fighter u in Side[(byte)BattleSide.Defending].Fighters_Alive)
                            {
                                tmp.Add(u); break;
                            }
                        } break;
                }
            return tmp;
        }

        byte Atktype(EffectLayer q)
        {
            switch (q)
            {
                case EffectLayer.Flee:
                case EffectLayer.Buffs1:
                case EffectLayer.Defend:
                case EffectLayer.Healing1:
                case EffectLayer.Seals1:
                case EffectLayer.Magical:
                case EffectLayer.Physical: return 17;
                case EffectLayer.Mana: return 28;
                case EffectLayer.None: return 23;
            }
            return 0;
        }

        byte DmgType(EffectLayer q)
        {
            switch (q)
            {
                case EffectLayer.Flee:
                //case EffectLayer.Buffs1:
                case EffectLayer.Defend:
                //case EffectLayer.Healing1:
                //case EffectLayer.Seals1:
                case EffectLayer.Magical:
                case EffectLayer.Physical: return 25;
                //case EffectLayer.Mana: attktype = 28; break;
                //case EffectLayer.noEffects: attktype = 23; break;
            }
            return 0;
        }

        uint[] GetDamage(BattleAction d, Fighter trgt)
        {
            double tmp = 0;
            bool add = false;
            switch (d.skill.EffectLayer)
            {
                case EffectLayer.Physical: { add = true; tmp = GetAtkDamage((ushort)d.src.Eqs.FullAtk, d.skill.AttackPower(),(ushort)trgt.Eqs.FullDef, (float)GetElementCorrection(d.src.Element, trgt.Element)); } break;
                case EffectLayer.Magical: { add = true; tmp = GetMatkDamage(d.src.Eqs.FullMatk, d.skill.AttackPower(), trgt.Eqs.FullMdef, (float)GetElementCorrection(d.src.Element, trgt.Element)); } break;
            }

            return new uint[] { (uint)Math.Round(tmp), BitConverter.GetBytes(add)[0] };
        }

        byte[] GetState(bool miss, Skill j, Fighter dst)
        {
            byte[] res = new byte[4];
            if (miss)
            {
                switch (j.EffectLayer)
                {
                    case EffectLayer.Physical:
                    case EffectLayer.Seals1:
                        {
                            res[0] = 0;
                            res[1] = 1;
                            res[2] = 1;
                            res[3] = 0;
                        } break;
                    case EffectLayer.Flee:
                        {
                            res[0] = 0;
                            res[1] = 2;
                            res[2] = 1;
                            res[3] = 0;
                        } break;
                }
            }
            else
            {
                if (dst.SkillEffect != null && dst.SkillEffect.EffectLayer == EffectLayer.Defend)
                {
                    if (j.SkillID == 11056)
                    {
                        res[0] = 1;
                        res[1] = 0;
                        res[2] = 1;
                        res[3] = (byte)DmgType(j.EffectLayer);
                    }
                    else
                    {
                        res[0] = 1;
                        res[1] = 1;
                        res[2] = 1;
                        res[3] = (byte)DmgType(j.EffectLayer);
                    }
                }
                else
                    switch (j.EffectLayer)
                    {
                        case EffectLayer.Flee:
                            {
                                res[0] = 0;
                                res[1] = 1;
                                res[2] = 1;
                                res[3] = (byte)DmgType(j.EffectLayer);
                            } break;
                        case EffectLayer.Physical:
                        case EffectLayer.Magical:
                        case EffectLayer.Healing1:
                            {
                                res[0] = 1;
                                res[1] = 0;
                                res[2] = 1;
                                res[3] = (byte)DmgType(j.EffectLayer);
                            } break;
                        case EffectLayer.Seals2:
                            {
                                switch (j.GetData().UnknownByte7)
                                {
                                    case 0:
                                        {
                                            res[0] = 1;
                                            res[1] = 0;
                                            res[2] = 1;
                                            res[3] = (byte)DmgType(j.EffectLayer);
                                        } break;
                                    case 1:
                                        {
                                            res[0] = 1;
                                            res[1] = 1;
                                            res[2] = 1;
                                            res[3] = (byte)DmgType(j.EffectLayer);
                                        } break;
                                }
                            } break;
                    }
            }
            return res;
        }
        bool SucessRate(BattleAction t)
        {
            bool success = false;
            switch (t.skill.EffectLayer)
            {
                case EffectLayer.Flee:
                    {
                        //switch (t.src.BattlePosition)
                        //{
                        //    case BattleSide.Defending: success = CanFlee((byte)t.src.Level, (byte)Rightside.fighterlist[0].Stats.Level); break;
                        //    case BattleSide.Attacking: success = CanFlee((byte)t.src.Level, (byte)Leftside.fighterlist[0].Stats.Level); break;
                                
                        //}
                        success = true;
                    } break;
                default: success = Succuss_miss(((79.0 / 100.0) * 100), 100, 0); break;
            }
            return success;
        }

        bool inSpdRange(Fighter chk, List<Fighter> agst)
        {
            bool ret = false;
            foreach (Fighter w in agst.Where(c => c != chk))
            {
                if ((w.Eqs.FullSpd.CompareTo((UInt16)(chk.Eqs.FullSpd - 100)) >= 0) &&
                        (chk.Eqs.FullSpd.CompareTo((UInt16)(w.Eqs.FullSpd - 100)) >= 0))
                    ret = true;
            }
            return ret;
        }
    }

    public class BattleArea
    {
        Battle battleRef;
        int pos; int turn;
        public BattleSide Position { get { return (BattleSide)pos; } }
        public List<Fighter> fighterlist = new List<Fighter>(8);


        public BattleArea(byte side, Battle area)
        {
            pos = side;
            battleRef = area;
        }
        public bool AllReady { get { if (fighterlist.Count(c => c.ActionDone) == fighterlist.Count)return true; else return false; } }
        public byte[] Placement
        {
            get
            {
                byte x = 0;
                byte y = 2;
                switch (Position)
                {
                    case BattleSide.Defending: x = 1; break;
                    case BattleSide.Attacking: x = 4; break;
                }
                switch (fighterlist.Count)
                {
                    case 0: break;
                    case 1: y += 1; break;
                    case 2: y -= 1; break;
                    case 3: y += 2; break;
                }
                return new byte[] { x, y };
            }
        }
        public int Count
        {
            get
            {
                return fighterlist.Count;
            }
        }
        public List<Fighter> Fighters_Alive
        {
            get
            {
                return fighterlist.Where(c => c.State == PlayerState.InGame_Battling_Alive).ToList();
            }
        }

        public void onRoundStart()
        {
            foreach (Fighter f in fighterlist.ToList())
            {
                if(f.SkillEffect != null)
                f.SkillEffect.DecreaseTurns();
                if (f.TypeofFighter == eFighterType.Npc)
                {
                    /* if (f.AI != null)
                         f.AI.ProcessMove();*/
                }
                else if (f.TypeofFighter == eFighterType.player)
                {
                    (f as Player).DataOut = SendType.Multi;

                    switch (Position)
                    {
                        case BattleSide.Attacking:
                            {
                                foreach (Fighter ft in fighterlist.ToList())
                                {
                                    Send51_1(ft, 25, (ushort)ft.CurHP, (f as Player));
                                    Send51_1(ft, 26, (ushort)ft.CurSP, (f as Player));
                                }
                                foreach (Fighter lt in battleRef[BattleSide.Defending].fighterlist)
                                    Send51_1(lt, 25, (ushort)lt.CurHP, (f as Player));
                            } break;
                        case BattleSide.Defending:
                            {
                                foreach (Fighter ft in fighterlist.ToList())
                                {
                                    Send51_1(ft, 25, (ushort)ft.CurHP, (f as Player));
                                    Send51_1(ft, 26, (ushort)ft.CurSP, (f as Player));
                                }
                                foreach (Fighter lt in battleRef[BattleSide.Attacking].fighterlist)
                                    Send51_1(lt, 25, (ushort)lt.CurHP, (f as Player));
                            } break;
                    }
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 52, 1 });
                    (f as Player).Send(p);
                    (f as Player).DataOut = SendType.Normal;
                }
            }
            foreach (Fighter f in fighterlist.ToList())
                if (f.TypeofFighter == eFighterType.player)
                    (f as Player).DataOut = SendType.Normal;
        }
        public void FightStarted(object sender, EventArgs e)
        {

        }
        public void onBattleOver(Battle sender, eBattleLeaveType t)
        {
            //if (Position == BattleSide.Watching)
            //    foreach (Fighter fighter in fighterlist.ToList())
            //        FighterLeft(sender, fighter.BattlePosition, eBattleLeaveType.RunAway, fighter);
            //else
            //    foreach (Fighter fighter in fighterlist.ToList())
            //        FighterLeft(sender, fighter.BattlePosition, t, fighter);
        }
        public void Send51_1(Fighter t, byte skill, ushort amt, Player target)
        {
            SendPacket p = new SendPacket();
            p.PackArray(new byte[] { 51, 1 });
            p.Pack8((byte)t.GridX); p.Pack8((byte)t.GridY);
            p.Pack8(skill);
            p.Pack16(amt);
            p.Pack16(0);
            target.Send(p);
        }
        public void Add(eBattleType h, Fighter fighter)
        {
            if (fighter.TypeofFighter == eFighterType.player)
            {
                fighter.GridX = Placement[0];
                fighter.GridY = Placement[1];

                if (fighter.Pets.BattlePet != null)
                {
                    //Fighter pet = new Fighter(g);
                    //pet.SetFrom(fighter.playerinfo.pets.GetPetinBattleMode());//finish later
                    //pet.actionDone = false;
                    //if (fighter.starter)
                    //    pet.starter = true;
                    //pet.myBattleType = Fighter.eFighterType.Pet;
                    //pet.GridX = (byte)(fighter.GridX - 1);
                    //pet.GridY = fighter.GridY;
                    //pet.myplayerInfo = fighter.playerinfo;
                    //pet.ownerID = fighter.ID;
                    //pet.clickID = fighter.clickID;//not the right one
                    //fighterlist.Add(pet);
                }
            }
            else if (fighter.TypeofFighter == eFighterType.Npc)
            {
                fighter.GridX = Placement[0];
                fighter.GridY = Placement[1];
            }
            fighterlist.Add(fighter);
        }
        public void onFighterLeft(Battle src, BattleSide side, eBattleLeaveType exit, Fighter fighter)
        {
            if (this.Position == side)
            {
                bool defeat = false;
                if (fighter.TypeofFighter == eFighterType.Pet || fighter.TypeofFighter == eFighterType.Npc)
                {

                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 11, 1 });
                    p.Pack8((byte)fighter.GridX); p.Pack8((byte)fighter.GridY);

                    //foreach (Fighter u in src.AllFighters)
                    //    if (u is Player)
                    //        (u as Player).Send(p);

                    if (fighter.TypeofFighter == eFighterType.Pet)
                    {
                        if (fighter.CurHP == 0)
                        {
                            defeat = true;
                            (fighter as PetFighter).CurHP++;
                        }
                        else
                            (fighter as PetFighter).CurHP = (fighter as PetFighter).CurHP;
                        (fighter as PetFighter).CurSP = (fighter as PetFighter).CurSP;
                    }

                }
                else if (fighter.TypeofFighter == eFighterType.player)
                {
                    //(fighter as Player).playerinfo.character.MySkills.CheckforUpdate();
                    //SendPacket t = new SendPacket();
                    //t.Header(11, 12);
                    //if ((fighter as Player).StartedBattle)
                    //    t.Pack8(1);
                    //else
                    //    t.Pack8(2);
                    //g.SendPacket((fighter as Player).playerinfo, t);
                    //if ((fighter as Player).CurHP != 0)
                    //    (fighter as Player).CurHP = fighter.CurHP;
                    //else
                    //{
                    //    defeat = true;
                    //    (fighter as Player).CurHP += 1;
                    //}
                    //(fighter as Player).CurSP = (fighter as Player).CurSP;
                    //t.Pack32(0);
                    //(fighter as Player).Send(t);

                    //switch (exit)//Rewards proccessed on leave
                    //{
                    //    case eBattleLeaveType.BattleFinished:
                    //        {
                    //            //(fighter as Player).playerinfo.character.MyStats.Send_GoldGain((uint)(fighter as Player).myRewards.gold);
                    //            //if (!defeat)
                    //            //    (fighter as Player).playerinfo.character.MyStats.CurExp += (int)(fighter as Player).myRewards.exp;
                    //            //else
                    //            //    if ((fighter as Player).playerinfo.character.MyStats.CurExp - (int)Math.Round(((fighter as Player).playerinfo.character.MyStats.CurExp * .06)) > 0)
                    //            //        (fighter as Player).playerinfo.character.MyStats.CurExp -= (int)Math.Round(((fighter as Player).playerinfo.character.MyStats.CurExp * .06));
                    //            /*if (fighter.myRewards.IM > 0)
                    //                (fighter as Player).playerinfo.IM_Points += fighter.myRewards.IM;*/
                    //        } break;
                    //}
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[] { 11, 0 });
                    p.Pack32(fighter.ID);
                    p.Pack16(0);
                    (fighter as Player).CurrentMap.Broadcast(p);
                    p = new SendPacket();
                    p.PackArray(new byte[] { 11, 1 });
                    p.Pack8((byte)fighter.GridX); p.Pack8((byte)fighter.GridY);
                    p.Pack8(0);
                    (fighter as Player).Send(p);
                }
                fighterlist.Remove(fighter);
            }
        }
        public void Leave_Fight(ref Fighter src)
        {

        }
        public bool BattleActionRecieved(BattleAction h)
        {
            foreach (Fighter e in fighterlist.ToList())
                if (h.src == e)
                {
                    e.myAction = h;
                    return true;
                }
            return false;
        }
        public List<Fighter> Fightersbyspd
        {
            get
            {
                return fighterlist.OrderBy(r => r.Eqs.FullSpd).ToList();
            }
        }
        public void Send_Attack(Fighter src, ushort skill, Fighter dst, bool miss, uint Damg)
        {
            foreach (Fighter rt in fighterlist.ToList())
            {
                if (rt.TypeofFighter == eFighterType.player)
                {
                    (rt as Player).DataOut = SendType.Multi;
                    SendPacket p = new SendPacket();
                    p.PackArray(new byte[]{50, 6});
                    p.Pack8((byte)src.GridX); p.Pack8((byte)src.GridY);
                    p.Pack8(0);
                   (rt as Player).Send(p);
                    p = new SendPacket();
                    p.PackArray(new byte[]{50, 1});
                    p.Pack16(17);
                    p.Pack8((byte)src.GridX); p.Pack8((byte)src.GridY);
                    p.Pack16(skill);
                    p.Pack8(0); p.Pack8(1);
                    p.Pack8((byte)dst.GridX); p.Pack8((byte)dst.GridY);
                    p.Pack8(1);
                    p.Pack8(0);
                    p.Pack8(1);
                    if (!miss)
                        p.Pack8(25);
                    else
                        p.Pack8(0);
                    //Second part is damage calculation
                    p.Pack32(Damg);
                    p.Pack8(1);
                   (rt as Player).Send(p);
                    (rt as Player).DataOut = SendType.Normal;
                }
            }
        }
        public void Send_Attack(List<Fighter> r, byte[] data)
        {
            SendPacket w = new SendPacket();
            w.PackArray(new byte[] { 50, 1 });
            w.PackArray(data);

            foreach (Fighter rt in fighterlist.ToList())
            {
                if (rt.TypeofFighter == eFighterType.player)
                {
                    (rt as Player).DataOut = SendType.Multi;
                    foreach (var s in r)
                    {
                        SendPacket p = new SendPacket();
                        p.PackArray(new byte[] { 50, 6 });
                        p.Pack8((byte)s.GridX); p.Pack8((byte)s.GridY);
                        p.Pack8(0);
                        (rt as Player).Send(p);
                    }
                    (rt as Player).Send(w);
                    (rt as Player).DataOut = SendType.Normal;
                }
            }
        }

    }
}
