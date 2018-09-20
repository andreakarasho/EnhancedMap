using System;

namespace EnhancedMapServerNetCore.Network
{
    public class PacketWriter : PacketBase
    {
        private byte[] _data;

        public PacketWriter(byte id)
        {
            _data = new byte[3];
            _data[0] = id;
            Position = 3;
        }

        protected override byte this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public override byte ID => this[0];
        public override int Length => _data.Length;

        protected override void EnsureSize(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            while (Position + length > Length)
                Array.Resize(ref _data, Position + length);
        }

        public override byte[] ToArray()
        {
            if (Length > Position)
                Array.Resize(ref _data, Position);

            WriteSize();
            return _data;
        }

        public void WriteSize()
        {
            this[2] = (byte) (Position >> 8);
            this[1] = (byte) Position;
        }
    }
}