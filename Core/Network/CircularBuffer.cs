using EnhancedMap.Core.Network;
using System;

namespace EnhancedMap
{
    public sealed class CircularBuffer
    {
        // Private fields
        private int _head;

        private int _tail;
        private int _size;
        private int _sizeUntilCut;
        private byte[] _buffer;

        /// <summary>
        /// Gets the length of the byte queue
        /// </summary>
        public int Length
        {
            get { return _size; }
        }

        /// <summary>
        /// Constructs a new instance of a byte queue.
        /// </summary>
        public CircularBuffer()
        {
            _buffer = new byte[2048];
        }

        /// <summary>
        /// Clears the byte queue
        /// </summary>
        internal void Clear()
        {
            _head = 0;
            _tail = 0;
            _size = 0;
            _sizeUntilCut = _buffer.Length;
        }

        /// <summary>
        /// Clears the byte queue
        /// </summary>
        internal void Clear(int size)
        {
            if (size > _size)
                size = _size;

            if (size == 0)
                return;

            _head = (_head + size) % _buffer.Length;
            _size -= size;

            if (_size == 0)
            {
                _head = 0;
                _tail = 0;
            }

            _sizeUntilCut = _buffer.Length - _head;
        }

        /// <summary>
        /// Extends the capacity of the bytequeue
        /// </summary>
        private void SetCapacity(int capacity)
        {
            byte[] newBuffer = new byte[capacity];

            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Buffer.BlockCopy(_buffer, _head, newBuffer, 0, _size);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _head, newBuffer, 0, _buffer.Length - _head);
                    Buffer.BlockCopy(_buffer, 0, newBuffer, _buffer.Length - _head, _tail);
                }
            }

            _head = 0;
            _tail = _size;
            _buffer = newBuffer;
        }

        /// <summary>
        /// Enqueues a buffer to the queue and inserts it to a correct position
        /// </summary>
        /// <param name="buffer">Buffer to enqueue</param>
        /// <param name="offset">The zero-based byte offset in the buffer</param>
        /// <param name="size">The number of bytes to enqueue</param>
        internal void Enqueue(byte[] buffer, int offset, int size)
        {
            if (size == 0)
                return;

            if ((_size + size) > _buffer.Length)
                SetCapacity((_size + size + 2047) & ~2047);

            if (_head < _tail)
            {
                int rightLength = (_buffer.Length - _tail);

                if (rightLength >= size)
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail, size);
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail, rightLength);
                    Buffer.BlockCopy(buffer, offset + rightLength, _buffer, 0, size - rightLength);
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _tail, size);
            }

            _tail = (_tail + size) % _buffer.Length;
            _size += size;
            _sizeUntilCut = _buffer.Length - _head;
        }

        /// <summary>
        /// Dequeues a buffer from the queue
        /// </summary>
        /// <param name="buffer">Buffer to enqueue</param>
        /// <param name="offset">The zero-based byte offset in the buffer</param>
        /// <param name="size">The number of bytes to dequeue</param>
        /// <returns>Number of bytes dequeued</returns>
        internal int Dequeue(byte[] buffer, int offset, int size)
        {
            if (size > _size)
                size = _size;

            if (size == 0)
                return 0;

            if (_head < _tail)
            {
                Buffer.BlockCopy(_buffer, _head, buffer, offset, size);
            }
            else
            {
                int rightLength = (_buffer.Length - _head);

                if (rightLength >= size)
                {
                    Buffer.BlockCopy(_buffer, _head, buffer, offset, size);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _head, buffer, offset, rightLength);
                    Buffer.BlockCopy(_buffer, 0, buffer, offset + rightLength, size - rightLength);
                }
            }

            _head = (_head + size) % _buffer.Length;
            _size -= size;

            if (_size == 0)
            {
                _head = 0;
                _tail = 0;
            }

            _sizeUntilCut = _buffer.Length - _head;
            return size;
        }

        /// <summary>
        /// Peeks a byte with a relative index to the _head
        /// Note: should be used for special cases only, as it is rather slow
        /// </summary>
        /// <param name="index">A relative index</param>
        /// <returns>The byte peeked</returns>
        public byte PeekOne(int index)
        {
            return index >= _sizeUntilCut ? _buffer[index - _sizeUntilCut] : _buffer[_head + index];
        }

        public byte GetId() => _size > 1 ? _buffer[_head] : (byte)0;
        public short GetLength() => _size >= 3 ? (short) ((_buffer[(_head + 2) % _buffer.Length] << 8) | _buffer[(_head + 1) % _buffer.Length]) : (short)0xff;
           
    }
}