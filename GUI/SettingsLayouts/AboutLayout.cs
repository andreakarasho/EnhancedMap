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

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class AboutLayout : UserControl
    {
        public AboutLayout()
        {
            InitializeComponent();

            labelVersion.Text = "Version: " + MainCore.MapVersion;

            linkLabelServUO.LinkClicked += (sender, e) => { Process.Start("https://www.servuo.com/archive/enhancedmap.692/"); };
            linkLabelOfficialSite.LinkClicked += (sender, e) => { Process.Start("http://razorenhanced.org/"); };
            //linkLabel1.LinkClicked += (sender, e) => { Process.Start("http://razorenhanced.org/"); };
            linkLabel2.LinkClicked += (sender, e) => { Process.Start("https://discordapp.com/invite/P3Q7mKT"); };
        }

    }
}
