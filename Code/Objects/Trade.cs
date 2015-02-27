using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.Code.Objects
{
    public class TradeManager
    {
        public bool FinializedTrade;
        Player Owner;
        Player Trading_with { get; set; }
        List<byte> Items_Trading;
        List<cItem> partner_Items;

        uint GoldTradingTo_Them;
        uint GoldTradingTo_Me;

        public TradeManager(Player owner)
        {
            Owner = owner;
        }

        public void TradeWith(Player player)
        {
            //Owner.CharacterTemplateState = CharacterTemplate.PlayerState.Trading;
            //Trading_with = player;
            //SendPacket opentrade = new SendPacket();
            //opentrade.Header(25, 1);
            //opentrade.AddDWord(player.CharacterTemplateID);
            //g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), opentrade);
        }

        public void CancelTrade()
        {
            //if (Owner.CharacterTemplateState == CharacterTemplate.PlayerState.Trading)
            //{
            //    Owner.CharacterTemplateState = CharacterTemplate.PlayerState.inMap;
            //    SendPacket closetrade = new SendPacket();
            //    closetrade.Header(25, 2);
            //    closetrade.Pack(3);
            //    g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), closetrade);
            //    Trading_with.character.cTrader.CancelTrade();
            //    Trading_with = null;
            //}
        }

        public void onConfirmTrade(RecvPacket d)
        {
            //int ptr = 2;
            //My_GoldTrading = d.GetDWord(ptr); ptr += 4;
            //My_Items_Trading = new List<byte>();
            //while (ptr < d.Size)
            //{
            //    My_Items_Trading.Add(d.GetByte(ptr));
            //    ptr++;
            //}

            //List<cInvItem> tmp = new List<cInvItem>();
            //foreach (byte a in My_Items_Trading)
            //    tmp.Add(Owner.MyInventory.GetInventoryItem(a));

            //Trading_with.character.cTrader.Partner_confirmedTrade(My_GoldTrading, tmp);
        }

        public void onPartner_ConfirmedTrade(uint Gold, List<cItem> Items)
        {
            //partner_Items = Items;
            //partner_GoldTrading = Gold;
            //SendPacket Tradecfm = new SendPacket();
            //Tradecfm.Header(25, 3);
            //Tradecfm.AddDWord(Gold);

            //foreach (cInvItem h in Items)
            //{
            //    Tradecfm.AddWord(h.ID);
            //    Tradecfm.Pack(h.ammt);
            //    Tradecfm.Pack(h.damage);
            //    for (int a = 0; a < 24; a++)
            //        Tradecfm.Pack(0);
            //}
            //Tradecfm.SetSize();
            //g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), Tradecfm);
        }

        public void TradeComplete()
        {

            //Trading_with = null;
            //foreach (byte r in My_Items_Trading)
            //{
            //    Owner.MyInventory.RemoveItem(r);
            //}
            //foreach (cInvItem r in partner_Items)
            //{
            //    Owner.MyInventory.RecieveItem(r, true, r.ammt);
            //}
            //if (My_GoldTrading > 0)
            //{
            //    SendPacket closetrade = new SendPacket();
            //    closetrade.Header(26, 2);
            //    closetrade.AddDWord(My_GoldTrading);
            //    closetrade.SetSize();
            //    g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), closetrade);
            //}
            //if (partner_GoldTrading > 0)
            //{
            //    SendPacket closetrade = new SendPacket();
            //    closetrade.Header(26, 1);
            //    closetrade.AddDWord(partner_GoldTrading);
            //    closetrade.SetSize();
            //    g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), closetrade);
            //}

            //SendPacket endtrade = new SendPacket();
            //endtrade.Header(25, 2);
            //endtrade.Pack(4);
            //endtrade.SetSize();
            //g.SendPacket(g.FindPlayerby_CharacterTemplateID(Owner.CharacterTemplateID), endtrade);
        }

        public void TradeFinalized()
        {

            //FinializedTrade = true;

            //if (FinializedTrade && Trading_with.character.cTrader.FinializedTrade)
            //{
            //    Owner.CharacterTemplateState = CharacterTemplate.PlayerState.inMap;
            //    Trading_with.character.CharacterTemplateState = CharacterTemplate.PlayerState.inMap;
            //    TradeComplete();
            //    Trading_with.character.cTrader.TradeComplete();
            //}
        }
    }
}
