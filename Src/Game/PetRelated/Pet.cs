using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using DataFiles;
using Wlo.Core;

namespace Game.Code.PetRelated
{
    public class Pet : Game.Code.PetRelated.PetEquipManager
    {
        string name;

        public UInt32 OwnerID { get { return (owner != null) ? owner.CharID : 0; } set { } }
        public uint ID { get { return npcData.NpcID; } }
        public byte Amity { get { return m_amity; } set { m_amity = value; } }
        public string Name { get { return (!string.IsNullOrEmpty(name)) ? name : ASCIIEncoding.ASCII.GetString(npcData.NpcName); } }

        //public Skill Skill1 { get { return new Skill(); } }
        //public Skill Skill2 { get { return new Skill(); } }
        //public Skill Skill3 { get { return new Skill(); } }

        public Pet(byte mslot, Npc src, Player owner)
            : base(new Action<RCLibrary.Core.Networking.IPacket>(owner.SendPacket))
        {
            Slot = mslot;
            this.owner = owner;
            npcData = src;
            m_amity = 60;

            base.Str = npcData.STR;
            base.Con = npcData.CON;
            base.Int = npcData.INT;
            base.Wis = npcData.WIS;
            base.Agi = npcData.AGI;
        }

        #region Fighter Properties
        public FighterState BattleState { get { return FighterState.Alive; } }
        ////public Skill SkillEffect { get; set; }
        public BattleSide BattlePosition { get; set; }
        public eFighterType TypeofFighter { get { return eFighterType.Npc; } }
        public BattleAction myAction { get; set; }
        public UInt16 ClickID { get { return 0; } set { } }
        public byte GridX { get; set; }
        public byte GridY { get; set; }
        //public bool ActionDone { get { return (myAction != null || DateTime.Now > rndend); } }
        //public DateTime RdEndTime { set { rndend = value; } }
        public Int32 MaxHP { get { return FullHP; } }
        public Int16 MaxSP { get { return (short)FullSP; } }
        public override int CurHP
        {
            get
            {
                return base.CurHP;
            }
            set
            {
                base.CurHP = value;
            }
        }
        public override int CurSP
        {
            get
            {
                return base.CurSP;
            }
            set
            {
                base.CurSP = value;
            }
        }

        #endregion
    }


    public class PetList
    {
        readonly object mylock = new object();

        Player host;
        Dictionary<byte, Pet> petlist;
        Pet m_ridepet;
        Pet m_battlepet;

        public Pet BattlePet
        {
            get { return m_battlepet; }
            set
            {
                m_battlepet = value;
                if (value != null && host != null)
                    host.SendPacket(SendPacket.FromFormat("bbd", 19, 1, value.ID));
            }
        }
        public Pet RidePet { get { return m_ridepet; } set { m_ridepet = value; } }
        public Pet this[byte key]
        {
            get
            {
                lock (mylock)
                {
                    // If this key is in the dictionary, return its value.
                    if (petlist.ContainsKey(key))
                    {
                        // The key was found; return its value. 
                        return petlist[key];
                    }
                    else
                    {
                        // The key was not found; return null. 
                        return null;
                    }
                }
            }

            set
            {
                lock (mylock)
                {
                    // If this key is in the dictionary, change its value. 
                    if (petlist.ContainsKey(key))
                    {
                        // The key was found; change its value.
                        petlist[key] = value;
                    }
                }
            }
        }
        public PetList(Player i)
        {
            host = i;
            petlist = new Dictionary<byte, Pet>(20);

        }


        //public characterPet GetPetinBattleMode()
        //{
        //    foreach (characterPet e in myPets)
        //    {
        //        if (e.NpcID != 0)
        //            if (e.state == PetStatus.Battle && e.inPetGroup)
        //                return e;
        //    }
        //    return null;
        //}

        //public characterPet GetByID(UInt16 id)
        //{
        //    foreach (characterPet p in myPets)
        //        if (p.NpcID == id) return p;
        //    return null;
        //}
        //public characterPet GetBtSlot(byte slot)
        //{
        //    return myGroupPets[slot - 1];
        //}

        public void SendPetlistStatData()
        {

            //for (byte n = 1; n <= 4; n++)
            //{
            //    if (petlist.ContainsKey(n) && petlist[n] != null)
            //        petlist[n].Send8_1();
            //}
        }
        public SendPacket PetlistData
        {
            get
            {
                SendPacket p = new SendPacket();

                p.Pack(new byte[] { 15, 8 });

                for (byte n = 1; n <= 4; n++)
                {
                    if (petlist.ContainsKey(n) && petlist[n] != null)
                    {
                        p.Pack(n);
                        p.Pack(((ushort)petlist[n].ID));
                        p.Pack((uint)petlist[n].TotalExp);
                        p.Pack((byte)petlist[n].Level);
                        p.Pack(100);// p.Pack((uint)petlist[n].CurHP);
                        p.Pack((ushort)100);// p.Pack16((ushort)petlist[n].CurSP);
                        p.Pack((ushort)5);// p.Pack16(petlist[n].Int);
                        p.Pack((ushort)5);//p.Pack16(petlist[n].Str);
                        p.Pack((ushort)5);//p.Pack16(petlist[n].Con);
                        p.Pack((ushort)5);//p.Pack16(petlist[n].Agi);
                        p.Pack((ushort)5);//p.Pack16(petlist[n].Wis);
                        p.Pack((byte)0);
                        p.Pack(petlist[n].Amity);
                        p.Pack((ushort)1);
                        p.Pack((byte)0);

                        //p.Pack((byte)petlist[n].Skill1.Grade);
                        //p.Pack(petlist[n].Skill1.Exp);
                        //p.Pack((byte)petlist[n].Skill2.Grade);
                        //p.Pack(petlist[n].Skill2.Exp);
                        //p.Pack((byte)(petlist[n].Reborn) ? petlist[n].Skill3.Grade : (byte)0);
                        //p.Pack((petlist[n].Reborn) ? petlist[n].Skill3.Exp : (byte)0);
                        p.Pack(petlist[n].FullEqData);
                        p.Pack((ushort)0);
                        p.Pack((byte)0);//reborn?
                        p.Pack((byte)0);//potential
                    }
                }
                //if (p.Data.Count > 7)
                //    return p;
                //else
                return null;
            }
        }

        //public void RecievePet(Npc src, uint orride)
        //{
        //    src.NpcID = (ushort)orride;
        //    RecievePet(src, true);
        //}
        //public void RecievePet(Npc pet, bool load = false)
        //{
        //    if (pet.NpcID == 0) return;
        //    byte a = 1;

        //    while (petlist.ContainsKey(a)) { a++; }

        //    Pet tmp = new Pet(pet);
        //    //tmp.Owner = host;
        //    tmp.Groupslot = a;
        //    petlist.Add(a, tmp);
        //    if (!load)
        //    {
        //        SendPacket pkt = new SendPacket();
        //        pkt.PackArray(new byte[] { 15, 1 });
        //        pkt.Pack(host.ID);
        //        pkt.Pack(pet.NpcID);
        //        pkt.Pack((byte)2);//testing if level
        //        pkt.Pack((uint)tmp.TotalExp);
        //        pkt.Pack((byte)tmp.Skill1.Grade);
        //        pkt.Pack(tmp.Skill1.Exp);
        //        pkt.Pack((byte)tmp.Skill2.Grade);
        //        pkt.Pack(tmp.Skill2.Exp);
        //        pkt.Pack((byte)tmp.Skill3.Grade);
        //        pkt.Pack(tmp.Skill3.Exp);
        //        pkt.Pack((byte)tmp.Amity);
        //        pkt.Pack16(0);
        //        pkt.Pack16(0);
        //        pkt.Pack((byte)0);
        //        host.Send(pkt);

        //        if (BattlePet != null)
        //            Rest_Pet();

        //        BattlePet = tmp;
        //    }
        //}
        //public void DismissPet(byte slot)
        //{
        //    if (petlist.ContainsKey(slot))
        //    {
        //        if (petlist.Remove(slot))
        //        {
        //            SendPacket tmp = new SendPacket();
        //            tmp.PackArray(new byte[] { 15, 2 });
        //            tmp.Pack(host.ID);
        //            tmp.Pack((byte)slot);
        //            host.Send(tmp);
        //        }
        //    }
        //}
        //public void onPetLeaving(Pet i, byte slot)
        //{

        //}
        //public void Bring_into_Battle(byte slot)
        //{
        //    if (petlist.ContainsKey(slot))
        //    {
        //        BattlePet = petlist[slot];
        //        if (host.CurrentMap != null)
        //        {
        //            SendPacket tmp = new SendPacket();
        //            tmp.PackArray(new byte[] { 15, 4 });
        //            tmp.Pack(host.ID);
        //            tmp.Pack(BattlePet.ID);
        //            tmp.Pack((byte)0);
        //            tmp.Pack((byte)1);
        //            tmp.PackString(BattlePet.Name);
        //            tmp.Pack16(0);//weapon
        //            host.CurrentMap.Broadcast(tmp, host.ID);
        //        }
        //    }
        //}
        //public void Rest_Pet()
        //{
        //    SendPacket tmp = new SendPacket();
        //    tmp.PackArray(new byte[] { 19, 2 });
        //    host.Send(tmp);
        //    tmp = new SendPacket();
        //    tmp.PackArray(new byte[] { 19, 7 });
        //    tmp.Pack(host.ID);
        //    host.CurrentMap.Broadcast(tmp, host.ID);
        //    BattlePet = null;
        //}
        //public void onRidePet(byte slot)
        //{
        //    if (petlist.ContainsKey(slot))
        //    {
        //        if (BattlePet == petlist[slot])
        //        {
        //            SendPacket tmp = new SendPacket();
        //            tmp = new SendPacket();
        //            tmp.PackArray(new byte[] { 19, 7 });
        //            tmp.Pack(host.ID);
        //            host.CurrentMap.Broadcast(tmp, host.ID); RidePet = BattlePet; BattlePet = null;
        //        }
        //        SendPacket f = new SendPacket();
        //        f.PackArray(new byte[] { 15, 16 });
        //        f.Pack((byte)(byte)slot);
        //        f.Pack(host.ID);
        //        f.Pack(petlist[slot].ID);
        //        host.CurrentMap.Broadcast(f);
        //        host.Send8_1();
        //    }
        //}
        //public void onUnRidePet()
        //{
        //    if (RidePet != null)
        //    {
        //        SendPacket f = new SendPacket();
        //        f.PackArray(new byte[] { 15, 17 });
        //        f.Pack(host.ID);
        //        host.CurrentMap.Broadcast(f);
        //        host.Send8_1();
        //        RidePet = null;
        //    }
        //}
    }
}
