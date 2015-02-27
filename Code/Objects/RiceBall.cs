using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Code.Objects
{
    public class cRiceBall
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        Player own;

        public bool isAi = false;
        UInt16 id;
        int endtime;

        public ushort Npc { get { return id; } }
        public bool isActive { get { return (isAi) ? true : timer.IsRunning; } }
        public cRiceBall(Player ty)
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
                    SendPacket p = new SendPacket();
                    p.Pack(new byte[] { 5, 5 });
                    p.Unpack32(own.ID);
                    p.Pack(id);
                    own.CurrentMap.Broadcast(p);
                    p = new SendPacket();
                    p.Pack(new byte[] { 23, 207 });
                    p.Pack(1);
                    p.Pack(1);
                    own.Send(p);
                }
            }
        }
        public void Deactivate()
        {
            if (id > 0)
            {
                if (!CheckForStop()|| isAi)
                {
                    timer.Stop();
                    endtime -= timer.Elapsed.Seconds;
                    SendPacket p = new SendPacket();
                    p.Pack(new byte[] { 5, 5 });
                    p.Unpack32(own.ID);
                    p.Pack(0);
                    own.CurrentMap.Broadcast(p);
                    p = new SendPacket();
                    p.Pack(new byte[] { 23, 207 });
                    p.Pack(1);
                    p.Pack(2);
                    own.Send(p);
                }
            }
        }
        public bool Set(byte invCell)
        {
            try
            {
                id = own.Inv[invCell].NpcID; //will be set from i.item.npcID
                endtime = 60;
                Activate();
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
            SendPacket p = new SendPacket();
            p.Pack(new byte[] { 23, 207 });
            p.Pack(2);
            p.Pack((ushort)(endtime - timer.Elapsed.Seconds));
            own.Send(p);
        }
        public void Update()
        {
            if(isActive && CheckForStop())
            {
                Deactivate();
                id = 0;
            }
        }
        public bool CheckForStop() { return (timer.Elapsed.Seconds >= endtime); }



    }
}
