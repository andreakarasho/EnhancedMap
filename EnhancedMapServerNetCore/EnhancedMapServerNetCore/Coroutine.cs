using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EnhancedMapServerNetCore
{
    public class CoroutineManager
    {
        private static List<Coroutine> _coroutines = new List<Coroutine>();

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            var coroutine = new Coroutine(routine);
            _coroutines.Add(coroutine);
            return coroutine;
        }

        public static void Update()
        {
            for (int i = 0; i < _coroutines.Count; i++)
            {
                var coroutine = _coroutines[i];

                if (!coroutine.MoveNext())
                {
                    _coroutines[i] = _coroutines[_coroutines.Count - 1];
                    _coroutines.RemoveAt(_coroutines.Count - 1);
                }
            }
        }
    }

    public class YieldInstruction
    {
        internal IEnumerator routine;

        internal YieldInstruction() { }

        internal bool MoveNext()
        {
            if (routine.Current is YieldInstruction yieldInstruction)
            {
                if (yieldInstruction.MoveNext())
                {
                    return true;
                }
            }
            return routine.MoveNext();
        }
    }

    public class Coroutine : YieldInstruction
    {
        public Coroutine(IEnumerator routine)
        {
            this.routine = routine;
        }
    }

    public class WaitForSeconds : YieldInstruction
    {
        public WaitForSeconds(float seconds)
        {
            var delay = DateTime.Now.AddSeconds(seconds);
            this.routine = Count(delay);
        }

        private IEnumerator Count(DateTime delay)
        {
            while (DateTime.Now < delay)
                yield return true;
        }
    }

    public class WaitUntil : YieldInstruction
    {
        public WaitUntil(Func<bool> func)
        {
            this.routine = Until(func);
        }

        public WaitUntil(Func<bool> func, float seconds)
        {
            var delay = DateTime.Now.AddSeconds(seconds);
            this.routine = UntilWithTimeout(func, delay);
        }

        private IEnumerator Until(Func<bool> func)
        {
            while (!func())
                yield return true;
        }

        private IEnumerator UntilWithTimeout(Func<bool> func, DateTime delay)
        {
            while (!func() && DateTime.Now < delay)
                yield return true;
        }
    }
}
