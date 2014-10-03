using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Code.Objects
{
    public class Guild
    {



        public void CreateGuild(ref Player src)
        {

            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 39, 1, 0 });
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] {26,2});
            s.Pack32(1000); // id guild ?
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] { 39, 2 });
            s.PackString("GuildTesters");
            s.PackArray(new byte[] {000, 005, 026, 094, 038, 000,
                009, 067, 104, 101, 114, 114, 121, 066, 111, 111, 045, 
                003, 001, 002, 004, 003, 067, 163, 129, 026, 164, 194, 
                125, 026, 011, 067, 111, 109, 101, 032, 111, 110, 032,
                046, 046, 046, 000, 000, 000, 000, 001, 001, 001, 001, 
                001, 000, 000, 000, 000, 000, 000, 034, 137, 133, 148, 
                137, 104, 228, 064, 034, 137, 133, 148, 169, 103, 228, 064, 000 });            
            //testguild 009, 084, 101, 115, 116, 071, 117, 105, 108, 100
            // ??
            //name do char 009, 067, 104, 101, 114, 114, 121, 066, 111, 111

            //nick name 011, 067, 111, 109, 101, 032, 111, 110, 032,046, 046, 046,
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] {39,17});            
            src.Send(s);

            for (int a = 1; a < 8; a++)
            {
                s = new SendPacket();
                s.PackArray(new byte[] {39, 21});
                s.Pack8((byte)a);
                s.Pack32(0);
                src.Send(s);
            }
            for (int a = 1; a < 5; a++)
            {
                s = new SendPacket();
                s.PackArray(new byte[] { 39, 26 });
                s.Pack8((byte)a);
                s.Pack32(0);
                src.Send(s);
            }
            for (int a = 1; a < 8; a++)
            {
                s = new SendPacket();
                s.PackArray(new byte[] {39, 27});
                s.Pack8((byte)a);
                s.Pack32(0);
                src.Send(s);
            }

            // este pacote pareçe quem esta online ou menbro....
            //name char,
            //
            //nick name
            s = new SendPacket();
            s.PackArray(new byte[] { 039, 060, 026, 094, 038, 000, 009, 067, 104,
                101, 114, 114, 121, 066, 111, 111, 045, 003, 001, 002, 004, 003,
                067, 163, 129, 026, 164, 194, 125, 026, 011, 067, 111, 109, 101,
                032, 111, 110, 032, 046, 046, 046 });            
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] {039, 061, 026 ,094, 038, 000, 000});
            src.Send(s);

            //name da guild
            s = new SendPacket();
            s.PackArray(new byte[] { 039, 009, 026, 094, 038, 000, 074, 013, 000, 000, 009, 084, 101, 115, 116, 071, 117, 105, 108, 100 });
            src.Send(s);
            //unlock player dialogs
            s = new SendPacket();
            s.PackArray(new byte[] {20,8});
            src.Send(s);

          

        }
    }
}
