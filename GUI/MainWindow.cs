using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnhancedMap.Core.Network;
using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core;
using System.IO;
using System.Diagnostics;
using EnhancedMap.Diagnostic;

namespace EnhancedMap.GUI
{
    public partial class MainWindow : CustomForm
    {
        private EnhancedCanvas _enhancedCanvas;
        private System.Windows.Forms.Timer _timer;

        private long _counter;
        private long _refresh = 9999L;
        private long _nextRefresh;
        private long _checkUOStateForm;
        private Point _pointClicked;
        private bool _dragging;
        private Point _lastScroll;
        private bool _requestRefresh = false;
        private DateTime _lastSignalSended;
        private Panel _panel;

        public MainWindow()
        {        
            InitializeComponent();

            Global.MainWindow = this;

            this.MaximumSize = new Size(1200, 1200);
            this.MinimumSize = new Size(this.MinimumSize.Width, 150);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

#if BETA
            Text = $"Enhanced Map - {MainCore.MapVersion} - BETA";
#else
            Text = $"Enhanced Map - {MainCore.MapVersion}";
#endif

            SettingsWindow = new Settings()
            {
                Visible = false,
            };
            ChatWindow = new ChatF()
            {
                Visible = false,
                ShowInTaskbar = false,
                MinimizeBox = false,
                MaximizeBox = false,
            };



            Logger.Log("Initializing main window...");

            this.FormClosing += (sender, e) =>
            {
                Logger.Log("Main Window closing.");
                SettingsWindow.SaveSettings();
                FilesManager.Save();
                HouseReader.SaveXml();
                Process.GetCurrentProcess().Kill();
            };

            _panel = new Panel()
            {
                Parent = this,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(2, Native.STATUS_BAR_HEIGHT),
                Size = new Size(this.ClientRectangle.Width - 4, this.ClientRectangle.Height - Native.STATUS_BAR_HEIGHT - 2)
            };


            _enhancedCanvas = new EnhancedCanvas()
            {
                Parent = _panel,
                Scrolls = false,
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                /*Parent = this,
                Scrolls = false,
                BackColor = Color.Black,
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(2, Native.STATUS_BAR_HEIGHT),
                Size = new Size(this.ClientRectangle.Width - 4, this.ClientRectangle.Height - Native.STATUS_BAR_HEIGHT - 2)*/
            };

            _enhancedCanvas.ECanvas.Paint += ECanvas_Paint;
            _enhancedCanvas.ECanvas.MouseDown += ECanvas_MouseDown;
            _enhancedCanvas.ECanvas.MouseMove += ECanvas_MouseMove;
            _enhancedCanvas.ECanvas.MouseUp += ECanvas_MouseUp;
            _enhancedCanvas.ECanvas.MouseDoubleClick += ECanvas_MouseDoubleClick;
            _enhancedCanvas.ECanvas.MouseWheel += ECanvas_MouseWheel;
            _enhancedCanvas.ECanvas.Click += ECanvas_Click;
            _enhancedCanvas.ECanvas.MouseEnter += ECanvas_MouseEnter;
            _enhancedCanvas.ECanvas.MouseLeave += ECanvas_MouseLeave;

            this.Resize += MainWindow_Resize;

           

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = 1000 / Global.SettingsCollection["fps"].ToInt();



            // context menu
#region "CONTEXT MENU"
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem usersM = new ToolStripMenuItem("Users: 0");
            menu.Items.Add(usersM);

            ToolStripMenuItem markesM = new ToolStripMenuItem("Markers");
            ToolStripMenuItem markesM_local = new ToolStripMenuItem("Add local", Properties.Resources.pin, (sender, e) => { RenderObjectsManager.AddMarkerObject(new MarkerObject(Global.PlayerInstance, MouseManager.Location.X, MouseManager.Location.Y)); _requestRefresh = true; });
            ToolStripMenuItem markesM_shared = new ToolStripMenuItem("Add shared", Properties.Resources.SHAREDLABEL, (sender, e) => { if (SharedLabelWindow == null || SharedLabelWindow.IsDisposed) SharedLabelWindow = new SharedLabelF(); if (!SharedLabelWindow.Visible) SharedLabelWindow.Show(this); });
            markesM.DropDownItems.Add(markesM_local);
            markesM.DropDownItems.Add(markesM_shared);
            menu.Items.Add(markesM);

            ToolStripMenuItem signalsM = new ToolStripMenuItem("Signals");
            ToolStripMenuItem signalsM_panic = new ToolStripMenuItem("Send panic!", null, (sender, e) => { CommandManager.DoCommand((Global.PlayerInstance.InPanic ? "un" : "") + "panic"); });
            ToolStripMenuItem signalsM_alerts = new ToolStripMenuItem("Send alert", null, (sender, e) => 
            {
                if (_lastSignalSended < DateTime.Now)
                {
                    RenderObjectsManager.AddSignal(new SignalObject(MouseManager.Location.X, MouseManager.Location.Y));
                    _lastSignalSended = DateTime.Now.AddMilliseconds(500);
                    NetworkManager.SocketClient.Send(new PAlert((ushort)MouseManager.Location.X, (ushort)MouseManager.Location.Y));
                }
            });
            signalsM.DropDownItems.Add(signalsM_panic);
            signalsM.DropDownItems.Add(signalsM_alerts);
            menu.Items.Add(signalsM);

            ToolStripMenuItem deathPoinsM = new ToolStripMenuItem("Death points", Properties.Resources.tombstone);
            menu.Items.Add(deathPoinsM);
            //ToolStripMenuItem clearDeathPointsM = new ToolStripMenuItem("Clear death points", null, (sender, e) => { RenderObjectsManager.Get<DeathObject>().ToList().ForEach(s => s.Dispose()); });
            //menu.Items.Add(clearDeathPointsM);



            menu.Items.Add(new ToolStripSeparator());


            ToolStripMenuItem freevM = new ToolStripMenuItem("Free view", null, (sender, e) => { ((ToolStripMenuItem)sender).Checked = Global.FreeView = !Global.FreeView; });
            menu.Items.Add(freevM);

            ToolStripMenuItem zooomM = new ToolStripMenuItem("Zoom");
            zooomM.DropDownItems.AddRange(new ToolStripItem[]
            {
                    new ToolStripMenuItem("12.5%", null, (sender, e) => { Global.SetInitialZoom(0.125f); _requestRefresh = true; }) { Tag = 0.125f },
                    new ToolStripMenuItem("25%", null, (sender, e) => { Global.SetInitialZoom(0.25f);_requestRefresh = true; }){ Tag = 0.25f },
                    new ToolStripMenuItem("50%", null, (sender, e) => { Global.SetInitialZoom(0.5f); _requestRefresh = true;}){ Tag = 0.5f },
                    new ToolStripMenuItem("75%", null, (sender, e) => { Global.SetInitialZoom(0.75f); _requestRefresh = true;}){ Tag = 0.75f },
                    new ToolStripMenuItem("100%", null, (sender, e) => { Global.SetInitialZoom(1f); _requestRefresh = true;}){ Tag = 1f },
                    new ToolStripMenuItem("150%", null, (sender, e) => { Global.SetInitialZoom(1.5f);_requestRefresh = true; }){ Tag = 1.5f },
                    new ToolStripMenuItem("200%", null, (sender, e) => { Global.SetInitialZoom(2f);_requestRefresh = true; }){ Tag = 2f },
                    new ToolStripMenuItem("400%", null, (sender, e) => { Global.SetInitialZoom(4f); _requestRefresh = true;}){ Tag = 4f },
                    new ToolStripMenuItem("600%", null, (sender, e) => { Global.SetInitialZoom(6f);_requestRefresh = true; }){ Tag = 6f },
                    new ToolStripMenuItem("800%", null, (sender, e) => { Global.SetInitialZoom(8f);_requestRefresh = true; }){ Tag = 8f },
            });
            menu.Items.Add(zooomM);

            ToolStripMenuItem facetesM = new ToolStripMenuItem("Maps");
            menu.Items.Add(facetesM);

            ToolStripMenuItem flipM = new ToolStripMenuItem("Flip", null, (sender, e) => { Global.Angle = Global.Angle == 45f ? 0f : 45f; _requestRefresh = true; });
            menu.Items.Add(flipM);


            ToolStripMenuItem toolsM = new ToolStripMenuItem("Tools");
            ToolStripMenuItem macroesM = new ToolStripMenuItem("Macroes");


            ToolStripMenuItem placesM = new ToolStripMenuItem("Places editor", null, (seneder, e) =>
            {
                if (PlacesEditorWindow == null || PlacesEditorWindow.IsDisposed)
                {
                    BuildingObject building =  
                    RenderObjectsManager.Get<BuildingObject>()
                    .LastOrDefault(s => 
                    (Global.Facet == s.Entry.Map || ((Global.Facet == 0 || Global.Facet == 1) 
                    && s.Entry.Map == 7)) 
                    && s.IsMouseOver

                    );

                    if (building != null)
                    {
                        PlacesEditorWindow = new PlacesEditorF(building.Entry);
                    }
                    else
                    {
                        HouseObject house = RenderObjectsManager.Get<HouseObject>().LastOrDefault(s => (Global.Facet == s.Entry.Map || ((Global.Facet == 0 || Global.Facet == 1) && s.Entry.Map == 7)) && s.IsMouseOver);
                        if (house != null)
                        {
                            PlacesEditorWindow = new PlacesEditorF(house.Entry);
                        }
                        else
                        {
                            PlacesEditorWindow = new PlacesEditorF();
                        }
                    }
                }
                if (!PlacesEditorWindow.Visible)
                    PlacesEditorWindow.Show(this);
            });
            ToolStripMenuItem finderM = new ToolStripMenuItem("Search...", null, (sender, e) => { if (SearcerWindow == null || SearcerWindow.IsDisposed) SearcerWindow = new SearcherF(); if (!SearcerWindow.Visible) SearcerWindow.Show(this); });
            ToolStripMenuItem coordsConverterM = new ToolStripMenuItem("Coords converter", null, (sender, e) => { if (CoordsConverterWindow == null || CoordsConverterWindow.IsDisposed) CoordsConverterWindow = new CoordsConverterF(); if (!CoordsConverterWindow.Visible) CoordsConverterWindow.Show(this); });
            //ToolStripMenuItem atlasM = new ToolStripMenuItem("Atlas");
            toolsM.DropDownItems.Add(placesM);
            //toolsM.DropDownItems.Add(macroesM);
            toolsM.DropDownItems.Add(finderM);
            toolsM.DropDownItems.Add(coordsConverterM);
            //toolsM.DropDownItems.Add(atlasM);
            menu.Items.Add(toolsM);


            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem connectM = new ToolStripMenuItem("Connect", null, (sender, e) => 
            {
                ToolStripMenuItem t = (ToolStripMenuItem)sender;
                if (!NetworkManager.SocketClient.IsConnected)
                    NetworkManager.Connect();
                else
                    NetworkManager.Disconnect(false);
            });
            menu.Items.Add(connectM);
            ToolStripMenuItem chatM = new ToolStripMenuItem("Chat", null, (sender, e) => { if (ChatWindow.Visible) ChatWindow.Hide(); else ChatWindow.Show(this); } );
            menu.Items.Add(chatM);

            menu.Items.Add(new ToolStripSeparator());

            //ToolStripMenuItem wndAttachM = new ToolStripMenuItem("Follow UO window", null, (sender, e) => { ((ToolStripMenuItem)sender).Checked = (bool)(Global.SettingsCollection["followuowindowstate"] = !Global.SettingsCollection["followuowindowstate"].ToBool()); });
           // menu.Items.Add(wndAttachM);
            ToolStripMenuItem uoclientsM = new ToolStripMenuItem("UO Clients");
            menu.Items.Add(uoclientsM);



            ToolStripMenuItem settingsM = new ToolStripMenuItem("Settings", null, (sender, e) => { if (!SettingsWindow.Visible) SettingsWindow.Show(this); });
            menu.Items.Add(settingsM);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitM = new ToolStripMenuItem("Exit", null, (sender, e) => { this.Close(); });
            menu.Items.Add(exitM);

            _enhancedCanvas.ECanvas.ContextMenuStrip = menu;


            menu.Opening += (sender, e) =>
            {
                chatM.Checked = ChatWindow.Visible;
                deathPoinsM.DropDownItems.Clear();
                DeathObject[] deathobjs = RenderObjectsManager.Get<DeathObject>();
                foreach (DeathObject deathobj in deathobjs)
                {
                    if (!deathobj.IsDisposing && deathobj.Parent != null && !deathobj.Parent.IsDisposing)
                    {
                        ToolStripMenuItem dpM = new ToolStripMenuItem(deathobj.Parent.Name + "'s tombstone", Properties.Resources.tombstone, (ssender, ee) => { if (!Global.FreeView) Global.FreeView = true; Global.X = deathobj.Position.X; Global.Y = deathobj.Position.Y; Global.Facet = deathobj.Map; _requestRefresh = true; });
                        ToolStripMenuItem removedpM = new ToolStripMenuItem("Remove", null, (ssender, ee) => { deathobj.Dispose(); });
                        dpM.DropDownItems.Add(removedpM);
                        deathPoinsM.DropDownItems.Add(dpM);
                    }
                }

                if (deathobjs.Length > 0)
                {
                    deathPoinsM.Enabled = true;
                    ToolStripMenuItem clearDpM = new ToolStripMenuItem("Clear", null, (ssender, ee) => { deathobjs.ToList().ForEach(s => s.Dispose()); });
                    deathPoinsM.DropDownItems.Add(clearDpM);
                }
                else
                    deathPoinsM.Enabled = false;           

                freevM.Checked = Global.FreeView;

                uoclientsM.DropDownItems.Clear();

                var clients = UOClientManager.GetClientsWindowTitles();
                foreach (KeyValuePair<IntPtr, string> c in clients)
                {
                    ToolStripMenuItem tc = new ToolStripMenuItem(c.Value, null, (ssender, ee) => { UOClientManager.AttachToClient(c.Key); });
                    uoclientsM.DropDownItems.Add(tc);
                }

                connectM.Text = NetworkManager.SocketClient.IsConnected ? "Disconnect" : "Connect";
                connectM.Image = NetworkManager.SocketClient.IsConnected ? Properties.Resources.right : Properties.Resources.wrong;

                flipM.Checked = Global.Angle == 45f;

                usersM.DropDownItems.Clear();
                var users = RenderObjectsManager.Get<UserObject>();
                foreach (UserObject user in users)
                {
                    if (user == null || user.IsDisposing)
                        continue;

                    ToolStripMenuItem tu = new ToolStripMenuItem(user.Name);
                    ToolStripMenuItem tu_track = new ToolStripMenuItem("Track", null, (ssender, ee) => 
                    {
                        if (user != null && !user.IsDisposing)
                        {
                            if (Global.FreeView)
                                Global.FreeView = false;
                            
                            if (user is PlayerObject player)
                                Global.TrackedUser = null;
                            else
                                Global.TrackedUser = user;
                            

                            _requestRefresh = true;
                        }
                    });
                    ToolStripMenuItem tu_commander = new ToolStripMenuItem("Highlight", null, (ssender, ee) => { if (user != null && !user.IsDisposing) { if ((user.IsHighlighted = !user.IsHighlighted)) Global.HighlightesUsername.Add(user.Name); else if (Global.HighlightesUsername.Contains(user.Name)) Global.HighlightesUsername.Remove(user.Name); } });
                    tu.DropDownItems.Add(tu_track);
                    tu.DropDownItems.Add(tu_commander);

                    if (user.IsHighlighted)
                        tu_commander.Checked = true;
                    else if (!user.IsHighlighted && tu_commander.Checked)
                        tu_commander.Checked = false;

                    if (((Global.TrackedUser != null && Global.TrackedUser == user) || (Global.TrackedUser == null && user is PlayerObject)) && !Global.FreeView)
                        tu.Checked = true;
                    else if (tu.Checked)
                        tu.Checked = false;

                    usersM.DropDownItems.Add(tu);
                    if (user is PlayerObject && users.Length > 1)
                        usersM.DropDownItems.Add(new ToolStripSeparator());            
                }

                usersM.Text = "Users: " + users.Length;

                facetesM.DropDownItems.Clear();
                foreach (MapEntry map in Global.Maps)
                {
                    if (map == null)
                        continue;

                    ToolStripMenuItem item = new ToolStripMenuItem(map.Name, null, (ssender, ee) => { Global.Facet = map.Index; Global.IsFacetChanged = Global.PlayerInstance.Map != Global.Facet; });
                    facetesM.DropDownItems.Add(item);

                    if (Global.Facet == map.Index)
                        item.Checked = true;
                }

                foreach (ToolStripMenuItem item in zooomM.DropDownItems)
                {
                    item.Checked = Global.Zoom == (float)item.Tag;
                }

                markesM.DropDownItems.Clear();
                markesM.DropDownItems.Add(markesM_local);
                markesM.DropDownItems.Add(markesM_shared);
                MarkerObject[] markers = RenderObjectsManager.Get<MarkerObject>();
                SharedLabelObject[] sharedlabels = RenderObjectsManager.Get<SharedLabelObject>();

                if (markers.Length > 0 || sharedlabels.Length > 0)
                    markesM.DropDownItems.Add(new ToolStripSeparator());

                foreach (MarkerObject mark in markers)
                {
                    ToolStripMenuItem markMenu = new ToolStripMenuItem(mark.Parent.Name + "'s marker", Properties.Resources.pin, (ssender, ee) =>
                    {
                        Global.FreeView = true;
                        Global.X = mark.Position.X;
                        Global.Y = mark.Position.Y;
                        if (Global.Facet != mark.Map)
                            Global.Facet = mark.Map;
                    });
                    ToolStripMenuItem markSubMenu = new ToolStripMenuItem("Remove", null, (ssender, ee) => { mark?.Dispose(); });

                    markMenu.DropDownItems.Add(markSubMenu);
                    markesM.DropDownItems.Add(markMenu);
                }

                foreach (SharedLabelObject sharedlabel in sharedlabels)
                {
                    ToolStripMenuItem markMenu = new ToolStripMenuItem(sharedlabel.Parent.Name + "'s shared label", Properties.Resources.SHAREDLABEL, (ssender, ee) =>
                    {
                        Global.FreeView = true;
                        Global.X = sharedlabel.Position.X;
                        Global.Y = sharedlabel.Position.Y;
                        if (Global.Facet != sharedlabel.Map)
                            Global.Facet = sharedlabel.Map;
                    });
                    ToolStripMenuItem markSubMenu = new ToolStripMenuItem("Remove", null, (ssender, ee) => {
                        if (NetworkManager.SocketClient.IsConnected)
                            NetworkManager.SocketClient.Send(new PSharedLabel((ushort)sharedlabel.Position.X, (ushort)sharedlabel.Position.Y, sharedlabel.Map));

                        sharedlabel?.Dispose(); });

                    markMenu.DropDownItems.Add(markSubMenu);
                    markesM.DropDownItems.Add(markMenu);
                }
                
            };
#endregion


            this.Load += (sender, e) =>
            {
                RebuildMaps();
                Global.Facet = Global.Maps.FirstOrDefault(s => s != null).Index;

                Global.CreatePlayerInstance(Global.SettingsCollection["username"].ToString());

                FilesManager.Guardlines.ForEach(s => RenderObjectsManager.AddGuardline(new GuardLineObject(s)));
                FilesManager.Houses.ForEach(s => RenderObjectsManager.AddHouse(new HouseObject(s)));
                FilesManager.BuildSets.ForEach(s => s.Entries.ForEach(a => RenderObjectsManager.AddBuilding(new BuildingObject(a))));

                HouseReader.LoadXml();

                int mapX = Global.SettingsCollection["mapX"].ToInt();
                int mapY = Global.SettingsCollection["mapY"].ToInt(); 

                if (mapX <= -1 || mapY <= -1)
                    this.Location = new Point(0, 0);
                else
                    this.Location = new Point(mapX, mapY);

                int mapW = Global.SettingsCollection["mapW"].ToInt();
                int mapH = Global.SettingsCollection["mapH"].ToInt();
                if (mapW <= -1 || mapH <= -1)
                    this.Size = new Size(250, 250);
                else
                    this.Size = new Size(mapW, mapH);
            };

            this.Shown += (sender, e) =>
            {
                _timer.Start();

                if (Global.SettingsCollection["autologin"].ToBool())
                    NetworkManager.Connect();

                if (Global.SettingsCollection["openchatatstart"].ToBool())
                    ChatWindow.Show(this);
            };

            void func(ConnectionStatus status)
            {
#if BETA
                this.Do(s => s.Text = $"Enhanced Map - {MainCore.MapVersion} - {status} - BETA");
#else
                this.Do(s => s.Text = $"Enhanced Map - {MainCore.MapVersion} - {status}");
#endif
                this.Do(s => s.Invalidate());

                _requestRefresh = true;
            }

            SocketClient.Connected += (sender, e) => func(ConnectionStatus.Online);
            SocketClient.Waiting += (sender, e) => func(ConnectionStatus.Waiting);
            SocketClient.Disconnected += (sender, e) => func(ConnectionStatus.Offline);

            Application.Idle += Application_Idle;

            int detailChanged = -1;
            Global.FacetChanged += (sender, e) =>
            {
                RenderMap[] maps = RenderObjectsManager.Get<RenderMap>();

                if (!Global.SettingsCollection["loadmapsondemand"].ToBool())
                {
                    RenderMap map = maps.FirstOrDefault(s => s.MapEntry != null && s.MapEntry.Index == e.Previous);
                    if (map != null)
                    {
                        RenderObjectsManager.RemoveMap(map);
                        map.Unload();
                        map = null;
                    }

                    bool detailed = Global.SettingsCollection["mapkind"].ToInt() == 0;

                    string path = Path.Combine("Maps", (detailed ? "2D" : "") + "map" + e.Current + ".png");
                    if (!File.Exists(path))
                    {
                        foreach (MapEntry m in Global.Maps)
                        {
                            if (m != null)
                            {
                                Global.Facet = m.Index;
                                e.Allow = false;
                                return;
                            }
                        }
                    }

                    RenderObjectsManager.AddMap(new RenderMap(Global.Maps[e.Current], path));
                }
                else
                {
                    if (maps.Length <= 0)
                    {
                        if (Global.SettingsCollection["loadmapsondemand"].ToBool())
                        {
                            int firstvalidmapIdx = -1;

                            for (int i = 0; i < 2; i++)
                            {
                                foreach (MapEntry map in Global.Maps)
                                {
                                    if (map != null)
                                    {
                                        if (firstvalidmapIdx == -1)
                                        {
                                            firstvalidmapIdx = map.Index;
                                        }

                                        string path = Path.Combine("Maps", (i == 0 ? "2D" : "") + "map" + map.Index + ".png");

                                        RenderMap m = new RenderMap(map, path);
                                        RenderObjectsManager.AddMap(m);
                                        m.IsVisible = false;
                                        m.Load();
                                    }                                   
                                }
                            }

                           /* if (Global.Facet != firstvalidmapIdx)
                            {
                                Global.Facet = firstvalidmapIdx;
                                e.Allow = false;
                                return;
                            }*/
                        }

                        maps = RenderObjectsManager.Get<RenderMap>();
                    }

                    int result = Global.SettingsCollection["mapkind"].ToInt();

                    bool detailed = result == 0;

                    int countValidMaps = Global.Maps.Where(s => s != null).Count() + 1;
                    if (countValidMaps == Global.Maps.Length + 1)
                        countValidMaps = 0;

                    if (detailChanged == -1)
                        detailChanged = result;
                    else if (detailChanged != result)
                    {
                        maps[!detailed ? e.Previous - countValidMaps : (e.Previous - countValidMaps) + maps.Length / 2].IsVisible = false;
                    }
                    else
                    {
                        maps[detailed ? e.Previous- countValidMaps : (e.Previous -countValidMaps) + maps.Length / 2].IsVisible = false;
                    }
                    maps[detailed ? e.Current - countValidMaps : (e.Current - countValidMaps) + maps.Length / 2].IsVisible = true;

                    detailChanged = result;
                }

                _requestRefresh = true;
            };
           
            Global.SetZoomIndex(Global.SettingsCollection["zoomIndex"].ToInt());
            RenderObjectsManager.AddLabel(Global.CurrentLabelObject);
            RenderObjectsManager.AddMask(RoseMask.RoseInstance);
            RenderObjectsManager.AddMask(PanicMask.PanicInstance);
            RenderObjectsManager.AddMask(FPSMask.FPSInstance);
            RenderObjectsManager.AddMask(ConnectionMask.ConnectionInstance);
            RenderObjectsManager.AddMask(CoordinatesMask.CoortinatesInstance);
            RenderObjectsManager.AddMask(ZoomMask.ZoomInstance);
            RenderObjectsManager.AddMask(CrossMask.CorssInstance);

            Global.OnFreeView += (sender, e) =>
            {
                if (e)
                {
                    Global.TrackedUser = null;
                }
                else
                {
                    Global.Facet = Global.PlayerInstance.Map;
                    Global.X = Global.PlayerInstance.Position.X;
                    Global.Y = Global.PlayerInstance.Position.Y;
                    Global.IsFacetChanged = false;
                }
                _requestRefresh = true;
            };

            ChatManager.MessageWrited += (sender, e) =>
            {
                if (Global.SettingsCollection["showincomingmsg"].ToBool())
                {
                    if (ChatWindow.WindowState == FormWindowState.Minimized)
                        ChatWindow.WindowState = FormWindowState.Normal;
                }

                if (Form.ActiveForm != ChatWindow && e.Name != Global.PlayerInstance.Name && Global.SettingsCollection["soundsincomingmsg"].ToBool())
                {
                    SoundsManager.Play(SOUNDS_TYPE.CHATMSG);
                }

            };

            UOClientManager.Initialize(this.Handle);

            Logger.Log("main window inizialized.");
        }

        public Settings SettingsWindow { get; private set; }
        public ChatF ChatWindow { get; private set; }
        public PlacesEditorF PlacesEditorWindow { get; private set; }
        public SearcherF SearcerWindow { get; private set; }
        public CoordsConverterF CoordsConverterWindow { get; private set; }
        public SharedLabelF SharedLabelWindow { get; private set; }
        
        public void RebuildMaps(bool restarttimer = false)
        {
            _timer.Stop();

            if (!MapsManager.CheckMapExists())
            {
                this.Hide();
                MapMakerF mapmakerf = new MapMakerF();
                if (mapmakerf.ShowDialog() == DialogResult.Abort)
                {
                    this.Close();
                    Process.GetCurrentProcess().Kill();
                    return;
                }
            }

            MapsManager.LoadMaps();

            if (restarttimer)
                _timer.Start();
        }

        public void SetTimerIntervalByFps(int fps)
        {
            _timer.Interval = 1000 / fps;
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            RenderMap[] maps = RenderObjectsManager.Get<RenderMap>();

            for(int i = 0; i < maps.Length; i++)
            {
                if (maps[i].IsVisible)
                    maps[i].Renew();
            }
            _requestRefresh = true;
            _enhancedCanvas.ECanvas.Invalidate();
        }

        private bool UpdatePosition()
        {
            if (Global.PlayerInstance == null)
                return false;

            bool ok = Global.PlayerInstance.Update();

            if (Global.TrackedUser == null && !Global.FreeView)
            {
                if (Global.PlayerInstance.InGame)
                {
                    if (Global.X != Global.PlayerInstance.Position.X)
                        Global.X = Global.PlayerInstance.Position.X;
                    if (Global.Y != Global.PlayerInstance.Position.Y)
                        Global.Y = Global.PlayerInstance.Position.Y;

                    if (!Global.IsFacetChanged && Global.Facet != Global.PlayerInstance.Map)
                        Global.Facet = Global.PlayerInstance.Map;
                }
                else
                {
                    if (Global.X != 0) Global.X = 0;
                    if (Global.Y != 0) Global.Y = 0;
                    if (!Global.IsFacetChanged && Global.Facet != Global.FirstValidFacet)
                        Global.Facet = Global.FirstValidFacet;
                }
            }
            else
            {
                if (!Global.FreeView && Global.TrackedUser != null)
                {
                    if (Global.X != Global.TrackedUser.Position.X)
                    {
                        Global.X = Global.TrackedUser.Position.X;
                        ok |= true;
                    }
                    if (Global.Y != Global.TrackedUser.Position.Y)
                    {
                        Global.Y = Global.TrackedUser.Position.Y;
                        ok |= true;
                    }

                    if (!Global.IsFacetChanged && Global.Facet != Global.TrackedUser.Map)
                        Global.Facet = Global.TrackedUser.Map;
                }
            }
            return ok;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            UOClientManager.AttachToClient();
            NetworkManager.SocketClient.Sync();

            if (Global.SettingsCollection["followuowindowstate"].ToBool())
            {
                FormWindowState state = UOClientManager.GetProcessWindowState(this.WindowState);
                if (state != this.WindowState)
                {
                    this.WindowState = state;

                    switch (WindowState)
                    {
                        default:
                        case FormWindowState.Normal:
                            if (!ChatWindow.Visible)
                                ChatWindow.Show(this);
                            break;
                        case FormWindowState.Minimized:
                            if (ChatWindow.Visible)
                                ChatWindow.Hide();
                            break;
                    }

                }
            }

           // if (WindowState != FormWindowState.Minimized && !ChatWindow.Visible)
           // {
                //ChatWindow.Show(this);
           // }


            //else if (WindowState == FormWindowState.Normal && !ChatWindow.Visible)
             //   ChatWindow.Show();

            /*switch (WindowState)
            {
                default:
                case FormWindowState.Normal:
                    if (!ChatWindow.Visible)
                        ChatWindow.Show();
                    break;
                case FormWindowState.Minimized:
                    if (ChatWindow.Visible)
                        ChatWindow.Hide();
                    break;
            }*/

            _counter += _timer.Interval;
            if (_counter >= 100L)
            {
                _nextRefresh++;
                _counter -= 100L;
                _checkUOStateForm++;

                if (_nextRefresh % 8L == 0L)
                    _requestRefresh = true;
                if (_nextRefresh % 1L == 0L && (UpdatePosition() || NetworkManager.SocketClient.ReceivedPackets))
                    _requestRefresh = true;

                if (_checkUOStateForm % 4L == 0)
                {
                    //this.WindowState = UOClientManager.GetProcessWindowState();
                    _checkUOStateForm = 0;
                }
            }
        }

        private void ECanvas_MouseLeave(object sender, EventArgs e)
        {
            if (!_enhancedCanvas.ECanvas.ContextMenuStrip.Visible)
            {
                _requestRefresh = true;
                MouseManager.IsEnter = false;
            }
        }

        private void ECanvas_MouseEnter(object sender, EventArgs e)
        {
            // win7 fix wheel scroll
            // https://stackoverflow.com/questions/44735362/control-mousewheel-event
            _enhancedCanvas.ECanvas.Focus();

            _requestRefresh = true;
            MouseManager.IsEnter = true;
        }

        private void ECanvas_Click(object sender, EventArgs e)
        {
        }

        private void ECanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            Global.ChangeZoom(e.Delta < 0);
            _requestRefresh = true;
        }

        private void ECanvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*if (_enhancedCanvas.Dock != DockStyle.Fill)
            {
                _enhancedCanvas.Dock = DockStyle.Fill;
                ChatWindow.TopMost = TopMost = true;
            }
            else
            {
                _enhancedCanvas.Dock = DockStyle.None;
                _enhancedCanvas.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _enhancedCanvas.Location = new Point(2, Native.STATUS_BAR_HEIGHT);
                _enhancedCanvas.Size = new Size(this.ClientRectangle.Width - 4, this.ClientRectangle.Height - Native.STATUS_BAR_HEIGHT - 2);
                ChatWindow.TopMost = TopMost = false;
            }*/

            if (_panel.Dock != DockStyle.Fill)
            {
                _panel.Dock = DockStyle.Fill;
                ChatWindow.TopMost = TopMost = true;
            }
            else
            {
                _panel.Dock = DockStyle.None;
                _panel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _panel.Location = new Point(2, Native.STATUS_BAR_HEIGHT);
                _panel.Size = new Size(this.ClientRectangle.Width - 4, this.ClientRectangle.Height - Native.STATUS_BAR_HEIGHT - 2);
                ChatWindow.TopMost = TopMost = false;
            }
        }

        private void ECanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragging)
                _dragging = false;

            if (Cursor != Cursors.Default)
                Cursor = Cursors.Default;

            if (e.Button == MouseButtons.Left)
                MouseManager.LeftIsPressed = false;
            else if (e.Button == MouseButtons.Right)
                MouseManager.RightIsPressed = false;
        }

        private void ECanvas_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            if (_dragging)
            {
                Point point = PointToScreen(e.Location);
                point.Offset(-_pointClicked.X, -_pointClicked.Y);
                Location = point;
            }
            else if ((e.Button == MouseButtons.Left && Global.FreeView) || 
                e.Button == MouseButtons.Middle)
            {
                if (e.Button == MouseButtons.Middle && !Global.FreeView)
                    Global.FreeView = true;

                int scrollX = _lastScroll.X - x; 
                int scrollY = _lastScroll.Y - y; 

                (scrollX, scrollY) = Geometry.RotatePoint(scrollX, scrollY, 1f, -1, Global.Angle);

                Global.X += scrollX / Global.Zoom;
                Global.Y += scrollY / Global.Zoom;

                _requestRefresh = true;

                if (Global.X < 0)
                    Global.X = 0;
                if (Global.Y < 0)
                    Global.Y = 0;
                if (Global.X > Global.Maps[Global.Facet].Width)
                    Global.X = Global.Maps[Global.Facet].Width;
                if (Global.Y > Global.Maps[Global.Facet].Height)
                    Global.Y = Global.Maps[Global.Facet].Height;
                _lastScroll = new Point(x, y);
            }
            else
            {
                x -= _enhancedCanvas.ECanvas.Width / 2;
                y -= _enhancedCanvas.ECanvas.Height / 2;

                (x, y) = Geometry.RotatePoint(x, y, 1f / Global.Zoom, -1, Global.Angle);

                x += (int)Global.X; y += (int)Global.Y;
                MouseManager.Location = new Position((short)x, (short)y);
                _requestRefresh = true;
            }
        }

        private void ECanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseManager.LeftIsPressed = true;
            else if (e.Button == MouseButtons.Right)
                MouseManager.RightIsPressed = true;


            if (MouseManager.LeftIsPressed || e.Button == MouseButtons.Middle)
            {
                if (TopMost && !Global.FreeView && e.Button != MouseButtons.Middle)
                {

                    _dragging = true;
                    _pointClicked = new Point(e.X, e.Y);
                    if (Cursor != Cursors.SizeAll)
                        Cursor = Cursors.SizeAll;
                }
                else
                {

                    if (_dragging)
                        _dragging = false;

                    if (Global.FreeView)
                    {
                        if (Cursor != Cursors.SizeAll)
                            Cursor = Cursors.SizeAll;
                        _lastScroll = e.Location;
                    }
                }
            }
        }

        private void ECanvas_Paint(object sender, PaintEventArgs e)
        {
            int centerX = _enhancedCanvas.Width / 2;
            int centerY = _enhancedCanvas.Height / 2;
            int w = _enhancedCanvas.Width;
            int h = _enhancedCanvas.Height;

            _requestRefresh |= RenderObjectsManager.Render(e.Graphics, w, h, Global.Angle) | RenderObjectsManager.Render(e.Graphics, centerX, centerY, w, h);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (PeekMessage(out MESSAGE message, IntPtr.Zero, 0, 0, 0))
                return;

            if (_counter == _refresh || !_timer.Enabled)
                return;

            _refresh = _counter;
            if (!_requestRefresh)
                return;

            _requestRefresh = false;
            _enhancedCanvas.ECanvas.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch(keyData)
            {
                case Keys.Space:
                    CommandManager.DoCommand((Global.PlayerInstance.InPanic ? "un" : "") + "panic");
                    _requestRefresh = true;
                    break;
                case Keys.Control | Keys.G:
                    if (_lastSignalSended < DateTime.Now)
                    {
                        RenderObjectsManager.AddSignal(new SignalObject(MouseManager.Location.X, MouseManager.Location.Y));
                        _lastSignalSended = DateTime.Now.AddMilliseconds(500);
                        NetworkManager.SocketClient.Send(new PAlert((ushort)MouseManager.Location.X, (ushort)MouseManager.Location.Y));
                    }
                    break;
                case Keys.Control | Keys.F:
                    {

                    }
                    break;
                case Keys.Up:
                    {
                        if (!Global.FreeView)
                            Global.FreeView = true;

                        Global.X -= 50 / Global.Zoom;
                        Global.Y -= 50 / Global.Zoom;

                        if (Global.X < 0)
                            Global.X = 0;
                        if (Global.Y < 0)
                            Global.Y = 0;
                        if (Global.X > Global.Maps[Global.Facet].Width)
                            Global.X = Global.Maps[Global.Facet].Width;
                        if (Global.Y > Global.Maps[Global.Facet].Height)
                            Global.Y = Global.Maps[Global.Facet].Height;
                    }
                    break;
                case Keys.Right:
                    {
                        if (!Global.FreeView)
                            Global.FreeView = true;

                        Global.X += 50 / Global.Zoom;
                        Global.Y -= 50 / Global.Zoom;

                        if (Global.X < 0)
                            Global.X = 0;
                        if (Global.Y < 0)
                            Global.Y = 0;
                        if (Global.X > Global.Maps[Global.Facet].Width)
                            Global.X = Global.Maps[Global.Facet].Width;
                        if (Global.Y > Global.Maps[Global.Facet].Height)
                            Global.Y = Global.Maps[Global.Facet].Height;
                    }
                    break;
                case Keys.Down:
                    {
                        if (!Global.FreeView)
                            Global.FreeView = true;
                        Global.X += 50 / Global.Zoom;
                        Global.Y += 50 / Global.Zoom;

                        if (Global.X < 0)
                            Global.X = 0;
                        if (Global.Y < 0)
                            Global.Y = 0;
                        if (Global.X > Global.Maps[Global.Facet].Width)
                            Global.X = Global.Maps[Global.Facet].Width;
                        if (Global.Y > Global.Maps[Global.Facet].Height)
                            Global.Y = Global.Maps[Global.Facet].Height;
                    }
                    break;
                case Keys.Left:
                    {
                        if (!Global.FreeView)
                            Global.FreeView = true;

                        Global.X -= 50 / Global.Zoom;
                        Global.Y += 50 / Global.Zoom;

                        if (Global.X < 0)
                            Global.X = 0;
                        if (Global.Y < 0)
                            Global.Y = 0;
                        if (Global.X > Global.Maps[Global.Facet].Width)
                            Global.X = Global.Maps[Global.Facet].Width;
                        if (Global.Y > Global.Maps[Global.Facet].Height)
                            Global.Y = Global.Maps[Global.Facet].Height;
                    }
                    break;
                default:
                    {
                        if (msg.Msg == 256)
                        {      
                            string c = Convert.ToChar(MapVirtualKey((uint)keyData, 2)).ToString().ToLower();
                            if (c == "\0")
                                break;

                            if (!ChatWindow.Visible)
                            {
                                ChatWindow.Visible = true;
                            }

                            ChatWindow.AppendText(c);
                        }
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)MSG_RECV.HOUSES_BOATS_INFO:
                    {
                        int x = (m.WParam.ToInt32() & 65535);
                        int y = m.WParam.ToInt32() >> 16;
                        uint id = (uint)m.LParam - 0x4000;

                        GameHouse gamehouse = HouseReader.GetHouse(id);
                        if (gamehouse != null)
                        {
                            x = x - gamehouse.Size.X / 2;
                            y = y - gamehouse.Size.Y / 2;

                            var houses = RenderObjectsManager.Get<HouseObject>()
                                .Where(s => 
                                s.Entry.Map == Global.PlayerInstance.Map 
                                && 
                                new Rectangle(s.Entry.Location.X, s.Entry.Location.Y, s.Entry.Size.Width, s.Entry.Size.Height)
                                    .IntersectsWith(new Rectangle((short)x, (short)y, gamehouse.Size.X, gamehouse.Size.Y)) )
                                    .ToList();


                            if (houses.Count == 0)
                            {
                                var h = new HouseObject(new HouseEntry(string.Format("{0} House: {1}x{2} at {3},{4} {5}", id.ToString("X"), gamehouse.Size.X, gamehouse.Size.Y, x, y, Global.PlayerInstance.Map), (ushort)id,
                                    new Position((short)x, (short)y), new Size(gamehouse.Size.X, gamehouse.Size.Y), Global.PlayerInstance.Map));
                                RenderObjectsManager.AddHouse(h);

                                FilesManager.Houses.Add(h.Entry);
                            }
                        }

                        break;
                    }
                case (int)MSG_RECV.DEL_HOUSES_BOATS_INFO:
                    {
                        break;
                    }
                case 1425: // chat
                    {
                        string text = string.Empty.FromAtom(m.WParam.ToInt32());
                        CommandManager.DoCommand(text);                  
                    }
                    break;
                case (int)MSG_RECV.MOBILE_ADD_STRCT:
                    {
                        int x = (m.WParam.ToInt32() & 65535);
                        int y = m.WParam.ToInt32() >> 16;
                        uint serial = (uint)m.LParam.ToInt32();

                        MobileObject mobile = RenderObjectsManager.Get<MobileObject>().SingleOrDefault(s => s.Serial == serial);
                        if (mobile == null)
                            RenderObjectsManager.AddMobile(mobile = new MobileObject(serial));

                        mobile.UpdatePosition(x, y);
                    }
                    break;
                case (int)MSG_RECV.MOBILE_REMOVE_STRCT:
                    {
                        uint serial = (uint)m.LParam.ToInt32();
                        MobileObject mobile = RenderObjectsManager.Get<MobileObject>().SingleOrDefault(s => s.Serial == serial);
                        if (mobile != null)
                            mobile.Dispose();
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
        private static extern bool PeekMessage(out MESSAGE struct20, IntPtr intptr1, uint uint1, uint uint2, uint uint3);
        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);
        private struct MESSAGE
        {
            public IntPtr hwnd;
            public IntPtr message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point pt;
        }
    }
}
