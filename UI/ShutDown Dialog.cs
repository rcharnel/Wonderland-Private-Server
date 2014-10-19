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

        public ShutDown_Dialog(bool update = false)
        {
            isupdating = update;
            InitializeComponent();
            timer1.Start();
        }

        private async void ShutDown_Dialog_Load(object sender, EventArgs e)
        {
            dispMsg = "Preparing to Shutdown";

            maxperct += cGlobal.WLO_World.Clients_Connected;
            maxperct += cGlobal.WLO_World.Clients_inGame;
            maxperct += cGlobal.ThreadManager.Count;

            Task shutdwn = new Task(new Action(() =>
            {
                int max = cGlobal.ThreadManager.Count;
                int cnt = 0;
                dispMsg = "Saving Settings";
                cGlobal.SrvSettings.SaveSettings(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.settings.wlo");

                dispMsg = "Stopping Tcp Listener";
                Network.ListenSocket.Kill();
                dispMsg = "Disconnecting and saving Player info Remaining..";

                dispMsg = "Shutting Down Server";
                cGlobal.WLO_World.Kill();
                perct += 10;
                foreach (var t in cGlobal.ThreadManager.ToList())
                {
                    dispMsg = string.Format("Aborting Threads {0}/{1}", ++cnt, max);
                    t.Abort();
                    Thread.Sleep(1000);
                    perct++;
                }
                perct = 100;
                if (isupdating)
                {
                    dispMsg = "Preparing for update";
                    dispMsg = "Launching Updater";
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = Environment.CurrentDirectory + "\\WloPSrvUpdater.exe";
                    start.Arguments = cGlobal.GClient.Branch+ " "+cGlobal.GClient.myVersion + " " +cGlobal.GClient.UpdateInfo.TagName;
                    Process.Start(start);                    

                }
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
