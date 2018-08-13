using System;
using System.Collections.Generic;
using System.Drawing;

namespace EnhancedMap.Core.MapObjects
{
    public interface IRenderObject : IDisposable
    {
        bool IsDisposing { get; set; }
    }

    public abstract class RenderObject : IRenderObject
    {
        private static readonly Font _defaultFont = new Font("Segoe UI", 12, FontStyle.Regular, GraphicsUnit.Pixel);

        private Position _position;

        protected RenderObject(string name)
        {
            Name = name;
            _position = new Position(0, 0);
            Map = 0;
            Hue = new SolidBrush(Color.White);
            Font = _defaultFont;
            Hash = name.MD5();
            IsVisible = true;
            LifeTime = TimeSpan.FromMilliseconds(-1);
        }

        public static Dictionary<string, SolidBrush> ColorsCache { get; } = new Dictionary<string, SolidBrush> {{"poisoned", new SolidBrush(Color.LimeGreen)}, {"yellowhits", new SolidBrush(Color.Yellow)}, {"paralyzed", new SolidBrush(Color.AliceBlue)}, {"hits", new SolidBrush(Color.Red)}, {"stamina", new SolidBrush(Color.Yellow)}, {"mana", new SolidBrush(Color.Blue)}, {"blackbackground", new SolidBrush(Color.FromArgb(150, Color.Black))}, {"redbackground", new SolidBrush(Color.FromArgb(128, Color.Red))}, {"outmap", new SolidBrush(Color.DarkGray)}, {"transparent", new SolidBrush(Color.Transparent)}, {"highlighted", new SolidBrush(Color.FromArgb(80.PercentageToColorComponent(), Color.LightYellow))}};

        public string Name { get; protected set; }
        public Position Position => _position;
        public Position RelativePosition => new Position((short) (_position.X - (int) Global.X), (short) (_position.Y - (int) Global.Y));
        public byte Map { get; set; }
        public string Hash { get; }

        public SolidBrush Hue { get; set; }
        public Font Font { get; set; }
        public bool IsVisible { get; set; }

        /// <summary>
        ///     How many time remain reamain into screen
        /// </summary>
        public TimeSpan LifeTime { get; protected set; }

        public bool IsEndOfLife => LifeTime.TotalMilliseconds != -1 && LifeTime.Subtract(DateTime.Now.TimeOfDay).TotalMilliseconds < 0;
        public bool IsDisposing { get; set; }

        public void Dispose()
        {
            IsDisposing = true;
        }


        public void UpdatePosition(int x, int y)
        {
            _position.X = (short) x;
            _position.Y = (short) y;
        }

        public void UpdatePosition(Position position)
        {
            UpdatePosition(position.X, position.Y);
        }


        public abstract bool Render(Graphics g, int x, int y, int canvasW, int canvasH);

        public override string ToString()
        {
            return Name;
        }

        protected void AdjustPosition(int x, int y, int centerX, int centerY, out int newX, out int newY)
        {
            var offset = GetOffset(x, y, centerX, centerY);
            var currX = x;
            var currY = y;

            while (offset != 0)
            {
                if ((offset & 1) != 0)
                {
                    currY = centerY;
                    currX = x * currY / y;
                }
                else if ((offset & 2) != 0)
                {
                    currY = -centerY;
                    currX = x * currY / y;
                }
                else if ((offset & 4) != 0)
                {
                    currX = centerX;
                    currY = y * currX / x;
                }
                else if ((offset & 8) != 0)
                {
                    currX = -centerX;
                    currY = y * currX / x;
                }

                x = currX;
                y = currY;
                offset = GetOffset(x, y, centerX, centerY);
            }

            newX = x;
            newY = y;
        }

        private int GetOffset(int x, int y, int centerX, int centerY)
        {
            const int offset = 0;
            if (y > centerY)
                return 1;
            if (y < -centerY)
                return 2;
            if (x > centerX)
                return offset + 4;
            if (x >= -centerX)
                return offset;
            return offset + 8;
        }
    }
}