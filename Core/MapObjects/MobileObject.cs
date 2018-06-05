using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class MobileObject : RenderObject
    {
        public MobileObject(uint serial) : base("0x" + serial.ToString("X"))
        {
            Serial = serial;
            Map = Global.PlayerInstance.Map;
        }

        public uint Serial { get; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (IsDisposing || Map != Global.Facet)
                return false;

            if (Global.PlayerInstance.Position.DistanceTo(Position) > 24)
            {
                Dispose();
                return false;
            }

            int gameX = RelativePosition.X;
            int gameY = RelativePosition.Y;

            (gameX, gameY) = Geometry.RotatePoint(gameX, gameY, Global.Zoom, 1, Global.Angle);

            g.FillRectangle(Brushes.Red, gameX + x - 2, gameY + y - 2, 4, 4);

            return false;
        }
    }
}
