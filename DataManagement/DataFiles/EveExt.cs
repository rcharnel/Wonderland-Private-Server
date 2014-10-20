using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wonderland_Private_Server.Code.Objects;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    #region Enums
    public enum Dialogtype
    {
        none,
        yes_no,
        invspacechek,
        petspacecheck,
        itemcheck,
        npccheck,

    }

    #endregion

    public class Dialog
    {
        public Dialogtype type;
        public int req;

        public byte[] reg;
        public byte[] yes;
        public byte[] no;

    }
    public class MapData
    {
        public UInt16 mapID;
        public UInt16 sceneID;
        public UInt32 dataptr;
        public UInt16 datalen;
        public UInt16 unknownword;
        public SceneInfo Scene;
        public List<BattleInfoEntries> ExtBattleInfo = new List<BattleInfoEntries>();
        public List<NpcEntries> Npclist = new List<NpcEntries>();
        public List<Entry_Exit_Point_Entries> Entry_Points = new List<Entry_Exit_Point_Entries>();
        public List<MiningAreaEntries> MiningAreas = new List<MiningAreaEntries>();
        public List<ItemsinMapEntries> ItemAreas = new List<ItemsinMapEntries>();
        public List<EventsinMapEntries> Events = new List<EventsinMapEntries>();
        public List<GroupEntries> Group = new List<GroupEntries>();
        public List<WarpInfo> WarpLoc = new List<WarpInfo>();
        public List<preEventEntries> PreEvents = new List<preEventEntries>();
        public List<InteractiveInfoEntries> InteractiveInfo = new List<InteractiveInfoEntries>();
        public List<ExtgroupEntries> ExtGroup = new List<ExtgroupEntries>();
        public categoryoffset offsetlist = new categoryoffset();
    }
    public class npcWalkStep
    {
        public UInt32 x;
        public UInt32 y;
        public UInt32 z;
    }
    public class walkpattern
    {
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
        public byte steps_needed;
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public List<npcWalkStep> walksteps = new List<npcWalkStep>();
    }
    public class NpcEntries
    {
        public enum Interaction_Type
        {
            none,
            Talking,
            Answering,
            Buying,
            Selling,
        }
        TimeSpan LastMove = new TimeSpan();
        int step = 0;

        #region MapNpc Info
        public UInt16 clickId;
        public string Name;
        public byte unknownbyte1;
        public UInt32 x;
        public UInt32 y;
        public List<byte> Events = new List<byte>();
        public List<byte> unknownbytearray2 = new List<byte>();
        public byte unknownbyte2; //usually 0
        public UInt32 npcId;
        public byte unknownbyte3;
        public byte unknownbyte4;
        public byte unknownbyte5;
        public List<npcWalkStep> walksteps = new List<npcWalkStep>();
        public byte unknownbyte6;
        public byte unknownbyte7;
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public byte unknownbyte8;
        public byte unknownbyte9;
        public byte unknownbyte10;
        public List<walkpattern> walkpatterns = new List<walkpattern>();
        public UInt16 unknownword1;
        public UInt16 unknownword2; //usually 255
        public UInt16 unknownword3;
        public UInt16 unknownword4;
        public UInt16 unknownword5;
        #endregion

        public bool NeedUpdate(TimeSpan time)
        {
                if (walksteps.Count > 0)
                    if ((time - LastMove) > new TimeSpan(0, 0, 5))
                        return true;
                    else
                        return false;
                return false;
        }
        //public void Update(List<cCharacter> t,cGlobals g)
        //{
        //    for (int a = 0; a < t.Count; a++)
        //        if (!t[a].warping)
        //            Walk(t[a], g);
        //    LastMove = g.UpTime.Elapsed;
        //}
        //public void Walk(cCharacter src,cGlobals g)
        //{
        //    cSendPacket y = new cSendPacket(g);
        //    if (step == walksteps.Count)
        //    {
        //        step = 0;
        //        y = new cSendPacket(g);
        //        y.Header(22, 2);
        //        y.AddWord(this.clickId);
        //        y.AddWord((ushort)this.x);
        //        y.AddWord((ushort)this.y);
        //        y.AddByte(this.unknownbyte6);
        //        y.SetSize();
        //        y.cCharacter = src;
        //        y.Send();
        //    }
        //    //send new step
        //    y = new cSendPacket(g);
        //    y.Header(22, 2);
        //    y.AddWord(this.clickId);
        //    y.AddWord((ushort)this.walksteps[step].x);
        //    y.AddWord((ushort)this.walksteps[step].y);
        //    y.AddByte(3);
        //    y.SetSize();
        //    step++;
        //    y.SetSize();
        //    y.cCharacter = src;
        //    y.Send();
        //}

        //public void Interact(Interaction_Type l = Interaction_Type.none, byte answer = 0, byte slot = 0, byte ammt = 0)
        //{
        //    var events = location.mapData.Events[Events[0] - 1];

        //    switch (l)
        //    {
        //        case Interaction_Type.Talking:
        //            {
        //                LoadDialog(events, 1); 
        //                globals.packet.cCharacter.NpcTalk(true);
        //            }break;
        //        case Interaction_Type.Answering:
        //            {
        //                foreach (EventSubEntry j in events.SubEntry)
        //                    if (j.unknownword2 == answer)
        //                        LoadDialog(events, j.subIndex);
        //            }break;
        //        case Interaction_Type.Buying:
        //            {
        //                //try find slot info i guess<--
        //            }break;
        //    }
            
        //}
        //void LoadDialog(EventsinMapEntries events, byte unk)
        //{
        //    unk -= 1;
        //    bool Add = true;
        //    for (int a = 0; a < events.SubEntry[unk].SubEntry.Count; a++)
        //    {
        //        cSendPacket talk = new cSendPacket(globals);
        //        talk.Header(20);
        //        talk.AddDWord(1);
        //        talk.AddByte((byte)events.SubEntry[events.unknownbyte1 - 1].SubEntry[a].subsubIndex);
        //        switch (events.SubEntry[unk].SubEntry[a].DialogPtr)
        //        {
        //            case 1:
        //                {
        //                    talk.AddByte((byte)events.SubEntry[unk].SubEntry[a].dialog3);
        //                    talk.AddByte(7);
        //                    talk.AddWord(0);
        //                    talk.AddDWord(events.SubEntry[unk].SubEntry[a].unknowndword2);
        //                    talk.AddByte(0);
        //                    talk.AddWord(events.SubEntry[unk].SubEntry[a].dialog2);
        //                    talk.AddByte(events.unknownbyte1);
        //                    if (events.SubEntry[unk].SubEntry[a].dialog1 == 3)//warp event?
        //                    {
        //                        location.WarpRequest( WarpType.Questwarp,globals.packet.cCharacter,events.SubEntry[unk].SubEntry[a].dialog2,  null);
        //                        Add = false;
        //                    }
        //                } break;
        //            case 2:
        //                {
        //                    talk.AddByte((byte)events.SubEntry[unk].SubEntry[a].dialog2);
        //                    talk.AddByte(3);
        //                    talk.AddWord(events.SubEntry[unk].SubEntry[a].dialog1);
        //                    talk.AddDWord(events.SubEntry[unk].SubEntry[a].unknowndword2);
        //                    talk.AddByte(0);
        //                    talk.AddWord(events.SubEntry[unk].SubEntry[a].dialog3);
        //                    talk.AddByte(events.unknownbyte1);
        //                } break;
        //            case 8:
        //                {
        //                    talk.AddByte(5);
        //                    talk.AddByte(0);
        //                    talk.AddWord(0);
        //                    talk.AddByte((byte)events.SubEntry[unk].SubEntry[a].dialog1);
        //                    var gh = BitConverter.GetBytes(events.SubEntry[unk].SubEntry[a].dialog4);
        //                    talk.AddByte(gh[1]);
        //                    talk.AddDWord(events.SubEntry[unk].SubEntry[a].unknowndword1);
        //                    talk.AddWord(0);
                            
        //                } break;
        //            case 5:
        //                {
        //                } break;
        //            case 7:
        //                {
        //                    switch (events.SubEntry[unk].SubEntry[a].dialog1)
        //                    {
        //                        case 1: break; //weap keeper sell
        //                        case 2: globals.ac27.Send_3(); break;//props shopkeeper sell
        //                        case 3: break;//found a guild
        //                        case 5: globals.ac31.Send_7(); break;//npc record
        //                        case 4: globals.ac29.Send_6(); break;//props keeper
        //                        case 6: break; //welling props/money acess
        //                        case 7: break;//hotel accomendation/Healing
        //                        case 9: break;//stock keeper
        //                        case 24: break; //Lost item keeper
        //                    }
        //                } break;

        //        }
        //        if (Add)
        //        {
        //            talk.SetSize(); globals.packet.cCharacter.DatatoSend.Enqueue(talk);
        //        }
        //    }
        //}
    }
    public class Entry_Exit_Point_Entries
    {
        public UInt16 clickID;
        public string Name;
        public byte unknownbyte1;
        public UInt32 x;//map x location
        public UInt32 y;//map y location
        public List<byte> unknownbytearray1 = new List<byte>();
        public List<byte> unknownbytearray2 = new List<byte>();
        public byte unknownbyte2; //usually 0
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public byte unknownbyte3;
        public UInt32 unknowndword3;
        public UInt32 unknowndword4;
        public virtual bool Enter(ref Player src)
        {
            //portal Requirements
            return true;
        }

    }
    public class MiningAreaEntries
    {
        public UInt16 clickID;
        public string Name;
        public byte unknownbyte1;
        public UInt32 x;
        public UInt32 y;
        public List<byte> unknownbytearray1 = new List<byte>();
        public List<byte> unknownbytearray2 = new List<byte>();
        public byte unknownbyte2; //usually 0
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public byte unknownbyte3;
        public byte unknownbyte4;
    }
    public class ItemsinMapEntries
    {
        public UInt16 clickID;
        public bool pickedup;
        public TimeSpan dropin;
        public string Name;
        public byte unknownbyte1;
        public UInt32 x;
        public UInt32 y;
        public List<byte> unknownbytearray1 = new List<byte>();
        public List<byte> unknownbytearray2 = new List<byte>();
        public byte unknownbyte2; //usually 0
        public bool Drop(TimeSpan TimeSpan)
        {
            if (!pickedup) return false;
            else if (dropin < TimeSpan)
                return true;
            else
                return false;
        }
        public UInt32 itemID;
        public byte unknownbyte3;
        public byte unknownbyte4;
        public byte unknownbyte5;
        public UInt16 unknownword1;
        public UInt16 unknownword2;
    }
    public class EventSubEntry
    {
        public byte subIndex;
        public byte unknownbyte1;
        public UInt16 unknownword1;
        public UInt16 unknownword2;
        public UInt16 unknownword3;
        public UInt16 unknownword4;
        public UInt16 unknownword5;
        public UInt16 unknownword6;
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public List<EventSubSubEntry> SubEntry = new List<EventSubSubEntry>();
    }
    public struct EventSubSubEntry
    {
        public byte subsubIndex;
        public byte DialogPtr;
        public UInt16 dialog1;
        public UInt16 dialog2;
        public UInt16 dialog3;
        public UInt16 dialog4;
        public UInt32 unknowndword1;
        public UInt32 unknowndword2;
        public UInt32 unknowndword3;
    }
    public class EventsinMapEntries
    {
        public UInt16 clickID;
        public byte unknownbyte1;
        public string Name;
        public List<EventSubEntry> SubEntry = new List<EventSubEntry>();
    }
    public class GroupSubEntry
    {
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
    }

    public class GroupEntries
    {
        public UInt16 clickID;
        public string Name;
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
        public UInt16 unknownword1;
        public List<GroupSubEntry> subentry = new List<GroupSubEntry>();
        public byte unknownbyte4;
    }
    public class WarpInfo
    {
        public UInt16 clickID;
        public string Name;
        public UInt16 mapID;
        public UInt32 x;
        public UInt32 y;
        public byte unknownbyte1;
        public byte neededtopass;
        public byte unknownbyte3;
    }
    public class InteractiveInfoSubEntry
    {
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
        public byte unknownbyte4;
        public byte unknownbyte5;
    }
    public class InteractiveInfoEntries
    {
        public UInt16 entryID;
        public string Name;
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
        public List<InteractiveInfoSubEntry> subentry = new List<InteractiveInfoSubEntry>();
    }
    public class BattleInfoSubEntry
    {
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
    }
    public class BattleInfoEntries
    {
        public UInt16 entryID;
        public string Name;
        public byte unknownbyte1;
        public byte unknownbyte2;
        public byte unknownbyte3;
        public byte unknownbyte4;
        public byte unknownbyte5;
        public byte unknownbyte6;
        public byte unknownbyte7;
        public byte unknownbyte8;
        public byte unknownbyte9;
        public byte unknownbyte10;
        public byte unknownbyte11;
        public byte unknownbyte12;
        public byte unknownbyte13;
        public byte unknownbyte14;
        public byte unknownbyte15;
        public byte unknownbyte16;
        public List<BattleInfoSubEntry> subentry1 = new List<BattleInfoSubEntry>();
        public List<BattleInfoSubEntry> subentry2 = new List<BattleInfoSubEntry>();
        public byte unknownbyte17;
    }
    public class preEventSubEntry
    {
        public byte subIndex;
        public List<byte> unknown = new List<byte>();
        public List<preEventSubSubEntry> subentry2 = new List<preEventSubSubEntry>();
    }
    public class preEventSubSubEntry
    {
        public byte subIndex;
        List<byte> unknown = new List<byte>();
    }
    public class preEventEntries
    {
        public UInt16 clickID;
        public byte unknownbyte1;
        public string Name;
        public List<preEventSubEntry> subentry1 = new List<preEventSubEntry>();


    }
    public class extGroupSubEntry
    {
        public byte Index;
        public byte unknownbyte1;
        public byte unknownbyte2;

    }
    public class ExtgroupEntries
    {
        public UInt16 clickID;
        public string Name;
        public byte unknownbyte1;
        public UInt16 unknownword1;
        public UInt16 unknownword2;
        public List<extGroupSubEntry> subEntry = new List<extGroupSubEntry>();
    }
    public struct categoryoffset
    {
        public UInt32 Scenedata;
        public UInt32 NPC;
        public UInt32 Entry;
        public UInt32 Mining;
        public UInt32 Items;
        public UInt32 Events;
        public UInt32 Groups;
        public UInt32 Warp;
        public UInt32 Interactiveinfo;
        public UInt32 Battleinfo;
        public UInt32 PreEvent;
        public UInt32 groupext;

    }
}
