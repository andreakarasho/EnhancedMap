using System;
using System.Drawing;
using EnhancedMap.Core.Network;
using EnhancedMap.Properties;

namespace EnhancedMap.Core.MapObjects
{
    public class ZoomMask : RenderObject
    {
        private float _lastZoom;

        private ZoomMask() : base("zoom")
        {
            IsVisible = false;
        }

        public static ZoomMask ZoomInstance { get; } = new ZoomMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (_lastZoom != Global.Zoom)
            {
                LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(3));
                IsVisible = true;
                _lastZoom = Global.Zoom;
            }

            if (IsEndOfLife) IsVisible = false;

            if (!IsVisible)
                return false;

            string zoom = string.Format("{0}%", _lastZoom * 100);
            SizeF size = g.MeasureString(zoom, Font);
            g.FillRectangle(ColorsCache["blackbackground"], canvasW - size.Width - 4, canvasH - size.Height - 4, size.Width, size.Height);
            g.DrawStringWithBorder(zoom, canvasW - size.Width - 4, canvasH - size.Height - 4, Brushes.Yellow, Font);

            return true;
        }
    }

    public class CoordinatesMask : RenderObject
    {
        private CoordinatesMask() : base("coordinates")
        {
            IsVisible = false;
        }

        public static CoordinatesMask CoortinatesInstance { get; } = new CoordinatesMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible && !Global.SettingsCollection["showcoordinates"].ToBool())
                return false;

            int xLong = 0, yLat = 0, xMins = 0, yMins = 0;
            bool xEast = false, ySouth = false;

            if (Utility.FormatCoordinates(new Point((int) Global.X, (int) Global.Y), Global.Maps[Global.Facet], ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
            {
                string locString = string.Format("Center: {0}°{1}'{2} {3}°{4}'{5} | ({6},{7})", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W", (int) Global.X, (int) Global.Y);
                if (Global.FreeView && Utility.FormatCoordinates(new Point(MouseManager.Location.X, MouseManager.Location.Y), Global.Maps[Global.Facet], ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth)) locString += string.Format("\r\nMouse: {0}°{1}'{2} {3}°{4}'{5} | ({6},{7})", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W", MouseManager.Location.X, MouseManager.Location.Y);

                SizeF size = g.MeasureString(locString, Font);

                g.FillRectangle(ColorsCache["blackbackground"], new RectangleF(canvasW / 2 - size.Width / 2 - 2, 2, size.Width + 4, size.Height + 4));
                g.DrawStringWithBorder(locString, canvasW / 2 - size.Width / 2, 4, Brushes.Yellow, Font);
            }

            return false;
        }
    }

    public class ConnectionMask : RenderObject
    {
        private ConnectionMask() : base("connection")
        {
            IsVisible = false;

            SocketClient.Connected += (sender, e) =>
            {
                IsVisible = true;
                LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(3));
            };

            SocketClient.Disconnected += (sender, e) =>
            {
                IsVisible = true;
                LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(3));
            };

            SocketClient.Waiting += (sender, e) => IsVisible = true;
        }

        public static ConnectionMask ConnectionInstance { get; } = new ConnectionMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible)
                return false;

            if (IsEndOfLife)
            {
                IsVisible = false;
                return false;
            }

            if (NetworkManager.SocketClient.IsConnected && NetworkManager.SocketClient.IsRunning)
                g.DrawImage(Resources.right, 5, canvasH - 30, Resources.right.Width, Resources.right.Height);
            else if (NetworkManager.SocketClient.IsRunning && !NetworkManager.SocketClient.IsConnected)
            {
                g.DrawWait(12, canvasH - 12, -5, -10, Color.White);
                return true;
            }
            else
                g.DrawImage(Resources.wrong, 5, canvasH - 30, Resources.right.Width, Resources.right.Height);

            return false;
        }
    }

    public class FPSMask : RenderObject
    {
        private int _fps; // the FPS calculated from the last measurement
        private int _framesRendered; // an increasing count
        private DateTime _lastTime; // marks the beginning the measurement began

        private FPSMask() : base("fpsmask")
        {
            Font = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        public static FPSMask FPSInstance { get; } = new FPSMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible || !Global.SettingsCollection["showfps"].ToBool())
                return false;

            _framesRendered++;

            if (DateTime.Now > _lastTime)
            {
                _fps = _framesRendered;
                _framesRendered = 0;
                _lastTime = DateTime.Now.AddSeconds(1);
            }

            g.DrawString("FPS: " + _fps, Font, Brushes.Yellow, 5, 5);

            return false;
        }
    }

    public class PanicMask : RenderObject
    {
        private DateTime _last;
        private readonly Pen _pen;
        private bool _render;

        private PanicMask() : base("panicmask")
        {
            _pen = new Pen(Brushes.Red, 5);
            IsVisible = false;
        }

        public static PanicMask PanicInstance { get; } = new PanicMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible)
                return false;

            if (_render)
                g.DrawRectangle(_pen, new Rectangle(1, 1, canvasW - 2, canvasH - 2));

            if (_last < DateTime.Now)
            {
                _last = DateTime.Now.AddSeconds(1);
                _render = !_render;
            }

            return true;
        }
    }

    public class RoseMask : RenderObject
    {
        private RoseMask() : base("panicmask")
        {
        }

        public static RoseMask RoseInstance { get; } = new RoseMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible)
                return false;
            g.DrawImageUnscaled(Resources.rose, canvasW - 35, 5);
            return false;
        }
    }

    public class CrossMask : RenderObject
    {
        private readonly Pen _pen = new Pen(Brushes.Yellow);

        private CrossMask() : base("crossmask")
        {
        }

        public static CrossMask CorssInstance { get; } = new CrossMask();

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible || !Global.FreeView)
                return false;

            g.DrawLine(_pen, x, y - 3, x, y + 3);
            g.DrawLine(_pen, x - 3, y, x + 3, y);

            return false;
        }
    }
}