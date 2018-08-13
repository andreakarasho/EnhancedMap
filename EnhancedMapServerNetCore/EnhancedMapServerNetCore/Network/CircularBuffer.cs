using System;

namespace EnhancedMapServerNetCore.Network
{
    public class CircularBuffer
    {
        private byte[] _buffer;
        private int _head;
        private int _tail;

        public CircularBuffer()
        {
            _buffer = new byte[2048];
        }

        public int Length { get; private set; }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            Length = 0;
        }

        private void SetCapacity(int capacity)
        {
            byte[] newBuffer = new byte[capacity];

            if (Length > 0)
            {
                if (_head < _tail)
                    Buffer.BlockCopy(_buffer, _head, newBuffer, 0, Length);
                else
                {
                    Buffer.BlockCopy(_buffer, _head, newBuffer, 0, _buffer.Length - _head);
                    Buffer.BlockCopy(_buffer, 0, newBuffer, _buffer.Length - _head, _tail);
                }
            }

            _head = 0;
            _tail = Length;
            _buffer = newBuffer;
        }


        public byte GetPacketID()
        {
            return Length > 1 ? _buffer[_head] : (byte) 0;
        }


        public short GetPacketLength()
        {
            return Length >= 3 ? (short) ((_buffer[(_head + 2) % _buffer.Length] << 8) | _buffer[(_head + 1) % _buffer.Length]) : (short) 0xff;
        }

        public int Dequeue(byte[] buffer, int offset, int size)
        {
            if (size > Length) size = Length;

            if (size == 0) return 0;

            if (_head < _tail)
                Buffer.BlockCopy(_buffer, _head, buffer, offset, size);
            else
            {
                int rightLength = _buffer.Length - _head;

                if (rightLength >= size)
                    Buffer.BlockCopy(_buffer, _head, buffer, offset, size);
                else
                {
                    Buffer.BlockCopy(_buffer, _head, buffer, offset, rightLength);
                    Buffer.BlockCopy(_buffer, 0, buffer, offset + rightLength, size - rightLength);
                }
            }

            _head = (_head + size) % _buffer.Length;
            Length -= size;

            if (Length == 0)
            {
                _head = 0;
                _tail = 0;
            }

            return size;
        }

        public void Enqueue(byte[] buffer, int offset, int size)
        {
            if (Length + size > _buffer.Length)
            {
                SetCapacity((Length + size + 2047) & ~2047);
                // da testare
                //throw new CapacityExceededException();
            }

            if (_head < _tail)
            {
                int rightLength = _buffer.Length - _tail;

                if (rightLength >= size)
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail, size);
                else
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail, rightLength);
                    Buffer.BlockCopy(buffer, offset + rightLength, _buffer, 0, size - rightLength);
                }
            }
            else
                Buffer.BlockCopy(buffer, offset, _buffer, _tail, size);

            _tail = (_tail + size) % _buffer.Length;
            Length += size;
        }
    }
}