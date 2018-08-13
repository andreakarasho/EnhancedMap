using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public class CustomProgressBar : ProgressBar
    {
        private readonly SolidBrush _color = new SolidBrush(ColorsTable.Black1);
        private int _index = -1;
        private readonly System.Windows.Forms.Timer _timer;

        public CustomProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            _timer = new System.Windows.Forms.Timer {Interval = 1000 / 144};

            _timer.Tick += (sender, e) =>
            {
                if (Style == ProgressBarStyle.Marquee) Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);

            switch (Style)
            {
                case ProgressBarStyle.Blocks:
                    if (_timer.Enabled && !DesignMode)
                        _timer.Stop();
                    rec.Width = (int) (rec.Width * ((double) Value / Maximum));
                    e.Graphics.FillRectangle(_color, 0, 0, rec.Width, rec.Height);
                    break;
                case ProgressBarStyle.Marquee:
                    if (!_timer.Enabled && !DesignMode)
                        _timer.Start();

                    _index++;
                    if (_index > e.ClipRectangle.Width)
                        _index = 0;

                    rec.X = _index % rec.Width;
                    rec.Width = (int) (rec.Width * ((double) 35 / Maximum));

                    int delta = Math.Max(0, rec.Right - e.ClipRectangle.Width);

                    e.Graphics.FillRectangle(_color, rec.X, 0, rec.Width, rec.Height);
                    if (delta > 0)
                        e.Graphics.FillRectangle(_color, 0, 0, delta, rec.Height);
                    break;
                default:
                    break;
            }
        }
    }
}