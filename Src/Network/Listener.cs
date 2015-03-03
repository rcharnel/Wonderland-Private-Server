using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RCLibrary.Core.Networking;

namespace Wonderland_Private_Server.Network
{
    public class TcpServer:ListenSocket
    {

        public override void ListenThread()
        {
            Wlo.Core.WloClient client;
            bool bKeepAlive = true;

            try
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(new IPEndPoint(0x00000000, 6414));
                m_Socket.Blocking = true;
                DebugSystem.Write("Server bound to port 6414 successfully.");
                m_Socket.Listen(40);
                DebugSystem.Write("Now listening for clients.");
                while (bKeepAlive)
                {
                    try
                    {
                        client = new Wlo.Core.WloClient(m_Socket.Accept());
                        client.SetBlock(false);
                        DebugSystem.Write(string.Format("New aLogin client [{0}] connected.", client.SockAddress()));
                        cGlobal.WLO_World.QueueaLoginClient(client);
                    }
                    catch (SocketException ex)
                    {
                        DebugSystem.Write(new ExceptionData(ex));
                        break;
                    }

                    lock (m_Lock)
                    {
                        bKeepAlive = m_bKeepAlive;
                    }
                }
            }
            catch (SocketException ex)
            {
                DebugSystem.Write(ex.Message);
            }

            bKeepAlive = false;
            m_Socket.Close();
            m_Socket = null;
        }
    }
}
