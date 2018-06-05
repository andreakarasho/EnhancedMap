using EnhancedMap.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class MarkerObject : RenderObject
    {
        private static readonly Pen _pen = new Pen(Color.White)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot,
            DashPattern = new float[] { 9, 4, 3, 4, 3, 4 }
        };

        private Image _img;

        public MarkerObject(UserObject parent, int x, int y) : base("marker")
        {
            Parent = parent;
            UpdatePosition(x, y);

            _img = new Bitmap(Resources.pin, 12, 18);
        }

        public UserObject Parent { get; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (Parent == null || Parent.IsDisposing)
                Dispose();

            if (IsDisposing || Map != Global.Facet)
                return false;

            int gameX = RelativePosition.X;
            int gameY = RelativePosition.Y;

            (gameX, gameY) = Geometry.RotatePoint(gameX, gameY, Global.Zoom, 1, Global.Angle);
            AdjustPosition(gameX, gameY, x - 4, y - 4, out int relativeX, out int relativeY);

            gameX = relativeX; gameY = relativeY;

            relativeX = gameX + x;
            relativeY = gameY + y;


            int playerX = Global.PlayerInstance.RelativePosition.X;
            int playerY = Global.PlayerInstance.RelativePosition.Y;

            (playerX, playerY) = Geometry.RotatePoint(playerX, playerY, Global.Zoom, 1, Global.Angle);
           // AdjustPosition(playerX, playerY, x - 4, y - 4, out int plRelX, out int plRelY);
           // bool inwindow = playerX == plRelX && playerY == plRelY;

           // playerX = plRelX; playerY = plRelY;

           // if (inwindow)
           // {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(_pen, relativeX, relativeY, x + playerX, y + playerY);
                g.ResetTransform();
           // }

            g.DrawImage(_img, relativeX - _img.Width / 2, relativeY - _img.Height, _img.Width, _img.Height);

            return false;
        }
    }
}
