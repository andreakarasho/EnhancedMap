using EnhancedMap.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class BuildingObject : RenderObject
    {
        public BuildingObject(BuildingEntry entry) : base(entry.Description)
        {
            Entry = entry;
            UpdatePosition(entry.Location);
            IsVisible = true;
        }

        public BuildingEntry Entry { get; }
        public bool IsMouseOver { get; private set; }

        private LabelObject _label = new LabelObject()
        {
            Background = ColorsCache["transparent"],
            Hue = ColorsCache["stamina"],
        };


        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible)
                return false;

            if ((Entry.Map == Global.Facet || (Entry.Map == 7 && (Global.Facet == 0 || Global.Facet == 1))) &&
                Entry.IsEnabled && Entry.Parent.IsEnabled)
            {

                if (Entry.Parent.IsSmart && Global.Zoom < 1f && Global.SettingsCollection["hidelessimportantplaces"].ToBool())
                {
                    return false;
                }

                int buildX = RelativePosition.X;
                int buildY = RelativePosition.Y;

                (buildX, buildY) = Geometry.RotatePoint(buildX, buildY, Global.Zoom, 1, Global.Angle);
                buildX += canvasW / 2;
                buildY += canvasH / 2;

                Image img = Entry.Parent.Image;

                if (buildX > -img.Width / 2 && buildX - img.Width / 2 < canvasW
                    && buildY > -img.Height / 2 && buildY - img.Height / 2 < canvasH)
                {
                    int wx = img.Width / 2;
                    int wy = img.Height / 2;

                    g.DrawImageUnscaled(img, buildX - wx, buildY - wy);
                    wx = (int)(wx / Global.Zoom);
                    wy = (int)(wy / Global.Zoom);

                    if (Entry.ShowName || (Entry.IsTown && Global.SettingsCollection["showtownsnames"].ToBool()))
                    {
                        if (!_label.IsVisible)
                        {
                            _label.IsVisible = true;
                            _label.UpdatePosition(Entry.Location);
                            _label.Text = Entry.Description;
                        }

                        _label.Render(g, x, y, canvasW, canvasH);
                    }
                    else if (_label.IsVisible)
                    {
                        _label.IsVisible = false;
                    }


                    if (MouseManager.Location.X >= Position.X - wx && MouseManager.Location.X <= Position.X + wx &&
                        MouseManager.Location.Y >= Position.Y - wy && MouseManager.Location.Y <= Position.Y + wy && MouseManager.IsEnter)
                    {
                        if (!IsMouseOver)
                            IsMouseOver = true;
                        if (Global.CurrentBuildObject != this)
                        {
                            Global.CurrentBuildObject = this;
                            Global.CurrentLabelObject.UpdatePosition(Entry.Location);
                            Global.CurrentLabelObject.Text = Entry.Description;
                            if (!Global.CurrentLabelObject.IsVisible)
                                Global.CurrentLabelObject.IsVisible = true;
                        }

                        return true;
                    }
                    else
                    {
                        if (IsMouseOver)
                            IsMouseOver = false;
                    }

                }
            }

            return false;
        }
    }

    public class HouseObject : RenderObject
    {
        public HouseObject(HouseEntry entry) : base(entry.Description)
        {
            Entry = entry;
            UpdatePosition(entry.Location);
            IsVisible = true;
        }


        public HouseEntry Entry { get; }
        public bool IsMouseOver { get; private set; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible || Entry.Map != Global.Facet || !Global.SettingsCollection["showhouses"].ToBool())
                return false;

            /*int buildX = RelativePosition.X;
            int buildY = RelativePosition.Y;

            Geometry.RotatePoint(ref buildX, ref buildY, Global.Zoom, 1);
            buildX += canvasW / 2;
            buildY += canvasH / 2;

            int w = (Position.X + Entry.Size.Width) - (int)Global.X;
            int h = (Position.Y + Entry.Size.Height) - (int)Global.Y;

            Geometry.RotatePoint(ref w, ref h, Global.Zoom, 1);
            w += canvasW / 2;
            h += canvasH / 2;*/

            int delta = Math.Max(canvasW, canvasH) * 2;

            int aa = (int)Global.X - canvasW / 2;
            int bb = (int)Global.Y - canvasH / 2;

            int bx = (int)((Entry.Location.X - aa - canvasW / 2) / (1f / Global.Zoom));
            int by = (int)((Entry.Location.Y - bb - canvasH / 2) / (1f / Global.Zoom));

            RectangleF houseRect = new RectangleF(bx, by, Entry.Size.Width * Global.Zoom, Entry.Size.Height * Global.Zoom);
            if (Utility.Distance(Entry.Location.X, Entry.Location.Y, (int)Global.X, (int)Global.Y) < delta / Global.Zoom)
            {
                g.FillRectangle(Brushes.Gray, houseRect);
                g.DrawRectangle(Pens.Black, bx - 1, by - 1, Entry.Size.Width * Global.Zoom + 2, Entry.Size.Height * Global.Zoom + 2);
            }

            int mouseX = (int)((MouseManager.Location.X - aa - canvasW / 2) / (1f / Global.Zoom));
            int mouseY = (int)((MouseManager.Location.Y - bb - canvasH / 2) / (1f / Global.Zoom));

            if (houseRect.Contains(mouseX, mouseY))
            {
                Global.CurrentHouseObject = this;
                Global.CurrentLabelObject.UpdatePosition(Entry.Location);
                Global.CurrentLabelObject.Text = Entry.Description;
                if (!Global.CurrentLabelObject.IsVisible)
                    Global.CurrentLabelObject.IsVisible = true;
                /*if (!MouseManager.IsOverAnObject)
                    MouseManager.IsOverAnObject = true;*/
                if (!IsMouseOver)
                    IsMouseOver = true;
                return true;
            }
            else //if (MouseManager.IsOverAnObject)
            {
                //MouseManager.IsOverAnObject = false;
                if (IsMouseOver)
                    IsMouseOver = false;
            }
            /*/ int rX = -(int)(canvasW * 1.75f);
             int rY = -(int)(canvasH * 1.75f);
             int rW = (int)( )
             */
            //if (bx > -canvasW * 1.75f && bx + Entry.Size.Height * Global.Zoom < )



            return false;
        }
    }

    public class GuardLineObject : RenderObject
    {
        public GuardLineObject(GuardlineEntry entry) : base("guardline")
        {
            Entry = entry;
            UpdatePosition(entry.Location);
            IsVisible = true;
        }

        public GuardlineEntry Entry { get; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible || !Global.SettingsCollection["showguardlines"].ToBool())
                return false;

            if ((Entry.Map == Global.Facet || (Entry.Map == 7 && (Global.Facet == 0 || Global.Facet == 1))))
            {
                int delta = Math.Max(canvasW, canvasH) * 2;

                int aa = (int)Global.X - canvasW / 2;
                int bb = (int)Global.Y - canvasH / 2;

                int bx = (int)((Entry.Location.X - aa - canvasW / 2) / (1f / Global.Zoom));
                int by = (int)((Entry.Location.Y - bb - canvasH / 2) / (1f / Global.Zoom));

                if (Utility.Distance(Entry.Location.X, Entry.Location.Y, (int)Global.X, (int)Global.Y) < delta / Global.Zoom)
                {
                    g.DrawRectangle(Pens.Green, bx, by , Entry.Size.Width * Global.Zoom , Entry.Size.Height * Global.Zoom );
                }
            }

            return false;
        }
    }
}
