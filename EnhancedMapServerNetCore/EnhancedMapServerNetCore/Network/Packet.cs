using System;
using System.Text;

namespace EnhancedMapServerNetCore.Network
{
    public sealed class Packet : PacketBase
    {
        private readonly byte[] _data;

        public Packet(byte[] data, byte id, int length)
        {
            _data = data;
            Length = length;
            ID = id;
        }

        protected override byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                _data[index] = value;
                IsChanged = true;
            }
        }

        public override byte ID { get; }

        public bool IsChanged { get; private set; }
        public override int Length { get; }

        protected override void EnsureSize(int length)
        {
            if (length < 0 || Position + length > Length)
                throw new ArgumentOutOfRangeException("length");
        }

        public override byte[] ToArray()
        {
            return _data;
        }

        public void MoveToData()
        {
            Seek(3);
        }

        public byte ReadByte()
        {
            EnsureSize(1);
            return this[Position++];
        }

        public sbyte ReadSByte()
        {
            return (sbyte) ReadByte();
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        public ushort ReadUShort()
        {
            EnsureSize(2);
            return (ushort) (ReadByte() | (ReadByte() << 8));
        }

        public uint ReadUInt()
        {
            EnsureSize(4);
            return (uint) (ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24));
        }

        public string ReadASCII()
        {
            EnsureSize(1);
            StringBuilder sb = new StringBuilder();
            char c;

            while ((c = (char) ReadByte()) != '\0')
                sb.Append(c);
            return sb.ToString();
        }

        public string ReadASCII(int length)
        {
            EnsureSize(length);
            StringBuilder sb = new StringBuilder(length);
            char c;

            for (int i = 0; i < length; i++)
            {
                c = (char) ReadByte();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public string ReadUnicode()
        {
            EnsureSize(2);
            StringBuilder sb = new StringBuilder();
            char c;

            while ((c = (char) ReadUShort()) != '\0')
                sb.Append(c);

            return sb.ToString();
        }

        public string ReadUnicode(int length)
        {
            EnsureSize(length);
            StringBuilder sb = new StringBuilder(length);
            char c;
            for (int i = 0; i < length; i++)
            {
                c = (char) ReadUShort();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public string ReadUnicodeReversed(int length)
        {
            EnsureSize(length);
            length /= 2;

            StringBuilder sb = new StringBuilder(length);
            char c;

            for (int i = 0; i < length; i++)
            {
                c = (char) ReadUShortReversed();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public ushort ReadUShortReversed()
        {
            return (ushort) (ReadByte() | (ReadByte() << 8));
        }

        public ushort ReadUIntReversed()
        {
            return (ushort) (ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24));
        }
    }
}