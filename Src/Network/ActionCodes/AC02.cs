using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Network;

namespace Network.ActionCodes
{
    public class AC02 : AC
    {
        public override int ID { get { return 2; } }
        public override void ProcessPkt( Player r, RecievePacket p)
        {
            switch (p.Unpack8())
            {
                // case 1: Recv1(ref r, p); break;
                case 2: Recv2(r, p); break;
            }
        }
        void Recv1(Player p, RecievePacket r)
        {
            
        }
        void Recv2(Player p, RecievePacket r)
        {
            try
            {
                string str = r.UnpackStringN();
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
                                                //InvItemCell i = new InvItemCell();
                                                //i.CopyFrom(cGlobal.gItemManager.GetItem(itemid));
                                                //i.Ammt = ammt;

                                                #region filter item
                                                switch (itemid)
                                                {
                                                   case 34076: // radio set only 1 item 
                                                       {
                                                           if (!p.Inv.ContainsItem(34076) && !p.Eqs.IsEquipped(34076))
                                                               p.Inv.AddItem(itemid, ammt);
                                                        }
                                                        break;

                                                   default:p.Inv.AddItem(itemid,ammt);break;

                                                }
                                                #endregion

                                            }

                                        } break;
#endregion
 
                                } break;

                            }break;
#endregion
                        #region warp
                        //case ":warp":
                        //    {
                        //        try
                        //        {
                        //            WarpData tmp = new WarpData();
                        //            tmp.DstMap = ushort.Parse(words[1]);
                        //            tmp.DstX_Axis = ushort.Parse(words[2]);
                        //            tmp.DstY_Axis = ushort.Parse(words[3]);
                        //            p.CurrentMap.Teleport(TeleportType.CmD, ref p, (byte)0, tmp);
                        //        }
                        //        catch { }
                        //    }break;
                        #endregion

                        #region CommandLine Develops
                        //case "cmd":
                        //    switch (words[1])
                        //    {
                        //        case "1":
                        //            {
                        //               cGlobal.gGuildSystem.CreateNewGuild(ref p, "balaiada");
                        //            }
                        //            break;  
                              
                        //        case "2":
                        //            p.CurGuild.GuilMail(p.UserID, p.UserID, "11");

                        //            break;
                        //    }

                        //    break;
                        #endregion
                        #region Default

                        default :
                            {
                                RCLibrary.Core.Networking.PacketBuilder tmp = new RCLibrary.Core.Networking.PacketBuilder();
                                tmp.Begin();
                                tmp.Add((byte)2);
                                tmp.Add((byte)2);
                                tmp.Add(p.CharID);
                                tmp.Add(str,true);

                                p.CurMap.Broadcast(new SendPacket(tmp.End()), "Ex", p.CharID);
                            }
                            break;
                        #endregion
                    }
                }
            }

                       
            catch (Exception t) { log.Error(t.Message,t); }
        }
    }
}
