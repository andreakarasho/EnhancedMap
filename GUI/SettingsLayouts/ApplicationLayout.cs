using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using EnhancedMap.Core;
using System.Threading;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class ApplicationLayout : UserControl
    {
        struct ClientInfo
        {
            public IntPtr handle;
            public string windowText;

            public override string ToString()
            {
                return windowText;
            }
        }

        private bool _checkSelected;

        public ApplicationLayout()
        {
            InitializeComponent();
        }

        public void ReloadClients()
        {
            comboBoxClientNum.Items.Clear();
            _checkSelected = false;

            var clients = UOClientManager.GetClientsWindowTitles();
            foreach (KeyValuePair<IntPtr, string> c in clients)
            {
                comboBoxClientNum.Items.Add(new ClientInfo() { windowText = c.Value, handle = c.Key });
                if (UOClientManager.hWnd == c.Key)
                    comboBoxClientNum.SelectedIndex = comboBoxClientNum.Items.Count - 1;
            }

            _checkSelected = true;
        }

        public void LoadXML(XmlElement root)
        {
            int clienttype = 0;
            Global.SettingsCollection["clienttype"] = clienttype = root["application"].ToText("clienttype", Global.SettingsCollection["clienttype"].ToString()).ToInt();
            if (clienttype == 0)
                this.radioButtonClassic.Checked = true;
            else if (clienttype == 1)
                radioButtonEnhanced.Checked = true;
            else if (clienttype == 2)
                radioButtonOrion.Checked = true;

            Global.SettingsCollection["showscrollbarls"] = this.checkBoxShowScrollbars.Checked = root["application"].ToText("showscrollbarls", Global.SettingsCollection["showscrollbarls"].ToString()).ToBool();
            Global.SettingsCollection["openchatatstart"] = this.checkBoxOpenChatAppStart.Checked = root["application"].ToText("openchatatstart", Global.SettingsCollection["openchatatstart"].ToString()).ToBool();
            Global.SettingsCollection["showincomingmsg"] = this.checkBoxShowChatMsgIn.Checked = root["application"].ToText("showincomingmsg", Global.SettingsCollection["showincomingmsg"].ToString()).ToBool();
            Global.SettingsCollection["soundsincomingmsg"] = this.checkBoxSoundsMsgIn.Checked = root["application"].ToText("soundsincomingmsg", Global.SettingsCollection["soundsincomingmsg"].ToString()).ToBool();
            Global.SettingsCollection["followuowindowstate"] = this.checkBoxFollowUOstate.Checked = root["application"].ToText("followuowindowstate", Global.SettingsCollection["followuowindowstate"].ToString()).ToBool();
            Global.SettingsCollection["loadmapsondemand"] = this.checkBoxLoadMapsOnDemand.Checked = root["application"].ToText("loadmapsondemand", Global.SettingsCollection["loadmapsondemand"].ToString()).ToBool();
            Global.SettingsCollection["showfps"] = this.checkBoxShowFPS.Checked = root["application"].ToText("showfps", Global.SettingsCollection["showfps"].ToString()).ToBool();
            Global.SettingsCollection["fps"] = this.numericUpDownFPS.Value = root["application"].ToText("fps", Global.SettingsCollection["fps"].ToString()).ToInt();
            Global.SettingsCollection["mapX"] = root["application"].ToText("mapX", Global.SettingsCollection["mapX"].ToString()).ToInt();
            Global.SettingsCollection["mapY"] = root["application"].ToText("mapY", Global.SettingsCollection["mapY"].ToString()).ToInt();
            Global.SettingsCollection["mapW"] = root["application"].ToText("mapW", Global.SettingsCollection["mapW"].ToString()).ToInt();
            Global.SettingsCollection["mapH"] = root["application"].ToText("mapH", Global.SettingsCollection["mapH"].ToString()).ToInt();
            Global.SettingsCollection["chatX"] = root["application"].ToText("chatX", Global.SettingsCollection["chatX"].ToString()).ToInt();
            Global.SettingsCollection["chatY"] = root["application"].ToText("chatY", Global.SettingsCollection["chatY"].ToString()).ToInt();         
            Global.SettingsCollection["chatW"] = root["application"].ToText("chatW", Global.SettingsCollection["chatW"].ToString()).ToInt();
            Global.SettingsCollection["chatH"] = root["application"].ToText("chatH", Global.SettingsCollection["chatH"].ToString()).ToInt();
            Global.SettingsCollection["zoomIndex"] = root["application"].ToText("zoomIndex", Global.SettingsCollection["zoomIndex"].ToString()).ToInt();

            int t = root["application"].ToText("mapkind", Global.SettingsCollection["mapkind"].ToString()).ToInt();
            Global.SettingsCollection["mapkind"] = t;

            if (t == 0)
            {
                this.radioButtonDetailed.Checked = true;
            }
            else
            {
                this.radioButtonNormal.Checked = true;
            }

            Global.SettingsCollection["clientpath"] = Global.UOPath = this.textBoxClientPath.Text = root["application"].ToText("clientpath",  "");
            label3.Visible = textBoxClientPath.Visible = customButtonClientPath.Visible = radioButtonEnhanced.Checked;

            Global.SettingsCollection["chatfontsize"] = this.numericUpDownChatFontSize.Value = root["application"].ToText("chatfontsize", Global.SettingsCollection["chatfontsize"].ToString()).ToInt();

            AssignEvents();
        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("application");

            if (radioButtonClassic.Checked)
            {
                writer.WriteAttributeString("clienttype", "0");
                Global.SettingsCollection["clienttype"] = 0;
            }
            else if (radioButtonEnhanced.Checked)
            {
                writer.WriteAttributeString("clienttype", "1");
                Global.SettingsCollection["clienttype"] = 1;
            }
            else if (radioButtonOrion.Checked)
            {
                writer.WriteAttributeString("clienttype", "2");
                Global.SettingsCollection["clienttype"] = 2;
            }
            writer.WriteAttributeString("showscrollbarls", this.checkBoxShowScrollbars.Checked.ToString());
            writer.WriteAttributeString("openchatatstart", this.checkBoxOpenChatAppStart.Checked.ToString());
            writer.WriteAttributeString("showincomingmsg", this.checkBoxShowChatMsgIn.Checked.ToString());
            writer.WriteAttributeString("soundsincomingmsg", this.checkBoxSoundsMsgIn.Checked.ToString());
            writer.WriteAttributeString("followuowindowstate", this.checkBoxFollowUOstate.Checked.ToString());
            writer.WriteAttributeString("loadmapsondemand", this.checkBoxLoadMapsOnDemand.Checked.ToString());
            writer.WriteAttributeString("showfps", this.checkBoxShowFPS.Checked.ToString());
            writer.WriteAttributeString("fps", this.numericUpDownFPS.Value.ToString());

            if (this.radioButtonDetailed.Checked)
            {
                Global.SettingsCollection["mapkind"] = 0;
                writer.WriteAttributeString("mapkind", "0");
            }
            else if (this.radioButtonNormal.Checked)
            {
                Global.SettingsCollection["mapkind"] = 1;
                writer.WriteAttributeString("mapkind", "1");
            }
            writer.WriteAttributeString("clientpath", this.textBoxClientPath.Text);

            writer.WriteAttributeString("mapX", Global.MainWindow.Location.X.ToString());
            writer.WriteAttributeString("mapY", Global.MainWindow.Location.Y.ToString());
            writer.WriteAttributeString("mapW", Global.MainWindow.Size.Width.ToString());
            writer.WriteAttributeString("mapH", Global.MainWindow.Size.Height.ToString());
            writer.WriteAttributeString("chatX", Global.MainWindow.ChatWindow?.Location.X.ToString() ?? "0");
            writer.WriteAttributeString("chatY", Global.MainWindow.ChatWindow?.Location.Y.ToString() ?? "0");      
            writer.WriteAttributeString("chatW", Global.MainWindow.ChatWindow?.Size.Width.ToString() ?? "250");
            writer.WriteAttributeString("chatH", Global.MainWindow.ChatWindow?.Size.Height.ToString() ?? "250");

            writer.WriteAttributeString("zoomIndex", Global.SetInitialZoom(Global.Zoom).ToString());

            writer.WriteAttributeString("chatfontsize", this.numericUpDownChatFontSize.Value.ToString());

            writer.WriteEndElement();

            Global.SettingsCollection["showscrollbarls"] = this.checkBoxShowScrollbars.Checked;
            Global.SettingsCollection["openchatatstart"] = this.checkBoxOpenChatAppStart.Checked;
            Global.SettingsCollection["showincomingmsg"] = this.checkBoxShowChatMsgIn.Checked;
            Global.SettingsCollection["soundsincomingmsg"] = this.checkBoxSoundsMsgIn.Checked;
            Global.SettingsCollection["followuowindowstate"] = this.checkBoxFollowUOstate.Checked;
            Global.SettingsCollection["loadmapsondemand"] = this.checkBoxLoadMapsOnDemand.Checked;
            Global.SettingsCollection["showfps"] = this.checkBoxShowFPS.Checked;
            Global.SettingsCollection["fps"] = this.numericUpDownFPS.Value;
            Global.SettingsCollection["clientpath"] = Global.UOPath = this.textBoxClientPath.Text;
            Global.SettingsCollection["mapX"] = Global.MainWindow.Location.X;
            Global.SettingsCollection["mapY"] = Global.MainWindow.Location.Y;
            Global.SettingsCollection["mapW"] = Global.MainWindow.Size.Width;
            Global.SettingsCollection["mapH"] = Global.MainWindow.Size.Height;
            Global.SettingsCollection["chatX"] = Global.MainWindow.ChatWindow?.Location.X.ToString() ?? "0";
            Global.SettingsCollection["chatY"] = Global.MainWindow.ChatWindow?.Location.Y.ToString() ?? "0";
            Global.SettingsCollection["chatW"] = Global.MainWindow.ChatWindow?.Size.Width.ToString() ?? "250";
            Global.SettingsCollection["chatH"] = Global.MainWindow.ChatWindow?.Size.Height.ToString() ?? "250";
            Global.SettingsCollection["zoomIndex"] = Global.SetInitialZoom(Global.Zoom);

            Global.SettingsCollection["chatfontsize"] = (int)this.numericUpDownChatFontSize.Value;
        }

        public void LoadDefault()
        {
            int t = Global.SettingsCollection["clienttype"].ToInt();
            if (t == 0)
                radioButtonClassic.Checked = true;
            else if (t == 1)
                radioButtonEnhanced.Checked = true;
            else
                radioButtonOrion.Checked = true;

            this.checkBoxShowScrollbars.Checked = Global.SettingsCollection["showscrollbarls"].ToBool();
            this.checkBoxOpenChatAppStart.Checked = Global.SettingsCollection["openchatatstart"].ToBool();
            this.checkBoxShowChatMsgIn.Checked = Global.SettingsCollection["showincomingmsg"].ToBool();
            this.checkBoxSoundsMsgIn.Checked = Global.SettingsCollection["soundsincomingmsg"].ToBool();
            this.checkBoxFollowUOstate.Checked = Global.SettingsCollection["followuowindowstate"].ToBool();
            this.checkBoxLoadMapsOnDemand.Checked = Global.SettingsCollection["loadmapsondemand"].ToBool();
            this.checkBoxShowFPS.Checked = Global.SettingsCollection["showfps"].ToBool();
            this.numericUpDownFPS.Value = Global.SettingsCollection["fps"].ToInt();

            t = Global.SettingsCollection["mapkind"].ToInt();
            if (t == 0)
                this.radioButtonDetailed.Checked = true;
            else
                this.radioButtonNormal.Checked = true;

            Global.SettingsCollection["clientpath"] = Global.UOPath = "";

            label3.Visible = textBoxClientPath.Visible = customButtonClientPath.Visible = radioButtonEnhanced.Checked;

            this.numericUpDownChatFontSize.Value = Global.SettingsCollection["chatfontsize"].ToInt();

            AssignEvents();
        }

        private void AssignEvents()
        {
            comboBoxClientNum.SelectedIndexChanged += (sender, e) =>
            {
                if (!_checkSelected)
                    return;

                if (comboBoxClientNum.Items.Count > 0 && comboBoxClientNum.SelectedIndex >= 0)
                {
                    ClientInfo client = (ClientInfo)comboBoxClientNum.Items[comboBoxClientNum.SelectedIndex];
                    UOClientManager.AttachToClient(client.handle);
                }
            };

            customButtonRefreshClients.Click += (sender, e) => { ReloadClients(); };


            this.customButtonRebuildMaps.Click += (sender, e) =>
            {
                if (MessageBox.Show("All maps will be lost.\r\n\r\nAre you sure to continue?", "Attention!", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;

                this.ParentForm.Hide();
                MapsManager.RemoveMapsFiles();
                Global.MainWindow.RebuildMaps(true);
                Global.MainWindow.Show();
            };

            this.numericUpDownFPS.ValueChanged += (sender, e) => { Global.SettingsCollection["fps"] = numericUpDownFPS.Value; Global.MainWindow.SetTimerIntervalByFps((int)numericUpDownFPS.Value); };

            this.radioButtonDetailed.CheckedChanged += (sender, e) => { if (this.radioButtonDetailed.Checked) { Global.SettingsCollection["mapkind"] = 0; Global.Facet = Global.Facet; } };
            this.radioButtonNormal.CheckedChanged += (sender, e) => { if (this.radioButtonNormal.Checked) { Global.SettingsCollection["mapkind"] = 1; Global.Facet = Global.Facet; } };
            this.customFlatButtonCheckNewVersion.Click += (sender, e) => { Core.Network.Update.CheckUpdates(); };

            this.radioButtonEnhanced.CheckedChanged += (sender, e) =>
            {
                label3.Visible = textBoxClientPath.Visible = customButtonClientPath.Visible = radioButtonEnhanced.Checked;
            };

            this.customButtonClientPath.Click += (sender, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog
                {
                    Description = "Select enhanced client folder"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Global.SettingsCollection["clientpath"] = Global.UOPath = textBoxClientPath.Text = dialog.SelectedPath;
                }
            };

            this.numericUpDownChatFontSize.ValueChanged += (sender, e) =>
            {
                Global.SettingsCollection["chatfontsize"] = (int)this.numericUpDownChatFontSize.Value;
                Global.MainWindow.ChatWindow?.ChangeFontSize();
            };
        }
    }
}
