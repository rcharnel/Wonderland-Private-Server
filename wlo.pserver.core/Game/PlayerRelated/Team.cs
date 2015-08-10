using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonderland_Private_Server.Code.Objects
{
    //public interface TeamManager
    //{
    //    List<Player> TeamMembers { get; }
    //    public bool PartyLeader { get; }
    //    public bool hasParty { get; }

    //    public SendPacket _13_6Data { get; }

    //    public void KickMember(Player t)
    //    {
    //        MemberLeave(t);
    //    }
    //    public void MakeLeader()
    //    {
    //        SendPacket f = new SendPacket();
    //        f.Header(13, 15);
    //        f.Pack(3);
    //        f.Pack(own.CharacterTemplateID);
    //        foreach (Player u in myTeamMembers.ToArray())
    //            if (u.character.MyTeam.PartyLeader)
    //            {
    //                f.Pack(u.CharacterTemplateID);
    //            }
    //        f.SetSize();
    //        leader = true;
    //        own.currentMap.Broadcast(f);

    //    }
    //    public void MemberLeave(Player l)
    //    {
    //        if (l.character.MyTeam.leader)
    //            EndTeam();
    //        else if (myTeamMembers.Contains(l))
    //        {
    //            Rem(l);
    //        }
    //        Rem(l);
    //        l.character.MyTeam.Leave();
    //    }
    //    public void Leave()
    //    {
    //        Send_5(53);
    //        Send_5(54);
    //        Send_5(183);
    //        SendPacket p = new SendPacket();
    //        p.Header(13, 4);
    //        p.Pack(own.CharacterTemplateID);
    //        p.SetSize();
    //        own.currentMap.Broadcast(p);
    //        leader = false;
    //        myTeamMembers.Clear();
    //    }
    //    public void EndTeam()
    //    {
    //        //end party
    //        foreach (Player s in myTeamMembers.ToArray())
    //            s.character.MyTeam.Leave();
    //    }
    //    public void Add(Player t) { myTeamMembers.Add(t); }
    //    public void Rem(Player t) { myTeamMembers.Remove(t); }

    //    public void AcceptMember(byte value, Player t, bool join)
    //    {
    //        if (myTeamMembers.Exists(c => c.CharacterTemplateID == t.CharacterTemplateID)) return;

    //        SendPacket p = new SendPacket();
    //        if (!join)
    //        {
    //            if (!leader && myTeamMembers.Count == 0)
    //                leader = true;
    //            p.Header(13, 3);

    //        }
    //        else
    //            p.Header(13, 10);
    //        p.Pack(value);
    //        p.Pack(own.CharacterTemplateID);
    //        p.SetSize();
    //        globals.SendPacket(t, p);
    //        t.character.MyTeam.Add(globals.FindPlayerby_CharacterTemplateID(own.CharacterTemplateID));
    //        p = new SendPacket();
    //        p.Header(13, 5);
    //        p.Pack(own.CharacterTemplateID);
    //        p.Pack(t.CharacterTemplateID);
    //        p.SetSize();
    //        own.currentMap.Broadcast(p);
    //        own.Send_8_3(t);
    //        t.character.Send_8_3(own.PlayerREf);

    //        foreach (Player y in myTeamMembers.ToArray())
    //        {
    //            t.character.Send_8_3(y);
    //            y.character.Send_8_3(t);
    //            t.character.MyTeam.Add(y);
    //        }
    //        Add(t);
    //    }
    //    public void JoinTeam(Player t)
    //    {

    //        SendPacket p = new SendPacket();
    //        p.Header(13, 1);
    //        p.Pack(own.CharacterTemplateID);
    //        p.SetSize();
    //        globals.SendPacket(t, p);
    //    }
    //    public void InvitetoTeam(Player t)
    //    {
    //        SendPacket f = new SendPacket();
    //        f.Header(13, 9);
    //        f.Pack(own.CharacterTemplateID);
    //        f.SetSize();
    //        globals.SendPacket(t, f);
    //    }
    //    public void AppointLeader(byte value, Player to)
    //    {
    //        SendPacket f = new SendPacket();

    //        f.Header(13, 15);
    //        f.Pack(value);
    //        f.Pack(own.CharacterTemplateID);
    //        f.SetSize();
    //        globals.SendPacket(to, f);
    //    }
    //    public void LeaderChanged(byte value, Player from, Player to)
    //    {
    //        if (from.CharacterTemplateID == own.CharacterTemplateID)
    //            leader = false;
    //    }

    //    public void Send_5(byte value)
    //    {
    //        SendPacket p = new SendPacket();
    //        p.Header(25, 5);
    //        p.Pack(value);
    //        p.Pack(0);
    //        p.SetSize();
    //        globals.SendPacket(globals.FindPlayerby_CharacterTemplateID(own.CharacterTemplateID), p);
    //    }
    //}
}
