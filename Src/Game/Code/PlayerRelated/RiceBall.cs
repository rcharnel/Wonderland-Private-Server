using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wonderland_Private_Server.Network;
using Wlo.Core;
using Game;

namespace Game.Code.PlayerRelated
{
    public class RiceBall
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        Player own;

        public bool isAi = false;
        UInt16 id;
        int endtime;

        public ushort Npc { get { return id; } }
        public bool isActive { get { return (isAi) ? true : timer.IsRunning; } }
        public RiceBall(Player ty)
        {
            own = ty;
            id = 0;
            endtime = 0;
        }

        public void Activate()
        {
            if (id > 0)
            {
                if (!CheckForStop() || isAi)
                {
                    timer.Restart();
                    own.CurMap.Broadcast(SendPacket.FromFormat("bbdw", 5, 5, own.CharID, id));
                    own.SendPacket(SendPacket.FromFormat("bbbb", 23, 207, 1, 1));
                }
            }
        }
        public void Deactivate()
        {
            if (id > 0)
            {
                if (!CheckForStop() || isAi)
                {
                    timer.Stop();
                    endtime -= timer.Elapsed.Seconds;
                    own.CurMap.Broadcast(SendPacket.FromFormat("bbdw", 5, 5, own.CharID, 0));
                    own.SendPacket(SendPacket.FromFormat("bbbb", 23, 207, 1, 2));
                }
            }
        }
        public bool Set(byte invCell)
        {
            try
            {
                //id = own.Inv[invCell].NpcID; //will be set from i.item.npcID
                //endtime = 60;
                //Activate();
                return true;
            }
            catch { return false; }
        }
        public void GMRiceBall(UInt16 npcid)
        {
            id = npcid;
            endtime = 6000;
            Activate();
        }
        public void SendStatus()
        {
            //SendPacket p = new SendPacket();
            //p.PackArray(new byte[] { 23, 207 });
            //p.Pack8(2);
            //p.Pack16((ushort)(endtime - timer.Elapsed.Seconds));
            //own.Send(p);
        }
        public void Update()
        {
            if (isActive && CheckForStop())
            {
                Deactivate();
                id = 0;
            }
        }
        public bool CheckForStop() { return (timer.Elapsed.Seconds >= endtime); }



    }
}
