using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EnhancedMap
{
    public static class TimerManager
    {
        static TimerManager()
        {
            Thread t = new Thread(Run) { IsBackground = true };
            t.Start();
        }

        private static readonly List<Timer> _Timers = new List<Timer>();

        private static readonly ConcurrentQueue<Timer> _ToAdd = new ConcurrentQueue<Timer>();
        private static readonly ConcurrentQueue<Timer> _ToRemove = new ConcurrentQueue<Timer>();

        private static readonly AutoResetEvent _waitHandle = new AutoResetEvent(true);

        private static void Run()
        {
            while (MainCore.IsRunning)
            {
                _waitHandle.WaitOne(1);

                if (_ToAdd.Count > 0 || _ToRemove.Count > 0)
                {
                    while (_ToAdd.Count > 0)
                    {
                        if (_ToAdd.TryDequeue(out Timer t))
                            _Timers.Add(t);
                    }

                    while (_ToRemove.Count > 0)
                    {
                        if (_ToRemove.TryDequeue(out Timer t))
                            _Timers.Remove(t);
                    }
                }

                foreach (var t in _Timers)
                {
                    if (!t.IsRunning) continue;

                    if (t._delay > 0 && t._firstRun)
                    {
                        if (DateTime.Now > t._nextDelay)
                            t._firstRun = false;
                        else
                            continue;
                    }

                    if (DateTime.Now > t._next)
                    {
                        t.OnTick();
                        t._next = DateTime.Now.AddMilliseconds(t._interval);
                    }
                }
            }
        }

 
        public static Timer Create(int interval, Action callback, bool started = true)
        {
            return Create(interval, 0, callback, started);
        }

        public static Timer Create(int interval, int delay, Action callback, bool started = true)
        {
            Timer t = Timer.CallBack(interval, delay, callback);
            Add(t);
            if (started)
                t.Start();
            return t;
        }

        private static void Add(Timer t)
        {
            _ToAdd.Enqueue(t);
        }

        public static void Remove(Timer t)
        {
            if (t.IsRunning)
                t.Stop();
            _ToRemove.Enqueue(t);
        }
    }

    public abstract class Timer
    {
        internal int _interval, _delay;
        internal DateTime _next, _nextDelay;
        internal bool _firstRun;

        protected Timer(int interval)
        {
            _interval = interval;
            _next = DateTime.Now;
        }

        protected Timer(int interval, int delay) : this(interval)
        {
            _delay = delay;
        }

        public bool IsRunning { get; private set; }

        public abstract void OnTick();

        public void Start()
        {
            IsRunning = true;

            if (_delay > 0)
            {
                _nextDelay = DateTime.Now.AddMilliseconds(_delay);
                _firstRun = true;
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        internal static Timer CallBack(int interval, int delay, Action callback)
        {
            return new TimerState(interval, delay, callback);
        }

        private class TimerState : Timer
        {
            private readonly Action _callback;

            public TimerState(int interval, int delay, Action callback) : base(interval, delay)
            {
                _callback = callback;
            }

            public override void OnTick()
            {
                _callback();
            }
        }
    }
}