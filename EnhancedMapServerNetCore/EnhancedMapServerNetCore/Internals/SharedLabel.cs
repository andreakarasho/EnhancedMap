namespace EnhancedMapServerNetCore.Internals
{
    public class SharedLabel
    {
        public SharedLabel(in ushort x, in ushort y, in byte map, in string descr)
        {
            X = x;
            Y = y;
            Map = map;
            Description = descr;
        }

        public ushort X { get; }
        public ushort Y { get; }
        public byte Map { get; }
        public string Description { get; }
    }
}