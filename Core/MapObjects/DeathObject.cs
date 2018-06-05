using EnhancedMap.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class DeathObject : RenderObject
    {
        private LabelObject _label = new LabelObject()
        {
            Background = ColorsCache["blackbackground"],
            Hue = ColorsCache["stamina"],
        };

        private Image _img;

        public DeathObject(UserObject parent, short x, short y, byte map) : base("death")
        {
            UpdatePosition(x, y);
            LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(15));
            Parent = parent;
            Map = parent.Map;

            _img = new Bitmap(Resources.tombstone, 20, 20);
        }

        public DeathObject(UserObject parent, Position position, byte map) : this(parent, position.X, position.Y, map)
        {

        }

        public UserObject Parent { get; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (IsDisposing || Parent == null || Parent.IsDisposing || Map != Global.Facet)
                return false;

            int gameX = RelativePosition.X;
            int gameY = RelativePosition.Y;

            (gameX, gameY) = Geometry.RotatePoint(gameX, gameY, Global.Zoom, 1, Global.Angle);
            AdjustPosition(gameX, gameY, x - 4, y - 4, out int relativeX, out int relativeY);

            bool inwindow = gameX == relativeX && gameY == relativeY;
            gameX = relativeX; gameY = relativeY;

            relativeX = gameX + x;
            relativeY = gameY + y;

            if (Parent is PlayerObject &&  Global.SettingsCollection["trackdeathpoint"].ToBool())
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int playerX = Global.PlayerInstance.RelativePosition.X;
                int playerY = Global.PlayerInstance.RelativePosition.Y;

               (playerX, playerY) = Geometry.RotatePoint(playerX, playerY, Global.Zoom, 1, Global.Angle);
                AdjustPosition(playerX, playerY, x - 4, y - 4, out int plRelX, out int plRelY);

                playerX = plRelX; playerY = plRelY;
                plRelX = playerX + x;
                plRelY = playerY + x;

                g.DrawLine(Pens.White, x + gameX, y + gameY, x + playerX,  y + playerY);
                g.ResetTransform();
            }

            bool result = false;

            if (!IsEndOfLife || Parent.IsDead)
            {
                //Image img = Resources.tombstone;

                int wx = _img.Width / 2;
                int wy = _img.Height / 2;

                g.DrawImage(_img, x + gameX - wx, y + gameY - wy, _img.Width, _img.Height);

                wx = (int)(wx / Global.Zoom);
                wy = (int)(wy / Global.Zoom);

                if (MouseManager.Location.X >= Position.X - wx && MouseManager.Location.X <= Position.X + wx &&
                          MouseManager.Location.Y >= Position.Y - wy && MouseManager.Location.Y <= Position.Y + wy && MouseManager.IsEnter)
                {
                    if (!_label.IsVisible)
                    {
                        _label.IsVisible = true;
                    }

                    _label.UpdatePosition(Position.X + (int)(25 / Global.Zoom), Position.Y + (int)(25 / Global.Zoom));
                    _label.Text = $"{Parent.Name}'s tombstone\r\n  -Remain: {LifeTime.Subtract(DateTime.Now.TimeOfDay):hh\\:mm\\:ss}"; //string.Format("{0}:{1}", LifeTime.Minutes - DateTime.Now.Minute, LifeTime.Seconds - DateTime.Now.Second);
                    _label.Render(g, x, y, canvasW, canvasH);
                    result = true;
                }
                else if (_label.IsVisible)
                    _label.IsVisible = false;
            }
            else
            {
                Dispose();
            }
            return result;
        }
    }
}
