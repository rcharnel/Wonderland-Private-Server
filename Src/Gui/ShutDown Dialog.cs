using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Wonderland_Private_Server.UI
{
    public partial class ShutDown_Dialog : Form
    {
        bool isupdating;
        ManualResetEvent block = new ManualResetEvent(false);
        string dispMsg = "";
        int perct = 0,maxperct = 100;

        public ShutDown_Dialog()
        {
            InitializeComponent();
            timer1.Start();
        }

        private async void ShutDown_Dialog_Load(object sender, EventArgs e)
        {
            dispMsg = "Preparing to Shutdown";

            //maxperct += cGlobal.WLO_WorldClients_Connected;
            //maxperct += cGlobal.WLO_World.Clients_inGame;


            Task shutdwn = new Task(new Action(() =>
            {
                int cnt = 0;
                dispMsg = "Saving Settings";
                cGlobal.SrvSettings.SaveSettings(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.settings.wlo");

                dispMsg = "Stopping Tcp Listener";
                cGlobal.gLoginServer.Kill();
                dispMsg = "Disconnecting and saving Player info Remaining..";

                dispMsg = "Shutting Down Server";
                cGlobal.gWorld.Kill();
                perct += 10;
                //foreach (var t in cGlobal.ThreadManager.Values.ToList())
                //{
                //    dispMsg = string.Format("Aborting Threads {0}/{1}", ++cnt, max);
                //    t.Abort();
                //    Thread.Sleep(1000);
                //    perct++;
                //}
                perct = 100;
                dispMsg = "Server Shutdown Completed..";
                Thread.Sleep(1500);

                this.Invoke(new Action(() => { Close(); }));
            }));
             shutdwn.Start();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = dispMsg;
            progressBar1.Maximum = maxperct;
            progressBar1.Value = perct;
        }

        private void ShutDown_Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }
    }
}
