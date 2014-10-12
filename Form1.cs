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
        
        void GuiThread()
        {
            do
            {
                #region LogGUI
                if (Utilities.LogServices.LogHistory.Count > 0)
                {
                    Utilities.LogItem j;

                    if (Utilities.LogServices.LogHistory.TryDequeue(out j))
                    {
                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                switch (j.eventtype)
                                {
                                    case Utilities.LogType.SYS: SystemLog.AppendText(j.happenat.ToShortTimeString() + "|" + j.eventtype + "|" + j.message + "\r\n=============================\r\n"); break;
                                    case Utilities.LogType.NET: NetWorkLog.AppendText(j.happenat.ToShortTimeString() + "|" + j.eventtype + "|" + j.message + "r\n===========================\r\n"); break;
                                    case Utilities.LogType.ERR: errorLog.AppendText(j.happenat.ToShortTimeString() + "|" + j.where + "|" + j.message + "\r\n============================\r\n"); break;
                                }
                            }));
                        }
                        catch { }
                    }
                }
                #endregion
                Thread.Sleep(1);
            }
            while (true);
        }


        void MainThreadWork()
        {
            //load settings file
            cGlobal.SrvSettings.LoadSettings();

            //load Updater
            cGlobal.GClient.Initialize(ref cGlobal.ThreadManager);
            cGlobal.GClient.GitinfoUpdated += GClient_GitinfoUpdated;
            cGlobal.GClient.onError += GClient_onError;
            
            if (GitUptOption.SelectedIndex != (byte)Config.Update.UpdtControl)
                        GitUptOption.SelectedIndex = (byte)Config.Update.UpdtControl;
                    if (updtrefresh.Value != cGlobal.GClient.UpdtCheck)
                        updtrefresh.Value = cGlobal.GClient.UpdtCheck;
                    if (GitBranch.Text != cGlobal.GClient.Branch)
                        GitBranch.Text = cGlobal.GClient.Branch;

            #region Intialize Base Threads
            Utilities.LogServices.Log("Intializing Gui Thread");
            Thread tmp2 = new Thread(new ThreadStart(GuiThread));
            tmp2.Start();
            cGlobal.ThreadManager.Add(tmp2);

            #endregion

            #region intial check  if theres an update
            Thread.Sleep(2000);
            Utilities.LogServices.Log("Checking For Update...");
            if (cGlobal.GClient.UpdateFound && cGlobal.GClient.AutoUpdate)
            {
                if (cGlobal.GClient.AutoUpdate_At < DateTime.Now)
                {
                    updating = true;
                    run = false;
                    goto ShutDwn;
                }
            }
            #endregion

            #region Initialize Objects
            Utilities.LogServices.Log("Intializing Objs");
            cGlobal.WLO_World = new Network.WorldManager();

            cGlobal.gCharacterDataBase = new DataManagement.DataBase.CharacterDataBase();
            cGlobal.gEveManager = new DataManagement.DataFiles.EveManager();
            cGlobal.gGameDataBase = new DataManagement.DataBase.GameDataBase();
            cGlobal.gItemManager = new DataManagement.DataFiles.ItemManager();
            cGlobal.gSkillManager = new DataManagement.DataFiles.SkillDataFile();
            cGlobal.gUserDataBase = new DataManagement.DataBase.UserDataBase();
            cGlobal.gNpcManager = new DataManagement.DataFiles.NpcDat();
            

            #endregion

            #region DataBase Initialization
            Utilities.LogServices.Log("Verifying DataBase Authencation Setup");
            if(cGlobal.gDataBaseConnection.VerifyConnection())
                Utilities.LogServices.Log("Connection Successful");
            else
                Utilities.LogServices.Log("Connection not successful");

            Utilities.LogServices.Log("Verifying DataBase Tables");
            cGlobal.gUserDataBase.VerifySetup();
            cGlobal.gCharacterDataBase.VerifySetup();
            cGlobal.gGameDataBase.VerifySetup();


            #endregion

            #region Load Data Files
            cGlobal.gItemManager.LoadItems("Data\\Item.dat");
            cGlobal.gSkillManager.LoadSkills("Data\\Skill.dat");
            cGlobal.gEveManager.LoadFile("Data\\eve.Emg");
            cGlobal.gNpcManager.LoadNpc("Data\\Npc.dat");
            #endregion

            #region Initialize the Wonderland Server
            Utilities.LogServices.Log("Jump Starting Server...");
            cGlobal.WLO_World.Initialize();
            Network.ListenSocket.Initialize();
            #endregion

            var test = cGlobal.GetMap(60000);

            do
            {
                #region Update Section
                if (cGlobal.GClient.UpdateFound && Config.Update.UpdtControl == Config.UpdtSetting.Auto)
                {
                    if (cGlobal.GClient.AutoUpdate_At < DateTime.Now)
                    {
                        updating = true;
                        break;
                    }
                }
                #endregion
                #region Thread Management
                foreach (var t in cGlobal.ThreadManager.ToList())
                    if (!t.IsAlive)
                        cGlobal.ThreadManager.Remove(t);
                #endregion
                Thread.Sleep(1);
            }
            while (run);

            ShutDwn:

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

        #region Git Client Events
        void GClient_onError(object sender, Exception e)
        {
            Utilities.LogServices.Log(e);
        }

        void GClient_GitinfoUpdated(object sender, EventArgs e)
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    UpdatePane.Controls.Clear();
                    var list = cGlobal.GClient.ReleasesFnd;

                    //foreach (var y in list.OrderByDescending(c => new Version(c.TagName)))
                    //    UpdatePane.Controls.Add(new Utilities.Update.GitUpdateItem(cGlobal.GClient.myVersion, y));

                }));
            }
            catch { }

        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (run || blockclose)
                e.Cancel = true;
            run = false;
        }

        private void GitBranch_TextChanged(object sender, EventArgs e)
        {
            if (cGlobal.GClient != null && cGlobal.GClient.Branch != GitBranch.Text)
                cGlobal.GClient.Branch = GitBranch.Text;
        }

        private void updtrefresh_ValueChanged(object sender, EventArgs e)
        {
            if (cGlobal.GClient != null && cGlobal.GClient.UpdtCheck != updtrefresh.Value)
                cGlobal.GClient.UpdtCheck = (int)updtrefresh.Value;
        }

        private void GitUptOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Config.Update.UpdtControl != (Config.UpdtSetting)GitUptOption.SelectedIndex)
                Config.Update.UpdtControl = (Config.UpdtSetting)GitUptOption.SelectedIndex;
        }

    }
}
