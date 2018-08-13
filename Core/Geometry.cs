using System;
using System.Drawing;

namespace EnhancedMap.Core
{
    public static class Geometry
    {
        public static int Distance(int fx, int fy, int tx, int ty)
        {
            var xDelta = Math.Abs(fx - tx);
            var yDelta = Math.Abs(fy - ty);
            return xDelta > yDelta ? xDelta : yDelta;
        }

        public static int Distance(Point p1, Point p2)
        {
            var xDelta = Math.Abs(p1.X - p2.X);
            var yDelta = Math.Abs(p1.Y - p2.Y);
            return xDelta > yDelta ? xDelta : yDelta;
        }

        public static bool LineIntersectsLine(Point l1P1, Point l1P2, Point l2P1, Point l2P2)
        {
            float q = (l1P1.Y - l2P1.Y) * (l2P2.X - l2P1.X) - (l1P1.X - l2P1.X) * (l2P2.Y - l2P1.Y);
            float d = (l1P2.X - l1P1.X) * (l2P2.Y - l2P1.Y) - (l1P2.Y - l1P1.Y) * (l2P2.X - l2P1.X);

            if (d == 0) return false;

            var r = q / d;

            q = (l1P1.Y - l2P1.Y) * (l1P2.X - l1P1.X) - (l1P1.X - l2P1.X) * (l1P2.Y - l1P1.Y);
            var s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1) return false;

            return true;
        }

        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) || LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) || LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) || LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) || r.Contains(p1) && r.Contains(p2);
        }

        public static (int, int) RotatePoint(int x, int y, float zoom, int dist, float angle = 45f)
        {
            x = (int) (x * zoom);
            y = (int) (y * zoom);

            if (angle == 0)
                return (x, y);

            return ((int) Math.Round(Math.Cos(dist * Math.PI / 4.0) * x - Math.Sin(dist * Math.PI / 4.0) * y), (int) Math.Round(Math.Sin(dist * Math.PI / 4.0) * x + Math.Cos(dist * Math.PI / 4.0) * y));
        }

        public static (float, float) RotatePoint(float x, float y, float zoom, int dist, float angle = 45f)
        {
            x = x * zoom;
            y = y * zoom;

            if (angle == 0)
                return (x, y);

            return ((float) Math.Round(Math.Cos(dist * Math.PI / 4.0) * x - Math.Sin(dist * Math.PI / 4.0) * y), (float) Math.Round(Math.Sin(dist * Math.PI / 4.0) * x + Math.Cos(dist * Math.PI / 4.0) * y));
        }
    }
}