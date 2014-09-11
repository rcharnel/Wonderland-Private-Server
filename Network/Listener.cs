using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Wonderland_Private_Server.Network
{
    public class ListenSocket
    {
        private static Socket m_Socket;
        private static bool m_bKeepAlive = false;
        private static Thread m_ThreadHandle = null;
        private static readonly object m_Lock = new object();

        public static void Initialize()
        {
            m_bKeepAlive = true;
            m_ThreadHandle = new Thread(new ThreadStart(ListenThread));
            m_ThreadHandle.Name = "Listerner Thread";
            m_ThreadHandle.Start();
            cGlobal.ThreadManager.Add(m_ThreadHandle);
        }

        private static void ListenThread()
        {
            Socket client;
            bool bKeepAlive = true;

            try
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(new IPEndPoint(0x00000000, 6414));
                Utilities.LogServices.Log("Server bound to port 6414 successfully.");
                m_Socket.Listen(40);
                m_Socket.Blocking = false;
                Utilities.LogServices.Log("Now listening for clients.");
                while (bKeepAlive)
                {
                    try
                    {
                        client = m_Socket.Accept();
                        Utilities.LogServices.Log(string.Format("New aLogin client [{0}] connected.", client.RemoteEndPoint.ToString()));
                        cGlobal.WLO_World.QueueaLoginClient(client);                       
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.WouldBlock)
                        {
                            m_Socket.Close();
                           Utilities.LogServices.Log(ex.Message);
                        }
                    }

                    lock (m_Lock)
                    {
                        bKeepAlive = m_bKeepAlive;
                    }

                    Thread.Sleep(20);
                }

            }
            catch (SocketException ex)
            {
                Utilities.LogServices.Log(ex.Message);
            }

            bKeepAlive = false;
            m_Socket.Close();
            m_Socket = null;
        }

        public static bool IsListening()
        {
            return m_bKeepAlive;
        }

        public static void Kill()
        {
            bool bJoin = false;
            lock (m_Lock)
            {
                if (m_bKeepAlive)
                {
                    m_bKeepAlive = false;
                    bJoin = true;
                }
            }

            if (bJoin)
            {
                while (m_ThreadHandle.IsAlive) { Thread.Sleep(1000); }
                m_ThreadHandle = null;
            }
        }
    }
}
