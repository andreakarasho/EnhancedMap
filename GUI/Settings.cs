using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using EnhancedMap.Core;
using EnhancedMap.Diagnostic;
using EnhancedMap.GUI.SettingsLayouts;

namespace EnhancedMap.GUI
{
    public partial class Settings : CustomForm
    {
        private const string SETTINGS = "config.xml";

        private readonly AboutLayout _aboutLayout = new AboutLayout {Tag = 5};

        private readonly ApplicationLayout _applicationLayout = new ApplicationLayout {Tag = 2};

        private readonly DiagnosticLayout _diagnosticLayout = new DiagnosticLayout {Tag = 6};

        private readonly GeneralLayout _generalLayout = new GeneralLayout {Tag = 1};

        private readonly LabelsLayout _labelsLayout = new LabelsLayout {Tag = 4};

        private readonly NetworkLayout _networkLayout = new NetworkLayout {Tag = 0};

        private readonly ServerCommLayout _serverCommLayout = new ServerCommLayout {Tag = 3};


        public Settings()
        {
            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            MaximizeBox = false;
            MinimumSize = MaximumSize = Size;
            ForeColor = Color.White;

            customFlatButtonNetwork.Tag = 0;
            customFlatButtonGenerals.Tag = 1;
            customFlatButtonApplication.Tag = 2;
            customFlatButtonRemote.Tag = 3;
            customFlatButtonLabels.Tag = 4;
            customFlatButtonAbout.Tag = 5;
            customFlatButtonDiagnostic.Tag = 6;

            customFlatButtonNetwork.Click += OnButtonPressend;
            customFlatButtonGenerals.Click += OnButtonPressend;
            customFlatButtonApplication.Click += OnButtonPressend;
            customFlatButtonRemote.Click += OnButtonPressend;
            customFlatButtonLabels.Click += OnButtonPressend;
            customFlatButtonAbout.Click += OnButtonPressend;
            customFlatButtonDiagnostic.Click += OnButtonPressend;

            _networkLayout.Visible = true;
            _generalLayout.Visible = _applicationLayout.Visible = _serverCommLayout.Visible = _labelsLayout.Visible = _aboutLayout.Visible = _diagnosticLayout.Visible = false;
            panelSet.Controls.Add(_networkLayout);
            panelSet.Controls.Add(_generalLayout);
            panelSet.Controls.Add(_applicationLayout);
            panelSet.Controls.Add(_serverCommLayout);
            panelSet.Controls.Add(_labelsLayout);
            panelSet.Controls.Add(_aboutLayout);
            panelSet.Controls.Add(_diagnosticLayout);

            customFlatButtonNetwork.ForceHover = true;


            FormClosing += (sender, e) =>
            {
                e.Cancel = true;
                Hide();
            };
            customButtonOk.Click += (sender, e) =>
            {
                SaveSettings();
                Hide();
            };
            pictureBox1.Click += (sender, e) => { Process.Start("https://www.paypal.me/muskara/"); };

            LoadSettings();

            VisibleChanged += (sender, e) =>
            {
                if (Visible)
                {
                    _labelsLayout.BuildList();
                    _applicationLayout.ReloadClients();
                }
            };
        }

        public void LoadSettings()
        {
            Logger.Log("Load settings...");

            if (!File.Exists("settings.xml") && !File.Exists(SETTINGS))
            {
                Logger.Log("config.xml not exists");
                _generalLayout.LoadDefault();
                _applicationLayout.LoadDefault();
                _networkLayout.LoadDefault();
                _diagnosticLayout.LoadDefault();
                return;
            }

            try
            {
                XmlElement root = null;
                XmlDocument doc = new XmlDocument();

                try
                {
                    doc.Load(SETTINGS);
                    root = doc["data"];
                }
                catch
                {
                }


                if (root == null)
                {
                    Configuration.Load();

                    Global.SettingsCollection["ip"] = Configuration.MySettings.Ip;
                    Global.SettingsCollection["port"] = Configuration.MySettings.Port;
                    Global.SettingsCollection["username"] = Configuration.MySettings.Name;
                    Global.SettingsCollection["password"] = Configuration.MySettings.Password;
                    Global.SettingsCollection["autologin"] = Configuration.MySettings.Autologin;
                    Global.SettingsCollection["tryreconnect"] = Configuration.MySettings.TryToRelog;
                    Global.SettingsCollection["showhits"] = Configuration.MySettings.HpBar;
                    Global.SettingsCollection["showstamina"] = Configuration.MySettings.StamBar;
                    Global.SettingsCollection["showmana"] = Configuration.MySettings.ManaBar;
                    Global.SettingsCollection["centerplayer"] = Configuration.MySettings.AutoCenterOnPg;
                    Global.SettingsCollection["trackdeathpoint"] = Configuration.MySettings.TrackDeathPoint;
                    Global.SettingsCollection["abbreviatenames"] = Configuration.MySettings.AbbreviatePgName;
                    Global.SettingsCollection["smartnamesposition"] = Configuration.MySettings.SmartVisual;
                    Global.SettingsCollection["showplacesicons"] = Configuration.MySettings.ShowBuilds;
                    Global.SettingsCollection["hidelessimportantplaces"] = Configuration.MySettings.SmartIcons;
                    Global.SettingsCollection["showtownsnames"] = Configuration.MySettings.ShowTownName;
                    Global.SettingsCollection["showserverbounds"] = Configuration.MySettings.ShowServerBounds;
                    Global.SettingsCollection["showguardlines"] = Configuration.MySettings.Guardlines;
                    Global.SettingsCollection["showcoordinates"] = Configuration.MySettings.ShowCoords;
                    Global.SettingsCollection["showhouses"] = Configuration.MySettings.ShowHouses;
                    Global.SettingsCollection["namecolor"] = Configuration.MySettings.ChatColor.ToArgb();
                    Global.SettingsCollection["namefont"] = Configuration.MySettings.FontName;
                    Global.SettingsCollection["namesize"] = (int) Configuration.MySettings.FontSize;
                    Global.SettingsCollection["namestyle"] = Configuration.MySettings.FontStyle;


                    _generalLayout.LoadDefault();
                    _applicationLayout.LoadDefault();
                    _networkLayout.LoadDefault();
                    _diagnosticLayout.LoadDefault();
                }
                else
                {
                    _generalLayout.LoadXML(root);
                    _applicationLayout.LoadXML(root);
                    _networkLayout.LoadXML(root);
                    _labelsLayout.LoadXML(root);
                    _diagnosticLayout.LoadXML(root);

                    LoadOtherData(root);
                }


                Logger.Log("Loaded.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while trying to load settings.\r\n" + e);
            }
        }

        public void SaveSettings()
        {
            Logger.Log("Saving settings...");

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};

                using (XmlWriter xml = XmlWriter.Create(SETTINGS, settings))
                {
                    xml.WriteStartDocument(true);
                    xml.WriteStartElement("data");

                    _generalLayout.SaveXML(xml);
                    _applicationLayout.SaveXML(xml);
                    _networkLayout.SaveXML(xml);
                    _labelsLayout.SaveXML(xml);
                    _diagnosticLayout.SaveXML(xml);

                    SaveOtherData(xml);

                    xml.WriteEndElement();

                    Logger.Log("Saved.");
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while trying to save settings.\r\n" + e.InnerException);
            }
        }

        private void OnButtonPressend(object sender, EventArgs e)
        {
            if (sender is CustomFlatButton flatButton)
            {
                foreach (Control c in panel1.Controls)
                {
                    if (c is CustomFlatButton flat && flat.ForceHover)
                        flat.ForceHover = false;
                }

                foreach (Control c in panelSet.Controls)
                {
                    if ((int) c.Tag == (int) flatButton.Tag)
                    {
                        flatButton.ForceHover = true;
                        if (!c.Visible)
                            c.Visible = true;
                        else
                            return;
                    }
                    else
                        c.Visible = false;
                }
            }
        }


        private void LoadOtherData(XmlElement root)
        {
            XmlElement highlist = root["highlightes"];
            if (highlist != null)
            {
                foreach (XmlElement high in highlist)
                {
                    string name = high.ToText("username");
                    Global.HighlightesUsername.Add(name);
                }
            }
        }

        private void SaveOtherData(XmlWriter writer)
        {
            writer.WriteStartElement("highlightes");
            foreach (string name in Global.HighlightesUsername)
            {
                writer.WriteStartElement("highlight");
                writer.WriteAttributeString("username", name);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}