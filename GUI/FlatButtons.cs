using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    class CustomFlatButton : Button, ICustomControl
    {
        private SizeF _textSize;
        private bool _forceHover;


        public CustomFlatButton()
        {
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            //AutoSize = true;
            Margin = new Padding(4, 6, 4, 6);
            Padding = new Padding(0);
        }

        public bool ForceHover
        {
            get => _forceHover;
            set
            {
                _forceHover = value;
                IsPressed = value;
                Invalidate();
            }
        }

        private Image _icon;
        public Image Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                if (AutoSize)
                    Size = GetPreferredSize();
                Invalidate();
            }
        }
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                _textSize = CreateGraphics().MeasureString(value.ToUpper(), Font);
                if (AutoSize)
                    Size = GetPreferredSize();
                Invalidate();
            }
        }

        public MouseState MouseState { get; set; }
        public bool IsPressed
        {
            get => MouseState == MouseState.DOWN;
            set
            {
                MouseState = value ? MouseState.DOWN : MouseState.OUT;
            }
        }
        public bool IsHover
        {
            get => MouseState == MouseState.HOVER;
            set
            {
                MouseState = value ? MouseState.HOVER : MouseState.OUT;
            }
        }

        public bool IsSelected { get; set; }

        private Size GetPreferredSize()
        {
            return GetPreferredSize(new Size(0, 0));
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            // Provides extra space for proper padding for content
            var extra = 16;

            if (Icon != null)
                // 24 is for icon size
                // 4 is for the space between icon & text
                extra += 24 + 4;

            return new Size((int)Math.Ceiling(_textSize.Width) + extra, 36);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;

            g.Clear(Parent.BackColor);

            if (MouseState == MouseState.HOVER)
                g.FillRectangle(new SolidBrush(Color.FromArgb(50.PercentageToColorComponent(), 0x999999.ToColor())), ClientRectangle);
            else if (MouseState == MouseState.DOWN || IsSelected)
                g.FillRectangle(new SolidBrush(Color.FromArgb(20.PercentageToColorComponent(), 0x999999.ToColor())), ClientRectangle);


            var iconRect = new Rectangle(8, 6, 24, 24);
            if (string.IsNullOrEmpty(Text))
                iconRect.X += 2;
            if (Icon != null)
                g.DrawImage(Icon, iconRect);

            var textRect = ClientRectangle;
            if (Icon != null)
            {
                textRect.Width -= 8 + 24 + 4 + 8;
                textRect.X += 8 + 24 + 4;
            }

            g.DrawString(Text, Font, Enabled ? new SolidBrush(ForeColor) : new SolidBrush(Color.LightGray), textRect, new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            MouseState = MouseState.OUT;
            MouseEnter += (sender, e) =>
            {
                if (!ForceHover)
                    MouseState = MouseState.HOVER;
                Invalidate();
            };

            MouseLeave += (sender, e) =>
            {
                if (!ForceHover)
                    MouseState = MouseState.OUT;
                Invalidate();
            };

            MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    MouseState = MouseState.DOWN;
                    Invalidate();
                }
            };

            MouseUp += (sender, e) =>
            {
                if (!ForceHover)
                    MouseState = MouseState.HOVER;
                Invalidate();
            };

        }
    }
}
