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
            s.Pack32(1000); //gold decrescimo
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] { 39, 2 });
            s.PackString("GuildTesters");
            s.PackArray(new byte[] {000, 005});
            s.Pack32(src.UserID);
            s.PackString(src.CharacterName);
            s.Pack8(src.Level);
            s.Pack8((byte)src.Body);
            s.Pack8(src.Head);           
            // elem
            s.PackArray(new byte[] { 002, 004, 003, 067, 163, 129, 026, 164, 194, 
                125, 026});
            s.PackString(src.Nickname);
            s.PackArray(new byte[] {000, 000, 000, 000, 001, 001, 001, 001, 
                001, 000, 000, 000, 000, 000, 000, 034, 137, 133, 148, 
                137, 104, 228, 064, 034, 137, 133, 148, 169, 103, 228, 064, 000 });            
           
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
            s.PackArray(new byte[] { 39, 60 });
            s.Pack32(src.UserID);
            s.PackString(src.CharacterName);
            s.Pack8(src.Level);
            s.Pack8((byte)src.Body);
            s.Pack8(src.Head);
            s.Pack8((byte)src.Element);
            //s.PackArray(new byte[] { 039, 060, 026, 094, 038, 000, 009, 067, 104,
            //    101, 114, 114, 121, 066, 111, 111, 045, 003, 001, 002, 004, 003,
            //    067, 163, 129, 026, 164, 194, 125, 026, 011, 067, 111, 109, 101,
            //    032, 111, 110, 032, 046, 046, 046 });            
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] {039, 061});
            s.Pack32(src.UserID);
            s.Pack8(0);
            src.Send(s);

            //name create guild
            s = new SendPacket();
            s.PackArray(new byte[] { 039, 009});
            s.Pack32(src.UserID);
            s.PackArray(new byte[] { 74, 13,0,0 });
            s.PackString("xxxxx"); // name guild NICK SHOW IN MAP (acima do nick name)
            src.Send(s);
            //unlock player dialogs
            s = new SendPacket();
            s.PackArray(new byte[] {20,8});
            src.Send(s);

          

        }
    }
}
