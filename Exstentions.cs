using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace EnhancedMap
{
    public static class Exstentions
    {
        private static readonly int[][] _Trs = {new[] {0, 35, 70, 100, 150, 180, 210, 250}, new[] {250, 0, 35, 70, 100, 150, 180, 210}, new[] {210, 250, 0, 35, 70, 100, 150, 180}, new[] {180, 210, 250, 0, 35, 70, 100, 150}, new[] {150, 180, 210, 250, 0, 35, 70, 100}, new[] {100, 150, 180, 210, 250, 0, 35, 70}, new[] {70, 100, 150, 180, 210, 250, 0, 35}, new[] {35, 70, 100, 150, 180, 210, 250, 0}};

        private static int _Count;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int GlobalGetAtomName(int nAtom, StringBuilder lpBuffer, int nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int GlobalDeleteAtom(int nAtom);

        public static string FromAtom(this string s, int atom)
        {
            StringBuilder t = new StringBuilder(512);
            GlobalGetAtomName(atom, t, t.Capacity);
            GlobalDeleteAtom(atom);
            return t.ToString();
        }

        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public static int PercentageToColorComponent(this int percentage)
        {
            return (int) (percentage / 100d * 255d);
        }

        public static void Do<TControl>(this TControl control, Action<TControl> action) where TControl : Control
        {
            if (control.InvokeRequired)
                control.Invoke(action, control);
            else
                action(control);
        }

        public static Color ToColor(this int argb)
        {
            return Color.FromArgb((argb & 0xff0000) >> 16, (argb & 0xff00) >> 8, argb & 0xff);
        }

        public static void Raise(this EventHandler handler, object sender = null)
        {
            handler?.Invoke(sender, EventArgs.Empty);
        }

        public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            handler?.Invoke(sender, e);
        }

        public static void RaiseAsync(this EventHandler handler, object sender = null)
        {
            Task.Run(() => handler?.Invoke(sender, EventArgs.Empty));
        }

        public static void RaiseAsync<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            Task.Run(() => handler?.Invoke(sender, e));
        }

        public static byte ToByte(this object o)
        {
            return Convert.ToByte(o);
        }

        public static sbyte ToSByte(this object o)
        {
            return Convert.ToSByte(o);
        }

        public static short ToShort(this object o)
        {
            return Convert.ToInt16(o);
        }

        public static ushort ToUShort(this object o)
        {
            return Convert.ToUInt16(o);
        }

        public static int ToInt(this object o)
        {
            return Convert.ToInt32(o);
        }

        public static uint ToUInt(this object o)
        {
            return Convert.ToUInt32(o);
        }

        public static bool ToBool(this object o)
        {
            return Convert.ToBoolean(o);
        }

        public static string ToText(this XmlElement node, string attr, string defaultvalue = "")
        {
            if (!node.HasAttribute(attr))
                return defaultvalue;
            return node.GetAttribute(attr);
        }

        public static void DrawWait(this Graphics g, int x, int y, int width, int heigh, Color color)
        {
            _Count++;
            if (_Count > _Trs.Length) _Count = 0;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(x, y);

            Pen p = null;
            for (int i = 0; i < 8; i++)
            {
                p = new Pen(Color.FromArgb(_Trs[_Count % 8][i], color), 2) {StartCap = LineCap.Round, EndCap = LineCap.Round};

                g.DrawLine(p, 0, -5, 0, -10);
                g.RotateTransform(45);
            }

            g.ResetTransform();
            p?.Dispose();
        }

        public static void DrawStringWithBorder(this Graphics g, string text, float x, float y, Brush color, Font font)
        {
            g.DrawString(text, font, Brushes.Black, x + 1, y + 1);
            g.DrawString(text, font, color, x, y);
        }

        public static void FillRectangleWithBorder(this Graphics g, RectangleF rectangle, Brush color)
        {
            g.FillRectangle(Brushes.Black, rectangle.X + 1, rectangle.Y + 1, rectangle.Width, rectangle.Height);
            g.FillRectangle(color, rectangle);
        }

        public static void FillRectangleWithBorder(this Graphics g, float x, float y, float width, float height, Brush color)
        {
            g.FillRectangle(Brushes.Black, x + 1, y + 1, width, height);
            g.FillRectangle(color, x, y, width, height);
        }

        public static void Clear<T>(this T[] array) where T : class
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = null;
        }

        public static void ForEach<T>(this T[] array, Action<T> action) where T : class
        {
            foreach (T a in array)
            {
                if (a != null)
                    action(a);
            }
        }
    }
}