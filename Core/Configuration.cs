using EnhancedMap.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using FontStyle = System.Drawing.FontStyle;

namespace EnhancedMap.Core
{
    // only to port v1 to v2 map

    public static class Configuration
    {
        public static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings.xml");
        public static readonly string PathBackup = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings.bck");

        public static Settings MySettings { get; set; }

        public static void Load()
        {
            MySettings = new Settings();

            /*if (!File.Exists(Path))
            {
                MySettings.MakeDefault();
                Save();
            }
            */
            var doc = new XmlDocument();

            try
            {
                doc.Load(Path);
                Console.WriteLine("Settings loaded.");
            }
            catch (Exception)
            {
                try
                {
                    doc.Load(PathBackup);
                }
                catch (Exception)
                {
                    //fai nuova form come messagebox
                    Task.Run(() => MessageBox.Show("Impossible to open Settings.backup"));
                    return;
                }
                Task.Run(() => MessageBox.Show("Settings.xml was corrupted. Opened Settings.backup"));
            }

            var root = doc["settings"];
            MySettings.Load(root);
        }

        public static void Save()
        {
            /*using (var op = new StreamWriter(Path))
            {
                var xml = new XmlTextWriter(op)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                xml.WriteStartDocument(true);
                xml.WriteStartElement("settings");

                MySettings.Save(xml);

                xml.WriteEndElement();
                xml.Close();
            }

            File.Copy(Path, PathBackup, true);
            Console.WriteLine("Settings saved.");*/
        }

        public class Settings
        {
            private string _name;
            private int _fontStyle, _opacity;
            private float _zoomValue, _fontSize;

           // private RefreshType _refreshType;

            public int MapX { get; set; }
            public int MapY { get; set; }
            public int MapW { get; set; }
            public int MapH { get; set; }
            public int Port { get; set; }
            public int ChatX { get; set; }
            public int ChatY { get; set; }
            public int ChatW { get; set; }
            public int ChatH { get; set; }

           /* public RefreshType RefreshType
            {
                get { return _refreshType; }
                set { _refreshType = value < RefreshType.RealTime || value > RefreshType.Slow ? RefreshType.High : value; }
            }*/

            public int FontStyle
            {
                get { return _fontStyle; }
                set { _fontStyle = Enum.IsDefined(typeof(FontStyle), value) ? value : 0; }
            }

            public int Opacity
            {
                get { return _opacity; }
                set { _opacity = value <= 0 || value > 10 ? 10 : value; }
            }

            public string Ip { get; set; }

            public string Name
            {
                get
                {
                    if (string.IsNullOrEmpty(_name))
                        _name = "You";
                    return _name;
                }
                set
                {
                    _name = value;
                    if (string.IsNullOrEmpty(_name))
                        _name = "You";
                }
            }

            public string Password { get; set; }

            public string EmVersion
            {
                get { return MainCore.MapVersion.ToString(); }
            }

            public string ClientVersion { get; set; }
            public string ClientDir { get; set; }
            public string CommanderName { get; set; }
            public string FontName { get; set; }
            public bool Autologin { get; set; }
            public bool ShowCoords { get; set; }
            public bool ShowPgName { get; set; }
            public bool Guardlines { get; set; }
            public bool HpBar { get; set; }
            public bool PanicAdvice { get; set; }
            public bool TrackDeathPoint { get; set; }
            public bool ShowBuilds { get; set; }
            public bool TiltMap { get; set; }
            public bool OpenChatIfMsg { get; set; }
            public bool OpenChat { get; set; }
            public bool AbbreviatePgName { get; set; }
            public bool StamBar { get; set; }
            public bool ManaBar { get; set; }
            public bool Statics { get; set; }
            public bool ShowTownName { get; set; }
            public bool TryToRelog { get; set; }
            public bool SmartVisual { get; set; }
            public bool SmartIcons { get; set; }
            public bool ClearCache { get; set; }
            public bool AutoCenterOnPg { get; set; }
            public bool ShowHouses { get; set; }
            public bool ShowServerBounds { get; set; }
            public bool LogHouses { get; set; }
            public bool PointInfo { get; set; }
            public bool ShowScrollBars { get; set; }
            public bool PanicSquare { get; set; }
            public bool AlertSquare { get; set; }
            public bool AttachOnUo { get; set; }
            public Color ChatColor { get; set; }

            //public MapType MapType { get; set; }

            public float ZoomValue
            {
                get { return _zoomValue; }
                set { _zoomValue = value <= 0 || value > 8 ? 1.5f : value; }
            }

            public float FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value <= 0 ? 12 : value; }
            }

            public Dictionary<string, Macro> Macros { get; } = new Dictionary<string, Macro>();
            public Dictionary<string, List<string>> Labels { get; } = new Dictionary<string, List<string>>();

            public void MakeDefault()
            {
                TrackDeathPoint = ShowBuilds = PanicAdvice =
                OpenChatIfMsg = ShowPgName = HpBar = Guardlines =
                StamBar = ManaBar = Statics = ShowTownName =
                TryToRelog = SmartIcons = AutoCenterOnPg = ShowHouses =
                ShowServerBounds = LogHouses = PointInfo = ShowScrollBars =
                TiltMap = PanicSquare = AlertSquare = true;

                OpenChat = ClearCache = Autologin = ShowCoords = AbbreviatePgName = SmartVisual = AttachOnUo = false;

                _name = "You";
                _opacity = 10;
                FontName = "Segoe UI";
                ChatColor = Color.White;
                //_refreshType = RefreshType.High;
                _fontSize = 12f;
                MapW = MapH = ChatW = ChatH = 300;

                //foreach (var file in LabelsManager.Labels.Where(s => s.Labels.Count > 0))
                //    file.Enabled = true;

                Console.WriteLine("Settings default created.");
            }

            public void Save(XmlWriter xml)
            {
                WriteElement(xml, "MapX", MapX);
                WriteElement(xml, "MapY", MapY);
                WriteElement(xml, "MapW", MapW);
                WriteElement(xml, "MapH", MapH);
                WriteElement(xml, "Port", Port);
                WriteElement(xml, "ChatX", ChatX);
                WriteElement(xml, "ChatY", ChatY);
                WriteElement(xml, "ChatW", ChatW);
                WriteElement(xml, "ChatH", ChatH);
              //  WriteElement(xml, "RefreshType", _refreshType);
                WriteElement(xml, "FontStyle", _fontStyle);
                WriteElement(xml, "Opacity", _opacity);
                WriteElement(xml, "Ip", Ip);
                WriteElement(xml, "Name", _name);
                WriteElement(xml, "Password", Password);
                WriteElement(xml, "EMVersion", EmVersion);
                WriteElement(xml, "ClientVersion", ClientVersion);
                WriteElement(xml, "ClientDir", ClientDir);
                WriteElement(xml, "CommanderName", CommanderName);
                WriteElement(xml, "FontName", FontName);
                WriteElement(xml, "Autologin", Autologin);
                WriteElement(xml, "ShowCoords", ShowCoords);
                WriteElement(xml, "ShowPgName", ShowPgName);
                WriteElement(xml, "Guardlines", Guardlines);
                WriteElement(xml, "PanicAdvice", PanicAdvice);
                WriteElement(xml, "TrackDeathPoint", TrackDeathPoint);
                WriteElement(xml, "ShowBuilds", ShowBuilds);
                WriteElement(xml, "TiltMap", TiltMap);
                WriteElement(xml, "OpenChatIfMsg", OpenChatIfMsg);
                WriteElement(xml, "OpenChat", OpenChat);
                WriteElement(xml, "AbbreviatePgName", AbbreviatePgName);
                WriteElement(xml, "HpBar", HpBar);
                WriteElement(xml, "StamBar", StamBar);
                WriteElement(xml, "ManaBar", ManaBar);
                WriteElement(xml, "Statics", Statics);
                WriteElement(xml, "ShowTownName", ShowTownName);
                WriteElement(xml, "TryToRelog", TryToRelog);
                WriteElement(xml, "SmartVisual", SmartVisual);
                WriteElement(xml, "SmartIcons", SmartIcons);
                WriteElement(xml, "ClearCache", ClearCache);
                WriteElement(xml, "AutoCenterOnPg", AutoCenterOnPg);
                WriteElement(xml, "ShowHouses", ShowHouses);
                WriteElement(xml, "ShowServerBounds", ShowServerBounds);
                WriteElement(xml, "LogHouses", LogHouses);
                WriteElement(xml, "PointInfo", PointInfo);
                WriteElement(xml, "ShowScrollBars", ShowScrollBars);
                WriteElement(xml, "ChatColor", ChatColor);
                WriteElement(xml, "ZoomValue", _zoomValue);
                WriteElement(xml, "FontSize", _fontSize);
                WriteElement(xml, "PanicSquare", PanicSquare);
                WriteElement(xml, "AlertSquare", AlertSquare);
                WriteElement(xml, "AttachOnUO", AttachOnUo);

                xml.WriteStartElement("macros");
                foreach (var m in Macros)
                {
                    xml.WriteStartElement("macro");
                    xml.WriteAttributeString("label", m.Key);
                    xml.WriteAttributeString("name1", m.Value.MacroName1);
                    xml.WriteAttributeString("name2", m.Value.MacroName2);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();

               /* xml.WriteStartElement("labelsfile");
                foreach (var file in LabelsManager.Labels)
                {
                    if (file.Labels.Count <= 0)
                        file.Enabled = false;

                    xml.WriteStartElement("file");
                    xml.WriteAttributeString("name", file.Name);
                    xml.WriteAttributeString("enabled", file.Enabled.ToString());
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();*/


              /*  WriteElement(xml, "MapType", MapType);

                xml.WriteStartElement("maps");
                foreach (MapBase map in MapBase.Maps)
                {
                    xml.WriteStartElement("map");
                    xml.WriteAttributeString("name", map.Name);
                    xml.WriteAttributeString("index", map.Index.ToString());
                    xml.WriteAttributeString("width", map.Width.ToString());
                    xml.WriteAttributeString("height", map.Height.ToString());
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();*/
            }

            private void WriteElement(XmlWriter xml, string name, object value)
            {
                xml.WriteStartElement(name);

                string towrite;

                if (value is Color)
                    towrite = ((Color)value).ToArgb().ToString();
                else
                    towrite = value?.ToString() ?? "";

                xml.WriteString(towrite);
                xml.WriteEndElement();
            }

            public void Load(XmlElement node)
            {
                MapX = Convert.ToInt32(Utility.GetText(node["MapX"], null));
                MapY = Convert.ToInt32(Utility.GetText(node["MapY"], null));
                MapW = Convert.ToInt32(Utility.GetText(node["MapW"], null));
                MapH = Convert.ToInt32(Utility.GetText(node["MapH"], null));
                Port = Convert.ToInt32(Utility.GetText(node["Port"], null));
                ChatX = Convert.ToInt32(Utility.GetText(node["ChatX"], null));
                ChatY = Convert.ToInt32(Utility.GetText(node["ChatY"], null));
                ChatW = Convert.ToInt32(Utility.GetText(node["ChatW"], null));
                ChatH = Convert.ToInt32(Utility.GetText(node["ChatH"], null));
                //RefreshType = (RefreshType)Enum.Parse(typeof(RefreshType), Utility.GetText(node["RefreshType"], "1"));
                _fontStyle = Convert.ToInt32(Utility.GetText(node["FontStyle"], null));
                _opacity = Convert.ToInt32(Utility.GetText(node["Opacity"], null));
                Ip = Utility.GetText(node["Ip"], null);
                _name = Utility.GetText(node["Name"], null);
                Password = Utility.GetText(node["Password"], null);
                ClientVersion = Utility.GetText(node["ClientVersion"], null);
                ClientDir = Utility.GetText(node["ClientDir"], null);
                CommanderName = Utility.GetText(node["CommanderName"], null);
                FontName = Utility.GetText(node["FontName"], null);
                Autologin = Convert.ToBoolean(Utility.GetText(node["Autologin"], null));
                ShowCoords = Convert.ToBoolean(Utility.GetText(node["ShowCoords"], null));
                ShowPgName = Convert.ToBoolean(Utility.GetText(node["ShowPgName"], null));
                Guardlines = Convert.ToBoolean(Utility.GetText(node["Guardlines"], null));
                HpBar = Convert.ToBoolean(Utility.GetText(node["HpBar"], null));
                PanicAdvice = Convert.ToBoolean(Utility.GetText(node["PanicAdvice"], null));
                TrackDeathPoint = Convert.ToBoolean(Utility.GetText(node["TrackDeathPoint"], null));
                ShowBuilds = Convert.ToBoolean(Utility.GetText(node["ShowBuilds"], null));
                TiltMap = Convert.ToBoolean(Utility.GetText(node["TiltMap"], "True"));
                OpenChatIfMsg = Convert.ToBoolean(Utility.GetText(node["OpenChatIfMsg"], null));
                OpenChat = Convert.ToBoolean(Utility.GetText(node["OpenChat"], null));
                AbbreviatePgName = Convert.ToBoolean(Utility.GetText(node["AbbreviatePgName"], null));
                StamBar = Convert.ToBoolean(Utility.GetText(node["StamBar"], null));
                ManaBar = Convert.ToBoolean(Utility.GetText(node["ManaBar"], null));
                Statics = Convert.ToBoolean(Utility.GetText(node["Statics"], null));
                ShowTownName = Convert.ToBoolean(Utility.GetText(node["ShowTownName"], null));
                TryToRelog = Convert.ToBoolean(Utility.GetText(node["TryToRelog"], null));
                SmartVisual = Convert.ToBoolean(Utility.GetText(node["SmartVisual"], null));
                SmartIcons = Convert.ToBoolean(Utility.GetText(node["SmartIcons"], null));
                ClearCache = Convert.ToBoolean(Utility.GetText(node["ClearCache"], null));
                AutoCenterOnPg = Convert.ToBoolean(Utility.GetText(node["AutoCenterOnPg"], null));
                ShowHouses = Convert.ToBoolean(Utility.GetText(node["ShowHouses"], null));
                ShowServerBounds = Convert.ToBoolean(Utility.GetText(node["ShowServerBounds"], null));
                LogHouses = Convert.ToBoolean(Utility.GetText(node["LogHouses"], null));
                PointInfo = Convert.ToBoolean(Utility.GetText(node["PointInfo"], null));
                ShowScrollBars = Convert.ToBoolean(Utility.GetText(node["ShowScrollBars"], null));
                ChatColor = Color.FromArgb(Convert.ToInt32(Utility.GetText(node["ChatColor"], null)));
                _zoomValue = Convert.ToSingle(Utility.GetText(node["ZoomValue"], null)?.Replace(",", "."), CultureInfo.DefaultThreadCurrentCulture);
                _fontSize = Convert.ToSingle(Utility.GetText(node["FontSize"], null).Replace(",", "."), CultureInfo.DefaultThreadCurrentCulture);
                PanicSquare = Convert.ToBoolean(Utility.GetText(node["PanicSquare"], "True"));
                AlertSquare = Convert.ToBoolean(Utility.GetText(node["AlertSquare"], "True"));
                AttachOnUo = Convert.ToBoolean(Utility.GetText(node["AttachOnUO"], "False"));

                foreach (XmlElement m in node["macros"].GetElementsByTagName("macro"))
                {
                    if (!m.HasAttributes)
                        continue;

                    Macros[m.GetAttribute("label")] = new Macro(m.GetAttribute("name1"), m.GetAttribute("name2"));
                }

                /*if (node["labelsfile"] == null)
                {
                    foreach (var file in LabelsManager.Labels)
                        file.Enabled = true;
                    return;
                }

                foreach (XmlElement m in node["labelsfile"].GetElementsByTagName("file"))
                {
                    if (!m.HasAttributes)
                        continue;
                    var file = LabelsManager.Labels.FirstOrDefault(s => s.Name == m.GetAttribute("name"));
                    if (file == null)
                        continue;

                    file.Enabled = Convert.ToBoolean(m.GetAttribute("enabled"));
                }

                MapType = (MapType)Enum.Parse(typeof(MapType), Utility.GetText(node["MapType"], "0"));

                if (MapType == MapType.Custom)
                {
                    int index = 0;
                    int realindex = 0;
                    foreach (XmlElement elem in node["maps"].GetElementsByTagName("map"))
                    {
                        if (!elem.HasAttributes) continue;

                        try
                        {
                            if (!string.IsNullOrEmpty(elem.GetAttribute("index")))
                                index = Convert.ToInt32(elem.GetAttribute("index"));
                        }
                        catch { }

                        CustomMap map = new CustomMap(elem.GetAttribute("name"), index, realindex++, Convert.ToInt32(elem.GetAttribute("width")), Convert.ToInt32(elem.GetAttribute("height")));
                        MapBase.Maps.Add(map);

                        index++;
                    }
                }
                else
                    ClassicMap.Setup();*/
            }

            public class Macro
            {
                public Macro(string name1, string name2)
                {
                    MacroName1 = name1;
                    MacroName2 = name2;
                }

                public string MacroName1 { get; set; }

                public string MacroName2 { get; set; }
            }
        }
    }
}