using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnhancedMap.GUI.MapMakerSteps
{
    public partial class KindClientStep : UserControl
    {
        private readonly Font _font = new Font("Arial", 20, FontStyle.Regular);

        private readonly bool[] _isHover = new bool[2] {false, false};

        public KindClientStep(Action<int> nextAction)
        {
            InitializeComponent();

            pictureBoxClassic.Paint += OnPaintPictureBox;
            pictureBoxCustom.Paint += OnPaintPictureBox;


            pictureBoxClassic.MouseEnter += (sender, e) =>
            {
                _isHover[0] = true;
                pictureBoxClassic.Invalidate();
            };

            pictureBoxClassic.MouseLeave += (sender, e) =>
            {
                _isHover[0] = false;
                pictureBoxClassic.Invalidate();
            };

            pictureBoxClassic.Click += (sender, e) => { nextAction(0); };


            pictureBoxCustom.MouseEnter += (sender, e) =>
            {
                _isHover[1] = true;
                pictureBoxCustom.Invalidate();
            };

            pictureBoxCustom.MouseLeave += (sender, e) =>
            {
                _isHover[1] = false;
                pictureBoxCustom.Invalidate();
            };

            pictureBoxCustom.Click += (sender, e) => { nextAction(1); };


            pictureBoxClassic.Invalidate();
            pictureBoxCustom.Invalidate();
        }

        private void OnPaintPictureBox(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            PictureBox pic = (PictureBox) sender;

            bool isclassic = pic == pictureBoxClassic;

            Color color = ColorsTable.Black4;
            if (isclassic)
            {
                if (_isHover[0])
                    color = ColorsTable.Black1;
            }
            else
            {
                if (_isHover[1])
                    color = ColorsTable.Black1;
            }

            g.FillRectangle(new SolidBrush(Color.FromArgb(70.PercentageToColorComponent(), color)), pic.DisplayRectangle);

            string title = isclassic ? "Original Maps" : "Custom Maps";
            SizeF size = g.MeasureString(title, _font);

            g.DrawString(title, _font, Brushes.White, pic.DisplayRectangle.Width / 2 - size.Width / 2, pic.DisplayRectangle.Height / 2 - size.Height);
        }
    }
}