using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Network;
using Game;

namespace Server
{
    /// <summary>
    /// Class used to keep track of users connecting to server
    /// Will handle preventing ban users/ip  from joining as well
    /// </summary>
    public class LoginServer : RCLibrary.Core.Networking.TcpServer
    {
        private TcpListener listener;


        /// <summary>
        /// Event used to forward new player to the actual game loop
        /// </summary>
        public event EventHandler<Player> OnNewPlayer;


        List<LoginClient> ClientList;

        public LoginServer()
        {
            ClientList = new List<LoginClient>();
        }


        public int Count { get { return ClientList.Count; } }


        public override void ListenThread()
        {

            Client3 client;
            bool bKeepAlive = true;

            try
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(new IPEndPoint(0x00000000, 6414));
                m_Socket.Blocking = true;
                DebugSystem.Write("Server bound to port " + 6414 + " successfully.");
                m_Socket.Listen(40);
                DebugSystem.Write("Now listening for clients on Port " + 6414 + ".");
                while (bKeepAlive && m_ThreadHandle.ThreadState != ThreadState.AbortRequested)
                {
                    try
                    {
                        m_bKeepAlive = bKeepAlive;
                        client = new Client3(m_Socket.Accept());
                        client.SetBlock(false);
                        bool found = false;
                        DebugSystem.Write(string.Format("New client [{0}] connected from Port " + 6414 + ".", client.SockAddress()));

                        foreach (var c in ClientList)
                        {
                            if (c.SrcIP == client.IPAddr)
                            {
                                c.AddSock(client);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            LoginClient tmp = new LoginClient();
                            Player p;
                            if ((p = tmp.AddSock(client)) != null)
                                ClientList.Add(tmp);
                            else
                            {
                                client.Disconnect();
                                continue;
                            }
                            if (OnNewPlayer != null) OnNewPlayer(this, p);
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != (int)SocketError.WouldBlock)
                            DebugSystem.Write(new ExceptionData(ex));
                    }
                    catch (Exception e)
                    {
                        DebugSystem.Write(new ExceptionData(e));
                    }

                    lock (m_Lock)
                    {
                        bKeepAlive = m_bKeepAlive;
                    }
                    Thread.Sleep(1);
                }
            }
            catch (SocketException ex)
            {
                DebugSystem.Write(new ExceptionData(ex));
            }

            bKeepAlive = false;
            try
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
            }
            catch (Exception e)
            {
                DebugSystem.Write(new ExceptionData(e));
            }
            m_Socket = null;
        }

        public bool IsOnline(string name)
        {
            int cnt = 0;
            foreach (var c in ClientList)
                foreach (var p in c.Values)
                    if (p.UserAcc.UserName == name) cnt++;
            return !(cnt <= 1);

        }

        public bool IsOnline(uint userID)
        {
            int cnt = 0;
            foreach (var c in ClientList)
                foreach (var p in c.Values)
                    if (p.UserAcc.UserID == userID) cnt++;

            return !(cnt <= 1);
        }

        public void Disconnect(uint userID)
        {
            foreach (var c in ClientList)
                foreach (var p in c.Values)
                    if (p.UserAcc.UserID == userID) p.Disconnect();
        }
    }
}
