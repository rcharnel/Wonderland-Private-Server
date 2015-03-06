using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Core.Networking;
using System.Net;
using System.Threading;
using Phoenix.Core.Controls;
using Wlo.Core;
using Game;

namespace PhoenixGameServer.Server
{
    public struct Node
    {
        int ID;
        int PlayerCnt;
        bool alive;

        IPAddress addr;

        Queue<RCLibrary.Core.Networking.IPacket> IncomingData, OutgoingData;
        List<Player> PlayerList;

        public Node()
        {
            ID = 0;
            PlayerCnt = 0;
            alive = false;
            addr = new IPAddress(0);
            IncomingData = new Queue<RCLibrary.Core.Networking.IPacket>();
            OutgoingData = new Queue<RCLibrary.Core.Networking.IPacket>();
            PlayerList = new List<Player>();
        }
    }

    public class IncomingUDPPacket : RCLibrary.Core.Networking.IncomingPacket
    {
        int ID;
        IPEndPoint From;
        System.Diagnostics.Stopwatch _timer;

        bool isDead { get { return _timer.Elapsed > new TimeSpan(0, 1, 0) && !IsReady(); } }
    }

    public struct OutgoingUDPPacket
    {
        int ID;
        DateTime sent;
        bool delivered;
        IPEndPoint To;
        SendPacket SendPacket;

    }

    public class NodeSystem : IUpdatableControl
    {
        Thread _thrd;

        RCLibrary.Core.Networking.UDPServer _listener;
        RCLibrary.Core.Networking.Client3 m_socket;

        IPAddress _ip = new IPAddress(0);
        int _port;
        string _name, _statmsg,apikey;

        public string Branch { get { return _name; } set { _name = value; } }
        public string IP { get { return _ip.MapToIPv4().ToString(); } set { SetField(ref _ip, IPAddress.Parse(value)); } }
        public int Port { get { return _port; } set { SetField(ref _port, value); } }
        public string ApiKey{ get { return apikey; } set { SetField(ref apikey,value); } }
        public string StatMsg { get { return _statmsg; } set { SetField(ref _statmsg, value); } }

        List<Node> Nodes;
        

        public NodeSystem(int port)
        {
            Nodes = new List<Node>();
            _listener = new RCLibrary.Core.Networking.UDPServer(port);
            _listener.XOR = 186;
            _listener.onPacketRecved = Process;
            _listener.Start();
            _thrd = new Thread(new ThreadStart(Work));
            _thrd.Start();
        }

        ~NodeSystem()
        {
            _listener.Stop();
            if (_thrd != null)
            {
                _thrd.Join(6000);
                if (_thrd.IsAlive) _thrd.Abort();
            }
        }


        void Work()
        {
            do
            {

                #region HQ
                if (m_socket == null || m_socket.isDisconnected())
                {
                }
                #endregion

                #region Nodes
                #endregion

                Thread.Sleep(1);
            } while (true);
        }

        void Process(IPEndPoint src, RCLibrary.Core.Networking.IPacket pkt)
        {
            ushort returnaddr = (pkt as SendPacket).Unpack16();
            IPEndPoint trgt = new IPEndPoint(src.Address.MapToIPv4(), 26952);
            SendPacket p = pkt as SendPacket;
            p.m_nUnpackIndex = 4;
            if (p != null)
            {
                byte a = 0, b = 0;

                a = p.Unpack8();
                if (p.Buffer.Count() >= 6)
                    b = p.Unpack8();
                switch (a)
                {
                    //#region AC 255
                    //case 255:
                    //    {
                    //        switch (b)
                    //        {

                    //        }
                    //    }
                    //    break;
                    //#endregion
                    //#region 250
                    //case 250:
                    //    {
                    //        switch (b)
                    //        {
                    //            case 1: SendPacket(trgt, SendPacket.FromFormat("bbs", 251, 1, _name)); break;// branch name
                    //        }
                    //    }
                    //    break;
                    //    #endregion
                }
            }
        }



    }
}
