using System.Collections.Generic;

namespace EnhancedMapServerNetCore.Network
{
    public class ReusableArray<T>
    {
        private readonly Queue<T[]> _segments;
        private readonly int _size, _capacity;

        public ReusableArray(int capacity, int size)
        {
            _capacity = capacity;
            _size = size;

            _segments = new Queue<T[]>(capacity);

            for (int i = 0; i < capacity; i++) _segments.Enqueue(new T[size]);
        }

        public T[] GetSegment()
        {
            lock (this)
            {
                if (_segments.Count > 0)
                    return _segments.Dequeue();

                for (int i = 0; i < _capacity; i++)
                    _segments.Enqueue(new T[_size]);
                return _segments.Dequeue();
            }
        }

        public void Free(T[] s)
        {
            if (s == null)
                return;
            lock (this)
            {
                _segments.Enqueue(s);
            }
        }
    }
}