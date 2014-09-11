using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Enums;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.DataManagement.DataFiles;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Pet : PetEquipementManager
    {
        public Npc npcData;
        public byte Groupslot;
        byte m_amity;
        string name;

        public UInt32 OwnerID { get { return (owner != null) ? owner.ID : 0; } }
        public UInt16 ID { get { return npcData.NpcID; } }
        public byte Amity { get { return m_amity; } }
        public string Name { get { return (!string.IsNullOrEmpty(name)) ? name : npcData.NpcName; } }

        public Skill Skill1 { get { return new Skill(); } }
        public Skill Skill2 { get { return new Skill(); } }
        public Skill Skill3 { get { return new Skill(); } }

        public Pet(Npc src)
        {
            npcData = src;
            //base.myData = this;
            m_amity = 60;

            base.CurHP = (int)npcData.HP;
            base.CurSP = (int)npcData.SP;
            base.Str = npcData.STR;
            base.Con = npcData.CON;
            base.Int = npcData.INT;
            base.Wis = npcData.WIS;
            base.Agi = npcData.AGI;
        }
    }
    
    
    public class cPetList
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
                if (value != null && host.inGame)
                {
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 19, 1 });
                    tmp.Pack32(value.ID);
                    host.Send(tmp);                    
                }
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
        public cPetList(Player i)
        {
            host = i;
            petlist = new Dictionary<byte, Pet>();
          
        }


        public bool Load()
        {
            return true;
        }
        public bool Save()
        {
            return true;
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

            for (byte n = 1; n <= 4; n++)
            {
                if (petlist.ContainsKey(n) && petlist[n] != null)
                    petlist[n].Send8_2();
            }
        }
        public SendPacket PetlistData
        {
            get
            {
                SendPacket p = new SendPacket();
                 
                p.PackArray(new byte[] { 15, 8 });

                for (byte n = 1; n <= 4; n++)
                {
                    if (petlist.ContainsKey(n) && petlist[n] != null)
                    {
                        p.Pack8(n);
                        p.Pack16(petlist[n].ID);
                        p.Pack32((uint)petlist[n].TotalExp);
                        p.Pack8(petlist[n].Level);
                        p.Pack32(100);// p.Pack32((uint)petlist[n].CurHP);
                        p.Pack16(100);// p.Pack16((ushort)petlist[n].CurSP);
                        p.Pack16(5);// p.Pack16(petlist[n].Int);
                        p.Pack16(5);//p.Pack16(petlist[n].Str);
                        p.Pack16(5);//p.Pack16(petlist[n].Con);
                        p.Pack16(5);//p.Pack16(petlist[n].Agi);
                        p.Pack16(5);//p.Pack16(petlist[n].Wis);
                        p.Pack8(0);
                        p.Pack8(petlist[n].Amity);
                        p.Pack16(1);
                        p.Pack8(0);
                       
                        p.Pack8(petlist[n].Skill1.Grade);
                        p.Pack32(petlist[n].Skill1.Exp);
                        p.Pack8(petlist[n].Skill2.Grade);
                        p.Pack32(petlist[n].Skill2.Exp);
                        p.Pack8((petlist[n].Reborn) ? petlist[n].Skill3.Grade : (byte)0);
                        p.Pack32((petlist[n].Reborn) ? petlist[n].Skill3.Exp : (byte)0);
                        p.PackArray(petlist[n].FullEqData);
                        p.Pack16(0);
                        p.Pack8(0);//reborn?
                        p.Pack8(0);//potential
                    }
                }
                if (p.Data.Count > 7)
                    return p;
                else
                    return null;
            }
        }

        public void RecievePet(Npc src, uint orride)
        {
            src.NpcID = (ushort)orride;
            RecievePet(src, true);
        }
        public void RecievePet(Npc pet, bool load = false)
        {
            if (pet.NpcID == 0) return;
            byte a = 1;

            while (petlist.ContainsKey(a)) { a++; }

            Pet tmp = new Pet(pet);
            //tmp.Owner = host;
            tmp.Groupslot = a;
            petlist.Add(a, tmp);
            if (!load)
            {
                SendPacket pkt = new SendPacket();
                pkt.PackArray(new byte[] { 15, 1 });
                pkt.Pack32(host.ID);
                pkt.Pack32(pet.NpcID);
                pkt.Pack8(2);//testing if level
                pkt.Pack32((uint)tmp.TotalExp);
                pkt.Pack8(tmp.Skill1.Grade);
                pkt.Pack32(tmp.Skill1.Exp);
                pkt.Pack8(tmp.Skill2.Grade);
                pkt.Pack32(tmp.Skill2.Exp);
                pkt.Pack8(tmp.Skill3.Grade);
                pkt.Pack32(tmp.Skill3.Exp);
                pkt.Pack8(tmp.Amity);
                pkt.Pack16(0);
                pkt.Pack16(0);
                pkt.Pack8(0);
                host.Send(pkt);

                if (BattlePet != null)
                    Rest_Pet();

                BattlePet = tmp;
            }
        }
        public void DismissPet(byte slot)
        {
            if (petlist.ContainsKey(slot))
            {
                if (petlist.Remove(slot))
                {
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 15, 2 });
                    tmp.Pack32(host.ID);
                    tmp.Pack8(slot);
                    host.Send(tmp);
                }
            }
        }
        public void onPetLeaving(Pet i,byte slot)
        {

        }
        public void Bring_into_Battle(byte slot)
        {
            if (petlist.ContainsKey(slot) )
            {
                BattlePet = petlist[slot];
                if (host.CurrentMap != null)
                {
                    SendPacket tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 15, 4 });
                    tmp.Pack32(host.ID);
                    tmp.Pack32(BattlePet.ID);
                    tmp.Pack8(0);
                    tmp.Pack8(1);
                    tmp.PackString(BattlePet.Name);
                    tmp.Pack16(0);//weapon
                    host.CurrentMap.Broadcast(tmp, host.ID);
                }
            }
        }
        public void Rest_Pet()
        {           
            SendPacket tmp = new SendPacket();
            tmp.PackArray(new byte[] { 19, 2 });
            host.Send(tmp);
            tmp = new SendPacket();
            tmp.PackArray(new byte[] { 19, 7 });
            tmp.Pack32(host.ID);
            host.CurrentMap.Broadcast(tmp, host.ID);
            BattlePet = null;
        }
        public void onRidePet(byte slot)
        {
            if (petlist.ContainsKey(slot))
            {
                if (BattlePet == petlist[slot])
                {
                    SendPacket tmp = new SendPacket();
                    tmp = new SendPacket();
                    tmp.PackArray(new byte[] { 19, 7 });
                    tmp.Pack32(host.ID);
                    host.CurrentMap.Broadcast(tmp, host.ID); RidePet = BattlePet; BattlePet = null;
                }
                SendPacket f = new SendPacket();
                f.PackArray(new byte[] { 15, 16 });
                f.Pack8((byte)slot);
                f.Pack32(host.ID);
                f.Pack32(petlist[slot].ID);
                host.CurrentMap.Broadcast(f);
                host.Send8_1();
            }
        }
        public void onUnRidePet()
        {
            if (RidePet != null)
            {
                SendPacket f = new SendPacket();
                f.PackArray(new byte[] { 15, 17 });
                f.Pack32(host.ID);
                host.CurrentMap.Broadcast(f);
                host.Send8_1();
                RidePet = null;
            }
        }
    }
}
