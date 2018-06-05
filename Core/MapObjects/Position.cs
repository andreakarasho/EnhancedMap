using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core.MapObjects
{
    public struct Position
    {

        public static Position Invalid = new Position(-1, -1);

        public short X { get; set; }
        public short Y { get; set; }

        public Position(short x, short y)
            : this()
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Position p1, Position p2) { return p1.X == p2.X && p1.Y == p2.Y; }
        public static bool operator !=(Position p1, Position p2) { return p1.X != p2.X || p1.Y != p2.Y; }
        public static Position operator +(Position p1, Position p2) { return new Position((short)(p1.X + p2.X), (short)(p1.Y + p2.Y)); }
        public static Position operator -(Position p1, Position p2) { return new Position((short)(p1.X - p2.X), (short)(p1.Y - p2.Y)); }
        public static Position operator *(Position p1, Position p2) { return new Position((short)(p1.X * p2.X), (short)(p1.Y * p2.Y)); }
        public static Position operator /(Position p1, Position p2) { return new Position((short)(p1.X / p2.X), (short)(p1.Y / p2.Y)); }


        public int DistanceTo(Position position) { return Math.Max(Math.Abs(position.X - X), Math.Abs(position.Y - Y)); }
        public double DistanceToSqrt(Position position)
        {
            int a = position.X - X;
            int b = position.Y - Y;
            return Math.Sqrt(a * a + b * b);
        }

        public override int GetHashCode() { return X ^ Y; }
        public override bool Equals(object obj) { return obj is Position && this == (Position)obj; }
        public override string ToString() { return $"{X}.{Y}"; }
        public static Position Parse(string str)
        {
            string[] args = str.Split('.');
            return new Position(short.Parse(args[0]), short.Parse(args[1]));
        }
    }
}
