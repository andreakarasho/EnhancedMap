using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class SharedLabelObject : RenderObject
    {
        public SharedLabelObject(UserObject parent, short x, short y, byte map, string description) : base("sharedlabel")
        {
            Parent = parent;
            UpdatePosition(x, y);
            Map = map;
            Description = description;
        }

        public UserObject Parent { get; }
        public string Description { get; }
        

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (Parent == null || Parent.IsDisposing)
                Dispose();

            if (IsDisposing|| Global.Facet != Map)
                return false;

            int buildX = RelativePosition.X;
            int buildY = RelativePosition.Y;

            (buildX, buildY) = Geometry.RotatePoint(buildX, buildY, Global.Zoom, 1, Global.Angle);
            buildX += canvasW / 2;
            buildY += canvasH / 2;

            Image img = Properties.Resources.SHAREDLABEL;

            if (buildX > -img.Width / 2 && buildX - img.Width / 2 < canvasW
                   && buildY > -img.Height / 2 && buildY - img.Height / 2 < canvasH)
            {
                int wx = img.Width / 2;
                int wy = img.Height / 2;

                g.DrawImageUnscaled(img, buildX - wx, buildY - wy);
                wx = (int)(wx / Global.Zoom);
                wy = (int)(wy / Global.Zoom);


                if (MouseManager.Location.X >= Position.X - wx && MouseManager.Location.X <= Position.X + wx &&
                           MouseManager.Location.Y >= Position.Y - wy && MouseManager.Location.Y <= Position.Y + wy && !string.IsNullOrEmpty(Description))
                {
                    Global.CurrentLabelObject.UpdatePosition(Position);
                    Global.CurrentLabelObject.Text = Description;
                    if (!Global.CurrentLabelObject.IsVisible)
                        Global.CurrentLabelObject.IsVisible = true;
                    return true;
                }
            }
            return false;
        }
    }
}
