using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Octokit;

namespace Wonderland_Private_Server.Utilities.Update
{
    public partial class GitUpdateItem : UserControl
    {
        public GitUpdateItem(Version curver,Release gitrelease)
        {
            InitializeComponent();


            textBox1.Text = gitrelease.Body;
            label1.Text = string.Format("{0} - {1}", gitrelease.TagName, (gitrelease.PublishedAt != null)?gitrelease.PublishedAt.Value.UtcDateTime.ToShortDateString():"TBA");

            if(gitrelease.Prerelease)
            {
                pictureBox1.Visible = false;
                button1.Visible = false;
            }
            else if (curver < new Version((gitrelease.TagName.Split('.').Length <4)?gitrelease.TagName + ".0":gitrelease.TagName))
            {
                pictureBox1.Visible = false;
            }
            else if (curver > new Version((gitrelease.TagName.Split('.').Length < 4) ? gitrelease.TagName + ".0" : gitrelease.TagName))
            {
                pictureBox1.Visible = false;
                button1.Visible = false;
            }
            else if (curver == new Version((gitrelease.TagName.Split('.').Length < 4) ? gitrelease.TagName + ".0" : gitrelease.TagName))
            {
                button1.Visible = false;
            }
        }

        private void GitUpdateItem_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not coded yet!!!");
        }
    }
}
