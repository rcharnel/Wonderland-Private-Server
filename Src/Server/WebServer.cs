using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Wonderland_Private_Server.Web
{
    public class IISServer
    {
        private TcpListener myListener;
        private int port = 9580;  // Select any free port you wish 

        //The constructor which make the TcpListener start listening on th
        //given port. It also calls a Thread on the method StartListen(). 
        public IISServer()
        {
        }

        public bool Run()
        {
            try
            {
                //start listing on the given port
                myListener = new TcpListener(IPAddress.Parse("127.0.0.1"),port);
                myListener.Start();
                Console.WriteLine("Web Server Running... Press ^C to Stop...");

                //start the thread which calls the method 'StartListen'
                //Thread th = new Thread(new ThreadStart(StartListen));
                //th.Init();
                //cGlobal.ThreadManager.Add(th);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred while Listening :"
                                   + e.ToString());
            }
            return false;
        }
        public void Stop()
        {

        }

        public string GetTheDefaultFileName(string sLocalDirectory)
        {
            StreamReader sr;
            String sLine = "";

            try
            {
                //Open the default.dat to find out the list
                // of default file
                sr = new StreamReader("data\\Default.Dat");

                while ((sLine = sr.ReadLine()) != null)
                {
                    //Look for the default file in the web server root folder
                    if (File.Exists(sLocalDirectory + sLine) == true)
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (File.Exists(sLocalDirectory + sLine) == true)
                return sLine;
            else
                return "";
        }

    }
   
}
