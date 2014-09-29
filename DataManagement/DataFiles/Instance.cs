using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Network;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
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


        public int GetIndexTab(int value)
        {
            int a = 0;
            foreach(var pair in InstanceList)
            {
                a++;
                if (pair.Key == value)
                {
                    if (a <= 5) return 1;
                    else if ((a > 5) && (a <= 10)) return 2;
                    else if ((a > 10) && (a <= 15)) return 3;
                    else if ((a > 15) && (a <= 20)) return 4;
                    else return 5;
                }                
            }
            return 0;
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
            ci.ListPlayers.Add(src.UserID, src);

            InstanceList.Add(ci.ID, ci);
            src.CurInstance = ci.ID;

            
            SendPacket s = new SendPacket();
            s.PackArray(new byte[]{85,8});
            int tab = GetIndexTab(src.CurInstance);
            s.Pack8((byte)tab);
            s.Pack16((UInt16)ci.ID);
            s.PackString(src.CharacterName);
            s.PackString(ci.Text);
            s.Pack8(1);
            s.Pack8(0);
            cGlobal.WLO_World.BroadcastTo(s); // this global players

           s = new SendPacket();
            s.PackArray(new byte[] { 85,5,1,1 });
            s.Pack16((UInt16)ci.ID);
            s.Pack8(1);
            s.PackString(ci.Text);
            s.Pack32(src.UserID);            
            cGlobal.WLO_World.BroadcastTo(s, directTo : src.UserID);
           // src.Send(s);

            SendPacket sc = new SendPacket();
            sc.PackArray(new byte[] { 85,2,0});            
            cGlobal.WLO_World.BroadcastTo(sc, directTo: src.UserID);
            //src.Send(s);
            
        }
        public void CheckMembers(ref Player src,byte CurTab)
        {
            if (InstanceList.ContainsKey(src.CurInstance))
            {
                SendPacket s = new SendPacket();
                s.PackArray(new byte[] {85,6});
                s.Pack8(CurTab); // test
                s.Pack8((byte)InstanceList[src.CurInstance].Tabs);//total                
                s.Pack8((byte)InstanceList[src.CurInstance].ListPlayers.Count);//current players comfirmado
                s.Pack8((byte)InstanceList[src.CurInstance].ListPlayers.Count);
                foreach (var pair in InstanceList[src.CurInstance].ListPlayers)
                {
                    s.Pack32(pair.Key);
                }
                src.Send(s);
            }
        }
        
        public void Send81_1(ref Player src) // UPDATE LIST INSTANCE !!!
        {
            SendPacket s = new SendPacket();
            s.PackArray(new byte[] { 85,1});
            s.Pack8((byte)Tabs); // number Tab
            s.Pack8(1); // unknow
           
            if (InstanceList.Count > 0)
            {
                s.Pack8((byte)InstanceList.Count);//number instances
                foreach (var pair in InstanceList)
                {
                    s.Pack16((UInt16)pair.Value.ID);
                    s.PackString(pair.Value.NameCreater);
                    s.PackString(pair.Value.Text);
                    s.Pack8(1);
                    s.Pack8(0);
                }                
            }
            else {s.Pack8(0); }
            src.Send(s);

            s = new SendPacket();
            s.PackArray(new byte[] { 85,13,0,0});           
            src.Send(s);
        }

        public void ExitInstancia(ref Player src)
        {
            if (InstanceList.ContainsKey(src.CurInstance))
            {
                //check here if exist + members
                int check = InstanceList[src.CurInstance].ListPlayers.Count - 1;
                if (check <= 0){ check = 0;}

                SendPacket s = new SendPacket();
                s.PackArray(new byte[] { 85, 12 });
                s.Pack8((byte)check);
                s.Pack32(src.UserID);
                s.Pack16((UInt16)src.CurInstance);
                s.Pack8(1);
                cGlobal.WLO_World.BroadcastTo(s); //global packet


                if (check == 0) // remove instance
                {
                    InstanceList.Remove(src.CurInstance);

                    s = new SendPacket();
                    s.PackArray(new byte[] { 85, 11, 2 });
                    s.Pack16((UInt16)src.CurInstance);
                    cGlobal.WLO_World.BroadcastTo(s); //global packet                    
                }

                s = new SendPacket();
                s.PackArray(new byte[] { 85, 5, 2 });
                src.Send(s);
            }

            src.CurInstance = 0; // player exit instance.

        }
        
        ////if windows open, cliente auto send 85,2 to update list
        //public void UPList(ref Player src)
        //{
        //    SendPacket s = new SendPacket();
        //    s.PackArray(new byte[] { 85,1,1,1,0 });
        //    src.Send(s);
        //    s = new SendPacket();
        //    s.PackArray(new byte[] {85,13,0,0});
        //    src.Send(s);

        //}
    } 

    class CInstance
    {        
        public string Text; // descrição
        public uint Creater; // Quem criou
        public string NameCreater; // nome de quem criou
        public  int TimerElapsed; // tempo decorrido
        public int ID; // id desta instancia
        public int CountPlayer; // numero de jogadores        
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
        public Dictionary<uint, Player> ListPlayers = new Dictionary<uint, Player>();

        public void RemoveMember(uint ID)
        {
            if (ListPlayers.ContainsKey(ID))
            {
                ListPlayers.Remove(ID);
            }

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
