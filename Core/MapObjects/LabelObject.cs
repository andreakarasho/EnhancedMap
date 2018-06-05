using EnhancedMap.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public class LabelObject : RenderObject
    {
        public LabelObject(short x, short y, string text = ""): base("label")
        {
            UpdatePosition(x, y);
            Text = text;
            Background = ColorsCache["transparent"];
            Hue = new SolidBrush(Color.Cyan);
        }

        public LabelObject() : base("label")
        {
            IsVisible = false;
            Background = ColorsCache["blackbackground"];
            Hue = new SolidBrush(Color.Cyan);
        }

        public string Text { get; set; }
        public SolidBrush Background { get; set; }

        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (!IsVisible)
                return false;

            int labelX = RelativePosition.X;
            int labelY = RelativePosition.Y;

            (labelX, labelY) = Geometry.RotatePoint(labelX, labelY, Global.Zoom, 1, Global.Angle);
            labelX += canvasW / 2;
            labelY += canvasH / 2;

            SizeF textSize = g.MeasureString(Text, Font);

            labelX -= (int)textSize.Width / 2;
            labelY -= 20;
            if (labelX < 4) labelX = 4;
            if (labelY < 4) labelY = 4;
            if (labelX + (int)textSize.Width >= canvasW - 4)
                labelX = canvasW - 4 - (int)textSize.Width;
            if (labelY + (int)textSize.Height >= canvasH - 4)
                labelY = canvasH - 4 - (int)textSize.Height;

            g.FillRectangle(Background, labelX - 2, labelY - 2, (int)textSize.Width + 4, (int)textSize.Height + 4);
            g.DrawStringWithBorder(Text, labelX, labelY, Hue, Font);

            return false;
        }
    }
}
