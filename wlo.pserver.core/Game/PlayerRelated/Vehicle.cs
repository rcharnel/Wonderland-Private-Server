using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;


namespace PhoenixGameServer.Game.Data.Code
{
    public class Veichle
    {
        public Player owner;
        public ushort ID;
        public byte invLoc;
        const int maxdamage = 1800;
        public Item item;
        ushort MaxFuel;
        public ushort Fuelleft;
        public ushort dmg;
        public ushort HP;
        List<Player> PassengersList = new List<Player>();
        public List<Player> Passengers { get { return PassengersList; } }
        public bool Broke { get { if (dmg == maxdamage)return true; else return false; } }
        public bool hasFuel { get { if (Fuelleft > 0)return true; else return false; } }
        public Veichle()
        {
        }
        public void DriverGetin(Player t)
        {
            owner = t;
            FuelGuage();
        }
        public void AddPassenger(Player t)
        {
            PassengersList.Add(t);
        }
        public void RemPassenger(Player t, bool All)
        {
            if (t == owner)
                owner = null;
            if (All)
            {
                PassengersList.Clear();
                return;
            }
            PassengersList.Remove(t);

        }
        public void FuelUP(int ammt)
        {
            if (Fuelleft + ammt < MaxFuel)
            {
                Fuelleft += (ushort)ammt;
            }
            else
                Fuelleft = MaxFuel;
            FuelGuage();
        }
        public void UseFuel(int amt = 50)
        {
            if (hasFuel)
            {
                Fuelleft -= (ushort)amt;
                if (Fuelleft < 0)
                    Fuelleft = 0;
            }
            else
                dmg += (ushort)new Random().Next(amt, amt + 1);
            if (dmg > maxdamage)
            {
                dmg = maxdamage;
            }
            FuelGuage();
        }
        void FuelGuage()
        {
            //SendPacket veh = new SendPacket();
            //veh.Header(15, 14);
            //veh.AddByte(invLoc);
            //veh.AddDWord(owner.characterID);
            //if (hasFuel)
            //{
            //    veh.AddDWord(0);
            //    veh.AddWord((ushort)(Fuelleft));
            //}
            //else
            //{
            //    veh.AddWord((ushort)(dmg));
            //    veh.AddDWord(0);
            //}
            //veh.SetSize();
            //veh.Player = owner;
            //veh.Send();
        }

    }
    public class TransportationManager
    {
        List<Veichle> myStoredVech = new List<Veichle>(4);
        List<Veichle> myInvVech = new List<Veichle>();
        Veichle vech_riding;

        public bool inVechile { get { if (vech_riding != null) return true; else return false; } }
        public Veichle Car { get { return vech_riding; } }
        public TransportationManager()
        {
        }

        void StoreVech()
        {
        }
        void GetStoredVech()
        {
        }

        public void VechWalk()
        {
            if (!vech_riding.Broke)
                vech_riding.UseFuel();
            else
                VechBroke(vech_riding);

        }
        public void VechBroke(Veichle tool)
        {
            //own.inv.RemoveInv(tool.invLoc, 1);
            //globals.ac15.Send_15(own.characterID, tool.ID);
            //cSendPacket veh = new cSendPacket(globals);
            //veh.Header(15, 11);
            //veh.AddByte(tool.invLoc);
            //veh.AddDWord(own.characterID);
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //veh = new cSendPacket(globals);
            //veh.Header(15, 13);
            //veh.AddDWord(own.characterID);
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //veh = new cSendPacket(globals);
            //own.Spawnto(1);
            //vech_riding = null;
            //Rem(tool.invLoc);
        }
        public Veichle GetVechilebyID(ushort ID)
        {
            for (int a = 0; a < myInvVech.Count; a++)
            {
                if (myInvVech[a].ID == ID)
                {
                    return myInvVech[a];
                }
            }
            return null;
        }
        public void PullOut(Veichle tool)
        {
            //try
            //{
            //    cSendPacket veh = new cSendPacket(globals);
            //    veh.Header(15, 12);
            //    veh.AddByte(tool.invLoc);
            //    veh.AddDWord(own.characterID);
            //    veh.AddWord(tool.ID);
            //    veh.AddDWord((uint)(own.x + new Random().Next(0, 40)));
            //    veh.AddDWord((uint)(own.y - new Random().Next(0, 20)));
            //    veh.SetSize();
            //    globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //}
            //catch { }
        }
        public void RideVech(Veichle tool, bool warping = false)
        {
            //cSendPacket veh = new cSendPacket(globals);
            //veh.Header(15, 10);
            //veh.AddByte(tool.invLoc);
            //veh.AddDWord(own.characterID);
            //veh.AddWord(tool.ID);
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //if (!warping)
            //{
            //    vech_riding = tool;
            //    vech_riding.DriverGetin(own);
            //    if (own.Party.Count > 0)
            //    {
            //        for (int a = 0; a < own.Party.Count; a++)
            //            vech_riding.AddPassenger(own.Party.TeamMembers[a]);
            //    }
            //}
        }
        public void LeaveVech(byte slot, Veichle tool)
        {
            if (vech_riding == null)
                PutinVech(slot, tool);
            else
            {
                unRideVech(slot, tool);
                vech_riding = null;
            }
        }
        public void AutoOpen(Veichle tool)
        {
            //cSendPacket g = new cSendPacket(globals);
            //g.Header(15, 18);
            //g.AddByte(tool.invLoc);
            //g.AddDWord(own.characterID);
            //g.AddWord(tool.ID);
            //g.AddDWord((uint)(own.x - 1));
            //g.AddDWord((uint)(own.y - 1));
            //g.SetSize();
            //g.cCharacter = own;
            //g.Send();
        }
        void unRideVech(byte invslot, Veichle tool)
        {
            //cSendPacket veh = new cSendPacket(globals);
            //veh.Header(15, 11);
            //veh.AddByte(invslot);
            //veh.AddDWord(own.characterID);
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //veh = new cSendPacket(globals);
            //veh.Header(15, 12);
            //veh.AddByte(invslot);
            //veh.AddDWord(own.characterID);
            //veh.AddWord(tool.ID);
            //veh.AddDWord((uint)(own.x + new Random().Next(0, 40)));
            //veh.AddDWord((uint)(own.y - new Random().Next(0, 20)));
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharacters(veh);
            //vech_riding.RemPassenger(own, true);

        }
        void PutinVech(byte unk, Veichle y)
        {
            //cSendPacket veh = new cSendPacket(globals);
            //veh.Header(15, 22);
            //veh.AddDWord(own.characterID);
            //veh.AddWord(y.ID);
            //veh.SetSize();
            //globals.gDataManager.MapManager.GetMapByID(own.mapLoc).SendtocCharactersEx(veh, own);
            //globals.ac15.Send_13(unk);

        }
        void RefuelVech(int red)
        {
            //cSendPacket veh = new cSendPacket(globals);
            //veh.Header(15, 14);
            //veh.AddByte(3);
            //veh.AddDWord(own.characterID);
            //veh.AddDWord(0);
            //veh.AddWord((ushort)(vech_riding.Fuelleft + red));
        }
        public void Add(ushort ID, int slot)
        {
            //Veichle j = new Veichle(globals);
            //j.owner = own;
            //var sslot = globals.NumbertoMatrix(slot);
            //j.item = own.inv.mainInv[sslot[0]][sslot[1]].itemtype;
            //j.invLoc = (byte)slot;
            //j.ID = own.inv.mainInv[sslot[0]][sslot[1]].ID;
            //j.HP = 100;
            //myInvVech.Add(j);
        }
        public void Rem(int slot)
        {
            for (int a = 0; a < myInvVech.Count; a++)
                if (myInvVech[a].invLoc == slot)
                    myInvVech.Remove(myInvVech[a]);
        }
        void RepairVech()
        {
        }

    }
}
