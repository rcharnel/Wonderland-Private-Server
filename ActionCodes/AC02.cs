using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

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
                        #region item add
                        case ":item":                            
                            {
                                switch (words[1])
                                {
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

                                } break;

                            }
#endregion

                        #region CommandLine Develops
                        case "cmd":
                            switch (words[1])
                            {
                                case "1":
                                    {
                                        Guild g = new Guild();
                                        //g.CreateGuild(ref p);
                                    }
                                    break;
                            }

                            break;
                        #endregion
                        #region Default

                        default :
                            SendPacket s = new SendPacket();
                            s.PackArray(new byte[]{2,2});
                            s.Pack32(p.ID);
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
