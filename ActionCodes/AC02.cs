using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;
using Wlo.Core;

namespace Wonderland_Private_Server.ActionCodes
{
    public class AC02 : AC
    {
        public override int ID { get { return 2; } }
        public override void ProcessPkt(ref Player r, RecvPacket p)
        {
            switch (p.B)
            {
                // case 1: Recv1(ref r, p); break;
                case 2: Recv2(ref r, p); break;
                default: Utilities.LogServices.Log("AC " + p.A + "," + p.B + " has not been coded"); break;
            }
        }
        void Recv1(ref Player p, RecvPacket r)
        {
            try
            {

            }
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
        void Recv2(ref Player p, RecvPacket r)
        {
            try
            {
                string str = r.UnpackNChar(2);
                string[] words = str.Split(' ');
                if (words.Length >= 1)
                {
                    switch (words[0])
                    {
                        #region item 
                        case ":item":                            
                            {
                                switch (words[1])
                                {
#region add
                                    case "add":
                                        {
                                            byte ammt = 1;
                                            int ct = words.Length;
                                            UInt16 itemid = 0;
                                            if (ct > 2)
                                            {

                                                try { itemid = UInt16.Parse(words[2]); }
                                                catch { }
                                                if (ct > 2)
                                                {
                                                    try { ammt = byte.Parse(words[3]); }
                                                    catch { }
                                                }
                                                InvItemCell i = new InvItemCell();
                                                i.CopyFrom(cGlobal.gItemManager.GetItem(itemid));
                                                i.Ammt = ammt;

                                                #region filter item
                                                switch (itemid)
                                                {
                                                   case 34076: // radio set only 1 item 
                                                        byte a = 0;
                                                        if (p.Inv.ContainsItem(34076,out a))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            p.Inv.AddItem(i);
                                                        }

                                                        break;

                                                   default:

                                                        p.Inv.AddItem(i);

                                                        break;

                                                }
                                                #endregion

                                            }

                                        } break;
#endregion

                                } break;

                            }break;
#endregion
                        #region warp
                        case ":warp":
                            {
                                try
                                {
                                    WarpData tmp = new WarpData();
                                    tmp.DstMap = ushort.Parse(words[1]);
                                    tmp.DstX_Axis = ushort.Parse(words[2]);
                                    tmp.DstY_Axis = ushort.Parse(words[3]);
                                    p.CurrentMap.Teleport(TeleportType.CmD, ref p, (byte)0, tmp);
                                }
                                catch { }
                            }break;
                        #endregion

                        #region CommandLine Develops
                        case "cmd":
                            switch (words[1])
                            {
                                case "1":
                                    {
                                       cGlobal.gGuildSystem.CreateNewGuild(ref p, "balaiada");
                                    }
                                    break;  
                              
                                case "2":
                                    p.CurGuild.GuilMail(p.UserID, p.UserID, "11");

                                    break;
                            }

                            break;
                        #endregion
                        #region Default

                        default :
                            SendPacket s = new SendPacket();
                            s.Pack(new byte[]{2,2});
                            s.Pack(p.ID);
                            s.PackNString(str);
                            p.CurrentMap.Broadcast(s, p.UserID);
                            break;
                        #endregion
                    }
                }
            }

                       
            catch (Exception t) { Utilities.LogServices.Log(t); }
        }
    }
}
