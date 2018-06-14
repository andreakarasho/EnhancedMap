using EnhancedMap.GUI;
using EnhancedMap.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class RangeProperty
    {
        public RangeProperty(ushort min, ushort max)
        {
            Min = min;
            Max = max;
        }

        public RangeProperty() : this(0,0)
        { }

        public ushort Min { get; private set; }
        public ushort Max { get; private set; }

        public void Reset()
        {
            Min = 0; Max = 0;
        }

        public void Set(ushort min, ushort max)
        {
            Min = min; Max = max;
        }
    }

    public enum FLAGS_PROPERTY : byte
    {
        NONE = 0x00,
        POISONED,
        YELLOWHITS,
        PARALYZED,
        DEAD,
        HIDDEN,
    }
    
    public class UserObject : RenderObject
    {
        private DateTime _panicTime;
        private bool _panicShow;
        private LabelObject _label = new LabelObject()
        {
            Background = ColorsCache["blackbackground"],
            Hue = ColorsCache["stamina"],
        };

        public UserObject(string name) : base(name)
        {
            Hits = new RangeProperty();
            Stamina = new RangeProperty();
            Mana = new RangeProperty();
            //Flags = 0x00;
            _panicTime = DateTime.MinValue;
            LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(1));

            IsHighlighted = Global.HighlightesUsername.Contains(name);
        }

        public RangeProperty Hits { get;}
        public RangeProperty Stamina { get; }
        public RangeProperty Mana { get; }
        //public byte Flags { get; set; }
        public bool InGame => Position.X > 0 && Position.Y > 0;
        public bool InPanic { get; set; }
        public bool IsDead { get; set; } //=> (Flags & (byte)FLAGS_PROPERTY.DEAD) == (byte)FLAGS_PROPERTY.DEAD;
        public bool IsHidden { get; set; }//=> (Flags & (byte)FLAGS_PROPERTY.HIDDEN) == (byte)FLAGS_PROPERTY.HIDDEN;
        public bool IsParalyzed { get; set; }//=> (Flags & (byte)FLAGS_PROPERTY.PARALYZED) == (byte)FLAGS_PROPERTY.PARALYZED;
        public bool IsYellowHits { get; set; }//=> (Flags & (byte)FLAGS_PROPERTY.YELLOWHITS) == (byte)FLAGS_PROPERTY.YELLOWHITS;
        public bool IsPoisoned { get; set; }//=> (Flags & (byte)FLAGS_PROPERTY.POISONED) == (byte)FLAGS_PROPERTY.POISONED;
        public bool IsHighlighted { get; set; }

        internal DateTime LastPanicUpdate { get; set; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (IsDisposing)
                return false;
            if (IsEndOfLife)
            {
                Dispose();
                return false;
            }

            if (Global.Facet != Map && Global.SettingsCollection["dontrenderusersindifferentfacets"].ToBool())
            {
                return true;
            }

            bool result = false;

            int gameX = RelativePosition.X;
            int gameY = RelativePosition.Y;

            (gameX, gameY) = Geometry.RotatePoint(gameX, gameY, Global.Zoom, 1, Global.Angle);
            AdjustPosition(gameX, gameY, x - 4, y - 4, out int relativeX, out int relativeY);

            bool inwindow = gameX == relativeX && gameY == relativeY;
            gameX = relativeX; gameY = relativeY;

            relativeX = gameX + x;
            relativeY = gameY + y;

            string name = Global.SettingsCollection["abbreviatenames"].ToBool() ?  Name.Substring(0, 3) : Name;

            SizeF sizeName = g.MeasureString(name, Font);
            int smartOffsetX = 0;
            int smartOffsetY = 0;

            bool smartname = Global.SettingsCollection["smartnamesposition"].ToBool();



            int nameposition = smartname ? 0 : Global.SettingsCollection["nameposition"].ToInt();
            // 0 is right
            // 1 is center
            // 2 is left
            switch (nameposition)
            {
                default:
                case 0:
                    break;
                case 1:
                    nameposition = (int)((-sizeName.Width / 2));
                    break;
                case 2:
                    nameposition = (int)((-sizeName.Width));
                    break;
            }



            if (relativeX + nameposition <= 4)
                relativeX = 4 - nameposition;
            else if (relativeX >= canvasW - (int)sizeName.Width - 2 - nameposition)
            {
                relativeX = canvasW - (int)sizeName.Width - 2 - nameposition;
                if (relativeX > x && inwindow && smartname)
                    smartOffsetX = (int)sizeName.Width;
            }

            /*if (relativeY < 4)
                relativeY = 2;*/

            if (relativeY > canvasH - (int)sizeName.Height - 2)
            {
                relativeY = canvasH - (int)sizeName.Height - 2;
                if (relativeY > y && inwindow && smartname)
                    smartOffsetY = (int)sizeName.Height;
            }

            if (smartname && inwindow)
            {
                if (relativeX > x)
                    smartOffsetX -= (int)sizeName.Width;
                if (relativeY > y)
                    smartOffsetY -= (int)sizeName.Height;
            }

            if (InPanic)
            {
                /*if (!PanicMask.PanicInstance.IsVisible)
                    PanicMask.PanicInstance.IsVisible = true;
*/
                if (_panicTime < DateTime.Now)
                {
                    _panicTime = DateTime.Now.AddSeconds(1);
                    _panicShow = !_panicShow;
                }

                if (_panicShow)
                {
                    g.FillRectangle(ColorsCache["redbackground"], relativeX - 1 + smartOffsetX + nameposition, relativeY - 1, sizeName.Width + 1, sizeName.Height + 1);
                }
                result |= true;
            }
            else
            {
               /* if (PanicMask.PanicInstance.IsVisible)
                    PanicMask.PanicInstance.IsVisible = false;
*/
                if (IsHighlighted)
                {
                    g.FillRectangle(ColorsCache["highlighted"], relativeX - 1+ smartOffsetX  + nameposition, relativeY - 1, sizeName.Width + 1, sizeName.Height + 1);
                }
            }
            

            SolidBrush statusColor = Global.Facet == Map ? Hue : ColorsCache["outmap"];

          
            if (inwindow)
            {
                SolidBrush hpColor = null;
                if (IsParalyzed)
                {
                    hpColor = ColorsCache["paralyzed"];
                }
                else if (IsYellowHits)
                {
                    hpColor = ColorsCache["yellowhits"];
                }
                else if (IsPoisoned)
                {
                    hpColor = ColorsCache["poisoned"];
                }
                else
                {
                    hpColor = ColorsCache[Global.Facet == Map ? "hits" : "outmap"];
                }

                int offsetbar = 0;

                if (Global.SettingsCollection["showhits"].ToBool())
                {
                    offsetbar += (int)sizeName.Height + 1;
                    int percHP = (Hits.Min == 0 ? 1 : Hits.Min * 100) / (Hits.Max == 0 ? int.MaxValue : Hits.Max);
                    g.FillRectangleWithBorder(relativeX + 2 + smartOffsetX + nameposition, relativeY + offsetbar + smartOffsetY, 35 * percHP / 100, 2, hpColor);
                }

                if (Global.SettingsCollection["showstamina"].ToBool())
                {
                    offsetbar += offsetbar > 0 ? 3 : (int)sizeName.Height + 1;
                    int percStam = (Stamina.Min == 0 ? 1 : Stamina.Min * 100) / (Stamina.Max == 0 ? int.MaxValue : Stamina.Max);
                    g.FillRectangleWithBorder(relativeX + 2 + smartOffsetX + nameposition, relativeY + offsetbar + smartOffsetY, 35 * percStam / 100, 2, ColorsCache["stamina"]);
                }

                if (Global.SettingsCollection["showmana"].ToBool())
                {
                    offsetbar += offsetbar > 0 ? 3 : (int)sizeName.Height + 1;
                    int percMana = (Mana.Min == 0 ? 1 : Mana.Min * 100) / (Mana.Max == 0 ? int.MaxValue : Mana.Max);
                    g.FillRectangleWithBorder(relativeX + 2 + smartOffsetX + nameposition, relativeY + offsetbar + smartOffsetY, 35 * percMana / 100, 2, ColorsCache["mana"]);
                }

                g.FillRectangleWithBorder(x + gameX - 1, y + gameY - 1, 2, 2, statusColor);

                if (IsDead)
                {
                    if (Global.SettingsCollection["showdeathicon"].ToBool())
                        g.DrawImage(Resources.skull2, relativeX + sizeName.Width + smartOffsetX + nameposition, relativeY - 2 + smartOffsetY + (sizeName.Height / 2 - Resources.skull2.Height / 2), Resources.skull2.Width, Resources.skull2.Height);
                }
                else if (IsHidden)
                {
                    if (Global.SettingsCollection["showhiddenicon"].ToBool())
                        g.DrawImage(Resources.hide, relativeX + sizeName.Width + smartOffsetX + nameposition, relativeY - 2 + smartOffsetY + (sizeName.Height / 2 - Resources.hide.Height / 2), Resources.hide.Width, Resources.hide.Height);
                }
            }

           

            g.DrawStringWithBorder(name, relativeX + nameposition + smartOffsetX, relativeY + smartOffsetY, statusColor, Font);

            if (MouseManager.Location.X >= Position.X - (1 / Global.Zoom)
                && MouseManager.Location.X <= Position.X + (1 / Global.Zoom)
                && MouseManager.Location.Y >= Position.Y - (1 / Global.Zoom)
                && MouseManager.Location.Y <= Position.Y + (1 / Global.Zoom) && MouseManager.IsEnter)
            {
                if (!_label.IsVisible)
                {
                    _label.IsVisible = true;                   
                }
                _label.UpdatePosition(Position.X + (int)(25 / Global.Zoom), Position.Y + (int)(25 / Global.Zoom));
                _label.Text = string.Format("{0}:\r\n-Location: {1}\r\n-Map: {2}\r\n-Status: {3}", Name, Position, Map, GetStatusString());
                _label.Render(g, x, y, canvasW, canvasH);
            }
            else if (_label.IsVisible)
                _label.IsVisible = false;

            return result;
        }

        private string GetStatusString()
        {
            if (IsDead)
                return "Dead";
            if (IsParalyzed)
                return "Paralyzed";
            if (IsYellowHits)
                return "YellowHits";
            if (IsPoisoned)
                return "Poisoned";

            return "N/A";
        }

        public void UpdateLifeTime() => LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(1));
    }
}
