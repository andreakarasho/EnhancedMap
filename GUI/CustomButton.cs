using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public class CustomButton : Button, ICustomControl
    {
        private const int ITEM_HEIGHT = 30;

        //private static readonly Font _font = new Font(Control.DefaultFont.Name, 10, FontStyle.Regular);
        private static SolidBrush _baseColor = new SolidBrush(ColorsTable.Black0); // new SolidBrush(Color.FromArgb(49, 49, 49));
        private static readonly SolidBrush _pressedColor = new SolidBrush(ColorsTable.Black2); // new SolidBrush(Color.Gray);
        private static readonly SolidBrush _hoverColor = new SolidBrush(ColorsTable.Black1);
        private bool _forceHover;

        private SizeF _textLen;

        public CustomButton()
        {
            BackColor = ColorsTable.Black0;
        }

        public bool IsPressed { get; set; }
        public bool IsHover { get; set; }

        public bool ForceHover
        {
            get => _forceHover;
            set
            {
                _forceHover = value;
                IsHover = value;
            }
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                _textLen = CreateGraphics().MeasureString(value, Font);
            }
        }

        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                _textLen = CreateGraphics().MeasureString(Text, value);
            }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        public MouseState MouseState { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (IsPressed)
                g.FillRectangle(_pressedColor, ClientRectangle);
            else if (IsHover)
                g.FillRectangle(_hoverColor, ClientRectangle);
            else if (Enabled)
                g.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            else
                g.FillRectangle(Brushes.DimGray, ClientRectangle);

            if (Image != null)
            {
                switch (ImageAlign)
                {
                    case ContentAlignment.MiddleCenter:
                        g.DrawImage(Image, Width / 2 - Image.Width / 2, Height / 2 - Image.Width / 2, Image.Width, Image.Height);
                        break;
                    default:
                    case ContentAlignment.MiddleLeft:
                        g.DrawImage(Image, 4, Height / 2 - Image.Height / 2, Image.Width, Image.Height);
                        break;
                }
            }

            //  g.DrawRectangle(IsHover ? Pens.Red : Pens.Gray, ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

            g.DrawString(Text, Font, Brushes.White, ClientRectangle /*new RectangleF(ClientRectangle.Width / 2 - _textLen.Width / 2, ClientRectangle.Height / 2 - _textLen.Height / 2, Width, Height)*/, new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center});
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!ForceHover)
                IsHover = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!ForceHover)
                IsHover = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            IsPressed = true;
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            IsPressed = false;
            base.OnMouseUp(mevent);
        }
    }
}