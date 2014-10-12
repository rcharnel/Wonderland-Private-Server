using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Utilities;
using Wonderland_Private_Server.DataManagement.DataFiles;
using Wonderland_Private_Server.Code.Interface;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC11:AC
    {
        public override int ID { get { return 11; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                case 1: Recv_1(ref r, p); break;
                case 2: Recv_2(ref r, p); break; //a pk was initiated

                default: LogServices.Log(p.A + "," + p.B + " Has not been coded"); break;
            }
        }
        public void Recv_1(ref Player r, RecvPacket p)
        {
            switch (p.Unpack8(2))
            {
                case 3: r.BattleScene.RemFighter(Code.Enums.eBattleLeaveType.RunAway, r); break;
            }
        }
        public void Recv_2(ref Player r, RecvPacket p) //pk
        {
            //check if gm
            //if (!r.GM)
            //{
            //    SendPacket t = new SendPacket();
            //    t = new SendPacket();
            //    t.PackArray(new byte[] { 2, 3 });
            //    t.Pack32(100);
            //    t.PackNString("Only GMs can start PK Battles %#");
            //    r.Send(t);
            //    return;
            //}

            byte pkType = p.Unpack8(2); //pk type
            UInt32 targetID = p.Unpack32(3); //id of player that was attacked
            UInt16 clickID = p.Unpack16(7); //npc, or pc's index (not sure if used on pc pks)

            switch (pkType)
            {
                case 2: //pk against other pc
                    {
                        if (r.CurrentMap.Players[targetID] != null)
                        {
                            //also check to see if both chars have their pk turned on TODO
                            if (r.Settings.PKABLE && r.CurrentMap.Players[targetID].Settings.PKABLE)
                                r.CurrentMap.onPk_Started(r, r.CurrentMap.Players[targetID]);
                        }
                    } break;
                case 3: //pk againts npc
                    {
                        Npc target = cGlobal.gNpcManager.GetNpc((ushort)targetID);
                        target.ClickID = clickID;
                        if (r.CurrentMap != null)
                            r.CurrentMap.onNpcPk(r, target);

                    } break;
                case 4: //join
                    {
                        //cCharacter t = g.gDataManager.cCharacterManager.getByID(targetID);
                        //if (t != null && t.battle != null)
                        //{
                        //    cFighter w = new cFighter(g);
                        //    w.SetFrom(c);
                        //    t.battle.FighterWatch(w);
                        //    cSendPacket watchstate = new cSendPacket(g);
                        //    watchstate.Header(11, 4);
                        //    watchstate.AddByte(2);
                        //    watchstate.AddDWord(c.characterID);
                        //    watchstate.AddWord(0);
                        //    watchstate.AddByte(4);
                        //    watchstate.SetSize();
                        //    g.gDataManager.MapManager.GetMapByID(c.mapLoc).SendtocCharactersEx(watchstate, c);
                        //}

                    } break;
                case 5: //watch
                    {
                        //cCharacter t = g.gDataManager.cCharacterManager.getByID(targetID);
                        //if (t != null && t.battle != null)
                        //{
                        //    cFighter w = new cFighter(g);
                        //    w.SetFrom(c);
                        //    w.Position = t.battle.FindFighterbyID((int)targetID).Position;
                        //    t.battle.FighterJoin(w);


                        //}

                    } break;
            }
            //get the cCharacter file for target
        }

    }
}
