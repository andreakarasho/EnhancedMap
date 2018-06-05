using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using EnhancedMap.Core.MapObjects;
using EnhancedMap.GUI;
using System.Security;

namespace EnhancedMap.Core
{
    public class OnMapChangedEventArgs : EventArgs
    {
        public OnMapChangedEventArgs(int prev, int current)
        {
            Previous = prev; Current = current;
            Allow = true;
        }
        public int Previous { get; }
        public int Current { get; }
        public bool Allow { get; set; }
    }


    public static class Global
    {
        private static int _facet = 0;
        private static bool _freeview;
        private static int _zoomIndex = 4;
        private static readonly float[] _zooms = new float[10] { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 4f, 6f, 8f };

        public static event EventHandler<bool> OnFreeView;
        public static event EventHandler<OnMapChangedEventArgs> FacetChanged;

        public static Bitmap[] Bitmaps { get; } = new Bitmap[6];
        public static bool UseUODLL { get; internal set; } = true;
        public static bool IsFacetChanged { get; internal set; }
        public static bool FreeView { get { return _freeview; }  set { _freeview = value; OnFreeView.Raise(value); } }
        public static float Zoom => _zooms[_zoomIndex];

        public static string UOPath { get; set; }

        public static float Angle { get; internal set; } = 45;

        public static PlayerObject PlayerInstance { get; private set; }
        public static UserObject TrackedUser { get; set; }
        public static BuildingObject CurrentBuildObject { get; set; }
        public static HouseObject CurrentHouseObject { get; set; }
        public static LabelObject CurrentLabelObject { get; } = new LabelObject();
        public static void CreatePlayerInstance(string name)
        {
            PlayerInstance = new PlayerObject(name)
            {
                Hue = new SolidBrush(Color.FromArgb(SettingsCollection["namecolor"].ToInt())),
                Font = new Font(SettingsCollection["namefont"].ToString(), SettingsCollection["namesize"].ToInt(), (FontStyle)SettingsCollection["namestyle"].ToInt(), GraphicsUnit.Pixel),
                Map = (byte)FirstValidFacet,               
            };
            RenderObjectsManager.AddUser(PlayerInstance);
        }

        public static MainWindow MainWindow { get; internal set; }

        public static MapEntry[] Maps { get; } = new MapEntry[6];


        public static void ChangeZoom(bool plus)
        {
            if (plus)
            {
                _zoomIndex--;
                if (_zoomIndex < 0)
                    _zoomIndex = 0;
            }
            else
            {
                _zoomIndex++;
                if (_zoomIndex > _zooms.Length - 1)
                    _zoomIndex = _zooms.Length - 1;
            }
        }

        public static int SetInitialZoom(float zoom)
        {
            for (_zoomIndex = 0; _zoomIndex < _zooms.Length; _zoomIndex++)
            {
                if (zoom == _zooms[_zoomIndex])
                {
                    return _zoomIndex;
                }
            }
            return _zoomIndex = 4;
        }

        public static void SetZoomIndex(int index)
        {
            if (index < 0 || index >= _zooms.Length)
                _zoomIndex = 4;
            else
                _zoomIndex = index;
        }

        public static float X { get; set; }
        public static float Y { get; set; }
        public static int Facet
        {
            get { return _facet; }
            set
            {
                var eventargs = new OnMapChangedEventArgs(_facet, value);
                FacetChanged.Raise(eventargs);
               // if (eventargs.Allow)
                    _facet = value;
            }
        }

        private static int _firstValidFacet = -1;
        public static int FirstValidFacet
        {
           get
            {
                if (_firstValidFacet == -1)
                {
                    _firstValidFacet = Maps.FirstOrDefault(s => s != null).Index;
                }
                return _firstValidFacet;
            }
        }

        public static List<string> HighlightesUsername { get; } = new List<string>();


        public static Dictionary<string, object> SettingsCollection { get; } = new Dictionary<string, object>()
        {
            // network
            { "username", "You" },
            { "password", "" },
            { "remembercredentials", true },
#if BETA
            { "ip", "enhancedmap.hopto.org" },
#else
            { "ip", "" },
#endif
            { "port", 8887 },
            { "rememberserver", true },
            { "autologin", false },
            { "tryreconnect", false },
            { "tryreconnecttime", 30 },

            // general
            { "showhits", true},
            { "showstamina", true },
            { "showmana", true },
            { "centerplayer", false },
            { "trackdeathpoint", true },
            { "showhiddenicon", true },
            { "showdeathicon", true },
            { "abbreviatenames", false },
            { "panicsounds", true },
            { "alertsounds", true },
            { "smartnamesposition", false },
            { "showplacesicons", true },
            { "hidelessimportantplaces", false },
            { "showtownsnames", false },
            { "showserverbounds", true },
            { "showguardlines", true },
            { "showcoordinates", true},
            { "showhouses", true },
            { "showmobilesaroundyou", true },
            { "dontrenderusersindifferentfacets", false },
            { "nameposition", 0 },
            { "namecolor", Color.White.ToArgb() },
            { "namefont", "Segoe UI" },
            { "namesize", 12 },
            { "namestyle", (int)FontStyle.Regular },

            // application
            { "clienttype", 0 },
            { "showscrollbarls", false },
            { "openchatatstart", false },
            { "showincomingmsg", false },
            { "soundsincomingmsg", true },
            { "followuowindowstate", false },
            { "loadmapsondemand", false },
            { "showfps", false },
            { "fps", 30 },
            { "mapkind", 0 },
            { "clientpath", string.Empty },
            { "mapX" , -1 },
            { "mapY", -1 },
            { "mapW", -1 },
            { "mapH", -1 },
            { "chatX", -1 },
            { "chatY", -1 },
            { "chatW", -1 },
            { "chatH", -1 },
            { "zoomIndex", 4 },

            { "chatfontsize", 10 },

            // diagnostic
            { "autoscroll", true },

        };
    }
}
