using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;
using Wlo.Core;

namespace Wonderland_Private_Server.Code.Objects
{//////
    public class Instance
    {
        Dictionary<int, CInstance> InstanceList;
        Dictionary<int, Data> InstanceData;
        int Tabs
        {
            get
            {
                if (InstanceList.Count <= 5) return 1;
                else if ((InstanceList.Count > 5) && (InstanceList.Count <= 10)) return 2;
                else if ((InstanceList.Count > 10) && (InstanceList.Count <= 15)) return 3;
                else if ((InstanceList.Count > 15) && (InstanceList.Count <= 20)) return 4;
                else return 1;
            }
        }

        public Instance()
        {
            InstanceList = new Dictionary<int, CInstance>();
            InstanceData = new Dictionary<int, Data>();
            LoadData();
        }
        void LoadData()
        {
            Data d = new Data();
            d.IDGlobal = 61591;
            d.LevelRestrict = 10;
            d.Timer = 15;
            d.NumberPlayers = 6;
            d.Name = "yoyo family";
            InstanceData.Add(30012, d);

        }


        public int GetIndexTab(int a)
        {            
                    if (a <= 5) return 1;
                    else if ((a > 5) && (a <= 10)) return 2;
                    else if ((a > 10) && (a <= 15)) return 3;
                    else if ((a > 15) && (a <= 20)) return 4;
                    else return 5;         
        }
        int GetTab(int CurTab)
        {
            if (CurTab == 1)
                return 0;
            else if (CurTab == 2)
                return 5;
            else if (CurTab == 3)
                return 10;
            else if (CurTab == 4)
                return 15;
            else
                return 20;

        }
        public int GetNumberPerTab(int Tab,int number)
        {
            switch(Tab)
            {
                case 1: if (number > 5) { return 5; } else { return number; } break;
                case 2: if (number > 10) { return 5; } else { return number - 5; } break;
                case 3: if (number > 15) { return 5; } else { return number - 10; } break;
                case 4: if (number > 20) { return 5; } else { return number - 20; } break;           

            }
            return 1;// error
        }
            
        
        int VerifyExitInstanceInList(int id)
        {            
            var g = InstanceData[id];
            int tmp = g.IDGlobal;
            if (InstanceList.ContainsKey(tmp))
            {
            gg:
                tmp++;
                if (InstanceList.ContainsKey(tmp))
                {
                    goto gg;
                }
                else { return tmp; }
            }
            else
            {
                return tmp;
            }
        }

        public void CreaterInstance(ref Player src,int Id,string text)
        {
            
            CInstance ci = new CInstance();
            ci.ID = VerifyExitInstanceInList(Id);
            ci.Text = text;
            ci.Creater = src.UserID;
            ci.NameCreater = src.CharacterName;
            ci.ListPlayers.Add(src.UserID,src);

            InstanceList.Add(ci.ID, ci);

            src.CurInstance = ci.ID;

            
            SendPacket s = new SendPacket();
            s.Pack(new byte[]{85,8});
            int tab = GetIndexTab(InstanceList.Count);
            s.Pack((byte)tab);
            s.Pack((UInt16)ci.ID); // ID INSTANCIA
            s.Pack(src.CharacterName); // CHAR NAME
            s.Pack(ci.Text); // DESCRITION INSTANCE
            s.Pack(1);
            s.Pack(0); // count + NAME GUILD
            cGlobal.WLO_World.BroadcastTo(s); // Packet global

           s = new SendPacket();
            s.Pack(new byte[] { 85,5,1,1 });
            s.Pack((UInt16)ci.ID);
            s.Pack(1);
            s.Pack(ci.Text);
            s.Pack(src.UserID);            
            cGlobal.WLO_World.BroadcastTo(s, directTo : src.UserID);
           

            SendPacket sc = new SendPacket();
            sc.Pack(new byte[] { 85,2,0});            
            cGlobal.WLO_World.BroadcastTo(sc, directTo: src.UserID);
            
            
        }
        public void CheckMembers(ref Player src,byte Tab)
        {           
            //int skip = 0;

            if (InstanceList.ContainsKey(src.CurInstance))
            {
                SendPacket s = new SendPacket();
                s.Pack(new byte[] {85,6});
                s.Pack((byte)InstanceList[src.CurInstance].Tabs);//total Tabs
                s.Pack(Tab); // current tab                           
                s.Pack((byte)InstanceList[src.CurInstance].ListPlayers.Count);//TotalPlayers
                int tmp = GetNumberPerTab(Tab,InstanceList[src.CurInstance].ListPlayers.Count);
                s.Pack((byte)tmp); // number player per tab 5 max                

                //if (Tab == 1)
                //    skip = 0;
                //else if (Tab == 2)
                //    skip = 5;
                //else if (Tab == 3)
                //    skip = 10;
                //else if (Tab == 4)
                //    skip = 15;
                //else
                //    skip = 20;


                    var item = InstanceList[src.CurInstance].ListPlayers.Skip(GetTab(Tab)).Take(5).ToList();
                    for (int a = 0; a < item.Count; a++)
                    {
                        s.Pack(item[a].Value.UserID);
                    }
               
                src.Send(s);
            }
        }
        
        public void Send81_1(ref Player src,int TabResquest) // UPDATE LIST INSTANCE !!!
        {
            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 85,1});
            s.Pack((byte)Tabs); // total Tabs
            s.Pack((byte)TabResquest); // Tab request (current tab)
           
            if (InstanceList.Count > 0)
            {
                s.Pack((byte)InstanceList.Count);//total instances

                var item = InstanceList.Skip(GetTab(TabResquest)).Take(5).ToList();

                for (int a = 0; a < item.Count; a++)
                {
                    s.Pack((UInt16)item[a].Value.ID);
                    s.Pack(item[a].Value.NameCreater);
                    s.Pack(item[a].Value.Text);
                    s.Pack(1);
                    s.Pack(0);
                }
                //foreach (var pair in InstanceList.Take(5))
                //{
                //    s.Pack((UInt16)pair.Value.ID);
                //    s.Pack(pair.Value.NameCreater);
                //    s.Pack(pair.Value.Text);
                //    s.Pack(1);
                //    s.Pack(0);
                //}                
            }
            else {s.Pack(0); }
            src.Send(s);

            s = new SendPacket();
            s.Pack(new byte[] { 85,13,0,0});           
            src.Send(s);
        }

        public void ExitInstancia(ref Player src)
        {
            int CurInstantance = src.CurInstance;
            uint srcID = src.UserID;

            if (InstanceList.ContainsKey(CurInstantance))
            {
                //check here if exist + members
                int check = InstanceList[CurInstantance].ListPlayers.Count - 1;
                if (check <= 0){ check = 0;}

                SendPacket s = new SendPacket();
                s.Pack(new byte[] { 85, 12 });
                s.Pack((byte)check); // numero de pessoas que ficaram
                s.Pack(srcID); // id de quem saiu
                s.Pack((UInt16)CurInstantance);
                if (InstanceList[CurInstantance].Creater == srcID)
                {
                    InstanceList[CurInstantance].Creater = 0;
                    s.Pack(1);
                }// 0 membro // 1 criador 
  
                else s.Pack(0); // 0 membro // 1 criador

                cGlobal.WLO_World.BroadcastTo(s); //global packet

                
                if (check == 0) // remove instance not players
                {
                    InstanceList.Remove(CurInstantance);

                    s = new SendPacket();
                    s.Pack(new byte[] { 85, 11, 2 });
                    s.Pack((UInt16)CurInstantance);
                    cGlobal.WLO_World.BroadcastTo(s); //global packet                    
                }
               

                s = new SendPacket();
                s.Pack(new byte[] { 85, 5, 2 });
                src.Send(s);
            }
            // se existe a instancia então remova o player e deixe os outros.
            if (InstanceList.ContainsKey(CurInstantance))
            {
                InstanceList[CurInstantance].RemoveMember(src.UserID);
            }
            //src.CurInstance = 0;

        }
        public void PreJoin(uint src,int id)
        {
            if (InstanceList.ContainsKey(id))
            {
                var tmp = InstanceList[id];

                SendPacket s = new SendPacket();
                s.Pack(new byte[] {85,4});
                s.Pack((UInt16)tmp.ID);
                s.Pack((byte)tmp.ListPlayers.Count);
                foreach (var pair in tmp.ListPlayers)
                {
                    s.Pack(pair.Value.CharacterName);
                    s.Pack(pair.Value.UserID);
                }
                cGlobal.WLO_World.BroadcastTo(s, directTo: src);

            }

        }
        public void Join(ref Player src,int id)
        {
            var tmp = InstanceList[id];

            SendPacket s = new SendPacket();
            s.Pack(new byte[] { 85,7});            
            s.Pack(1); // teste
            s.Pack((byte)(tmp.CountPlayer + 1)); // new player count
            s.Pack(src.UserID);
            s.Pack((UInt16)tmp.ID);
            cGlobal.WLO_World.BroadcastTo(s); // here global packet

            InstanceList[tmp.ID].ListPlayers.Add(src.UserID,src); //add player
            src.CurInstance = tmp.ID; // add id instance to player.

            s = new SendPacket();
            s.Pack(new byte[]{85, 5});
            s.Pack(1);//test
            s.Pack(1);//test
            s.Pack((UInt16)tmp.ID);
            s.Pack((byte)tmp.CountPlayer);
            s.Pack(tmp.Text); // text = null = 0;
            s.Pack(tmp.Creater); // id creater instancia
            cGlobal.WLO_World.BroadcastTo(s, directTo: src.UserID);

            s = new SendPacket();
            s.Pack(new byte[] { 85,3,0 });
            src.Send(s);

        }

        //Demiss player
        public void Demiss(ref Player src,uint member)
        {
            int CurInstance = src.CurInstance;
            if (InstanceList.ContainsKey(CurInstance))
            {
                InstanceList[CurInstance].RemoveMember(member);
                SendPacket s = new SendPacket();
                s.Pack(new byte[] { 85, 12 });
                s.Pack((byte)(InstanceList[CurInstance].CountPlayer - 1)); // numero de pessoas que ficaram
                s.Pack(member); // id de quem saiu
                s.Pack((UInt16)CurInstance);
                s.Pack(0); // 0 membro // 1 criador
                cGlobal.WLO_World.BroadcastTo(s); //global packet

                s = new SendPacket();
                s.Pack(new byte[] { 85,5,4 });
                cGlobal.WLO_World.BroadcastTo(s, directTo: member);
            }         

        }
    } 

    class CInstance
    {        
        public string Text; // descrição
        public uint Creater; // ID Quem criou
        public string NameCreater; // nome de quem criou
        public  int TimerElapsed; // tempo decorrido
        public int ID; // id desta instancia Key dictionary
        public int CountPlayer
        {
            get
            {
                if (ListPlayers.Count != null) return ListPlayers.Count;
                else return 0;

            }
        }
        public int Tabs
        {
            get
            {
                if (CountPlayer <= 5) return 1;
                 else if((CountPlayer > 5)&&(CountPlayer <=10)) return 2;
                 else if ((CountPlayer > 10)&&(CountPlayer <=15)) return 3;
                 else if ((CountPlayer > 15)&&(CountPlayer <=20)) return 4;
                else return 1;
            }
        }
        public Dictionary<uint,Player> ListPlayers = new Dictionary<uint,Player>();

        public void RemoveMember(uint ID)
        {
            if (ListPlayers.ContainsKey(ID))
            {
                ListPlayers[ID].CurInstance = 0;
                ListPlayers.Remove(ID);
            }
            //var item = ListPlayers.Find(x => x.UserID == ID);
            //if (item != null)            
            //    ListPlayers.Remove(item);
        }
        
    }
    class Data
    {
        //public ushort IDIndex;
        public int IDGlobal;
        public string Name;
        public int NumberPlayers;
        public int Timer;
        public byte LevelRestrict;
        /// quests
    }
}
