using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public class CustomProgressBar : ProgressBar
    {
        private System.Windows.Forms.Timer _timer;
        public CustomProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            _timer = new System.Windows.Forms.Timer
            {
                Interval = 1000 / 144
            };

            _timer.Tick += (sender, e) =>
            {
                if (this.Style == ProgressBarStyle.Marquee)
                {
                    this.Invalidate();
                }
            };
        }

        private SolidBrush _color = new SolidBrush(ColorsTable.Black1);
        private int _index = -1;

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
                    rec.Width = (int)(rec.Width * ((double)Value / Maximum));
                    e.Graphics.FillRectangle(_color, 0, 0, rec.Width, rec.Height);
                    break;
                case ProgressBarStyle.Marquee:
                    if (!_timer.Enabled && !DesignMode)
                        _timer.Start();

                    _index++;
                    if (_index > e.ClipRectangle.Width)
                        _index = 0;

                    rec.X = (_index % rec.Width);                       
                    rec.Width = (int)(rec.Width * ((double)35 / Maximum));

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
