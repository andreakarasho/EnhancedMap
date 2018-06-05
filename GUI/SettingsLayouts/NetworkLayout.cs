using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnhancedMap.Core.Network;
using System.Xml;
using EnhancedMap.Core;
using EnhancedMap.Properties;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class NetworkLayout : UserControl
    {
        public NetworkLayout()
        {
            InitializeComponent();

            textboxName.MaxLength = textboxPassword.MaxLength = 16;    
        }


        public void LoadXML(XmlElement root)
        {       
            Global.SettingsCollection["username"] = this.textboxName.Text = root["network"].ToText("username", Global.SettingsCollection["username"].ToString());
            Global.SettingsCollection["password"] = this.textboxPassword.Text = root["network"].ToText("password", Global.SettingsCollection["password"].ToString());
            Global.SettingsCollection["remembercredentials"] = this.checkBoxRememberCredentials.Checked = root["network"].ToText("remembercredentials", Global.SettingsCollection["remembercredentials"].ToString()).ToBool();
            Global.SettingsCollection["ip"] = this.textboxIP.Text = root["network"].ToText("ip", Global.SettingsCollection["ip"].ToString());
            Global.SettingsCollection["port"] = this.textboxPort.Text = root["network"].ToText("port", Global.SettingsCollection["port"].ToString());
            Global.SettingsCollection["rememberserver"] = this.checkBoxRememberServer.Checked = root["network"].ToText("rememberserver", Global.SettingsCollection["rememberserver"].ToString()).ToBool();
            Global.SettingsCollection["autologin"] = this.checkboxAutologin.Checked = root["network"].ToText("autologin", Global.SettingsCollection["autologin"].ToString()).ToBool();
            Global.SettingsCollection["tryreconnect"] = this.checkBoxTryReconnect.Checked = root["network"].ToText("tryreconnect", Global.SettingsCollection["tryreconnect"].ToString()).ToBool();
            Global.SettingsCollection["tryreconnecttime"] = this.numericUpDownTryReconnect.Value = root["network"].ToText("tryreconnecttime", Global.SettingsCollection["tryreconnecttime"].ToString()).ToInt();


            AssignEvents();


           /* List<Global.Profile> profiles = (List<Global.Profile>)Global.SettingsCollection["profiles"];
            foreach (XmlElement profileNode in root["network"]["profiles"])
            {               
                Global.Profile profile = new Global.Profile()
                {
                    Username = profileNode.ToText("username"),
                    Password = profileNode.ToText("password"),
                    IP = profileNode.ToText("ip"),
                    Port = profileNode.ToText("port").ToUShort()
                };

                if (profiles.FirstOrDefault(s => s.Username == profile.Username) == null)
                {
                    profiles.Add(profile);
                    comboBoxProfileName.Items.Add(profile);

                    if (profile.Username == Global.SettingsCollection["username"].ToString())
                        comboBoxProfileName.SelectedIndex = comboBoxProfileName.Items.Count - 1;
                }
            }*/

        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("network");

            writer.WriteAttributeString("username", this.textboxName.Text/*((Global.Profile)comboBoxProfileName.Items[comboBoxProfileName.SelectedIndex]).Username*/);
            writer.WriteAttributeString("password", this.checkBoxRememberCredentials.Checked ? this.textboxPassword.Text : string.Empty);
            writer.WriteAttributeString("remembercredentials", this.checkBoxRememberCredentials.Checked.ToString());
            writer.WriteAttributeString("ip", this.checkBoxRememberServer.Checked ? this.textboxIP.Text : string.Empty);
            writer.WriteAttributeString("port", this.textboxPort.Text);
            writer.WriteAttributeString("rememberserver", this.checkBoxRememberServer.Checked.ToString());
            writer.WriteAttributeString("autologin", this.checkboxAutologin.Checked.ToString());
            writer.WriteAttributeString("tryreconnect", this.checkBoxTryReconnect.Checked.ToString());
            writer.WriteAttributeString("tryreconnecttime", this.numericUpDownTryReconnect.Value.ToString());

           /* writer.WriteStartElement("profiles");
            List<Global.Profile> profiles = (List<Global.Profile>)Global.SettingsCollection["profiles"];
            foreach (Global.Profile p in profiles)
            {
                writer.WriteStartElement("profile");
                writer.WriteAttributeString("username", p.Username);
                writer.WriteAttributeString("password", p.Password);
                writer.WriteAttributeString("ip", p.IP);
                writer.WriteAttributeString("port", p.Port.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();*/

            writer.WriteEndElement();

            Global.SettingsCollection["username"] = this.textboxName.Text;
            Global.SettingsCollection["password"] = this.textboxPassword.Text;
            Global.SettingsCollection["remembercredentials"] = this.checkBoxRememberCredentials.Checked;
            Global.SettingsCollection["ip"] = this.textboxIP.Text;
            Global.SettingsCollection["port"] = this.textboxPort.Text;
            Global.SettingsCollection["rememberserver"] = this.checkBoxRememberServer.Checked;
            Global.SettingsCollection["autologin"] = this.checkboxAutologin.Checked;
            Global.SettingsCollection["tryreconnect"] = this.checkBoxTryReconnect.Checked;
            Global.SettingsCollection["tryreconnecttime"] = this.numericUpDownTryReconnect.Value;
        }

        public void LoadDefault()
        {
            this.textboxName.Text = Global.SettingsCollection["username"].ToString();
            this.textboxPassword.Text = Global.SettingsCollection["password"].ToString();
            this.checkBoxRememberCredentials.Checked = Global.SettingsCollection["remembercredentials"].ToBool();
            this.textboxIP.Text = Global.SettingsCollection["ip"].ToString();
            this.textboxPort.Text = Global.SettingsCollection["port"].ToString();
            this.checkBoxRememberServer.Checked = Global.SettingsCollection["rememberserver"].ToBool();
            this.checkboxAutologin.Checked = Global.SettingsCollection["autologin"].ToBool();
            this.checkBoxTryReconnect.Checked = Global.SettingsCollection["tryreconnect"].ToBool();
            this.numericUpDownTryReconnect.Value = Global.SettingsCollection["tryreconnecttime"].ToInt();

            AssignEvents();
        }

        private void AssignEvents()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = Global.SettingsCollection["tryreconnecttime"].ToInt();
            timer.Tick += (sender, e) => { if (!NetworkManager.SocketClient.IsConnected && Global.SettingsCollection["tryreconnect"].ToBool()) NetworkManager.Connect(); };


            SocketClient.Connected += (sender, e) =>
            {
                if (timer.Enabled)
                    timer.Stop();

                customButtonConnect.Do(s => s.Text = "Disconnect");
                customButtonConnect.Do(s => s.Image = Resources.right);
                textboxName.Do(s => s.Enabled = false);
                textboxPassword.Do(s => s.Enabled = false);
                textboxIP.Do(s => s.Enabled = false);
                textboxPort.Do(s => s.Enabled = false);
            };

            SocketClient.Disconnected += (sender, e) =>
            {
                if (!timer.Enabled && Global.SettingsCollection["tryreconnect"].ToBool())
                    timer.Stop();

                customButtonConnect.Do(s => s.Text = "Connect");
                customButtonConnect.Do(s => s.Image = Resources.wrong);
                textboxName.Do(s => s.Enabled = true);
                textboxPassword.Do(s => s.Enabled = true);
                textboxIP.Do(s => s.Enabled = true);
                textboxPort.Do(s => s.Enabled = true);
            };

            SocketClient.Waiting += (sender, e) =>
            {
                if (timer.Enabled)
                    timer.Stop();

                customButtonConnect.Do(s => s.Text = "Cancel");
                customButtonConnect.Do(s => s.Image = null);
                textboxName.Do(s => s.Enabled = false);
                textboxPassword.Do(s => s.Enabled = false);
                textboxIP.Do(s => s.Enabled = false);
                textboxPort.Do(s => s.Enabled = false);
            };


            customButtonConnect.Click += (sender, e) =>
            {
                if (NetworkManager.SocketClient.IsConnected || NetworkManager.SocketClient.Status == ConnectionStatus.Waiting)
                    NetworkManager.Disconnect(false);
                else
                {
                    // horrible fix. To do when first time you change settings
                    Global.SettingsCollection["username"] = this.textboxName.Text;
                    Global.SettingsCollection["password"] = this.textboxPassword.Text;
                    Global.SettingsCollection["ip"] = this.textboxIP.Text;
                    Global.SettingsCollection["port"] = this.textboxPort.Text;

                    NetworkManager.Connect();
                }
            };

            textboxName.TextChanged += (sender, e) => { Global.SettingsCollection["username"] = textboxName.Text; Global.PlayerInstance.SetName(textboxName.Text); };
            textboxIP.TextChanged += (sender, e) => { Global.SettingsCollection["ip"] = textboxIP.Text; };
            textboxPort.TextChanged += (sender, e) => { Global.SettingsCollection["port"] = textboxPort.Text; };
            textboxPort.KeyPress += (sender, e) => { if (!char.IsNumber(e.KeyChar) && (Keys)e.KeyChar != Keys.Back) e.Handled = true; };


            /*customFlatButtonAddProfile.Click += (sender, e) =>
            {
                List<Global.Profile> profiles = ((List<Global.Profile>)Global.SettingsCollection["profiles"]);

                string username = comboBoxProfileName.Text.Trim();
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please insert a valid profile name");
                    return;
                }

                Global.Profile profile = profiles.FirstOrDefault(s => s.Username == username);
                if (profile == null)
                {
                    profile = new Global.Profile()
                    {
                        Username = username,
                        Password = textboxPassword.Text,
                        IP = textboxIP.Text,
                        Port = textboxPort.Text.ToUShort(),
                    };
                    comboBoxProfileName.Items.Add(profile);
                    profiles.Add(profile);

                }
                else
                {
                    profile.Password = textboxPassword.Text;
                    profile.IP = textboxIP.Text;
                    profile.Port = textboxPort.Text.ToUShort();
                }

            };

            comboBoxProfileName.SelectedIndexChanged += (sender, e) =>
            {
                if (comboBoxProfileName.SelectedIndex < 0)
                    return;
                Global.Profile profile = (Global.Profile)comboBoxProfileName.SelectedItem;
                textboxPassword.Text = profile.Password;
                textboxIP.Text = profile.IP;
                textboxPort.Text = profile.Port.ToString();

                Global.SettingsCollection["username"] = profile.Username;
            };*/
        }
    }
}
