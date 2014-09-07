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

namespace Wonderland_Private_Server
{
    public partial class Form1 : Form
    {
        bool run = true; bool blockclose = true; bool updating;
        
       


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread MainThread = new Thread(new ThreadStart(MainThreadWork));
            MainThread.IsBackground = true;
            MainThread.Start();
        }


        void MainThreadWork()
        {

            //load Updater
            cGlobal.GClient.Initialize(ref cGlobal.ThreadManager);

            //intial check  if theres an update

            Thread.Sleep(2000);
            do
            {
                if (cGlobal.GClient.UpdateFound && cGlobal.GClient.AutoUpdate)
                {
                    if (cGlobal.GClient.AutoUpdate_At < DateTime.Now)
                    {
                        updating = true;
                        break;
                    }
                }

                Thread.Sleep(1);
            }
            while (run);

            this.Invoke(new Action(() => { this.Enabled = false; }));

            UI.ShutDown_Dialog tmp = new UI.ShutDown_Dialog(updating);
            tmp.Location = this.Location;
            tmp.Left = this.Left + 100;
            tmp.Top = this.Top + 250;
            
            tmp.ShowDialog();
            tmp.Dispose();

            run = false;
            blockclose = false;
            this.Invoke(new Action(() => { Close(); }));

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (run || blockclose)
                e.Cancel = true;
            run = false;
        }

    }
}
