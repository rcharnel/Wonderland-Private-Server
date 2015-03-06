using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;
using DataFiles;
using Game;
using Wlo.Core;

namespace Server.Network.WAC
{
    public class AC02:WLOAC
    {
        public override int ID { get { return 2; } }

        public override void Process(Player p, SendPacket r)
        {
            switch (r.Unpack8())
            {
                // case 1: Recv1(ref r, p); break;
                case 2: Recv2(p, r); break;
                case 3: Recv_3(p, r); break;
                default: base.Process(p,r); break;
            }
        }

        void Recv1(ref Player p, SendPacket r)
        {
        }
        void Recv2(Player p, SendPacket r)
        {

            string str = r.UnpackStringN();
            try
            {
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
                                                InvItem i = new InvItem();
                                                i.CopyFrom(new Item((PhxItemInfo)cGlobal.DatFile["item"].GetObject(itemid)));
                                                i.Ammt = ammt;

                                                #region filter item
                                                switch (itemid)
                                                {
                                                    case 34076: // radio set only 1 item 
                                                        byte a = 0;
                                                        if (p.Inv.ContainsItem(34076, out a))
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

                            } break;
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
                                    p.CurMap.Teleport(TeleportType.CmD, p, (byte)0, tmp);
                                }
                                catch { }
                            } break;
                        #endregion
                        #region riceball
                        #endregion
                        #region pets

                        #endregion

                        #region CommandLine Develops
                        //case "cmd":
                        //    switch (words[1])
                        //    {
                        //        case "1":
                        //            {
                        //                cGlobal.gGuildSystem.CreateNewGuild(ref p, "balaiada");
                        //            }
                        //            break;

                        //        case "2":
                        //            p.CurGuild.GuilMail(p.UserID, p.UserID, "11");

                        //            break;
                        //    }

                        //    break;
                        #endregion
                        #region Default

                        default:
                            SendPacket s = new SendPacket();
                            s.Pack8(2);
                            s.Pack8(2);
                            s.Pack32(p.CharID);
                            s.PackStringN(str);
                            s.SetHeader();
                            p.CurMap.Broadcast(s, "Ex", p.CharID);
                            break;
                        #endregion
                    }
                }
            }
            catch
            {
                SendPacket s = new SendPacket();
                s.Pack8(2);
                s.Pack8(2);
                s.Pack32(p.CharID);
                s.PackStringN(str);
                s.SetHeader();
                p.CurMap.Broadcast(s, "Ex", p.CharID);
            }

        }

        public void Recv_3(Player p, SendPacket r) //whisper
        {
            uint targetID = r.Unpack32();
            string text = r.UnpackStringN();

            SendPacket to = new SendPacket();
            to.Pack8(2);
            to.Pack8(3);
            to.Pack32(p.CharID);
            to.PackStringN(text);
            to.SetHeader();
            cGlobal.GameServer.Broadcast(to, "To", targetID,p.CharID);
        }
    }
}
