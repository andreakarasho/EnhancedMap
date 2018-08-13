using System;
using System.Threading.Tasks;

namespace EnhancedMapServerNetCore
{
    public static class Exstentions
    {
        public static void Raise(this EventHandler handler, object sender = null)
        {
            handler?.Invoke(sender, EventArgs.Empty);
        }

        public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            handler?.Invoke(sender, e);
        }

        public static void RaiseAsync(this EventHandler handler, object sender = null)
        {
            Task.Run(() => handler?.Invoke(sender, EventArgs.Empty));
        }

        public static void RaiseAsync<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            Task.Run(() => handler?.Invoke(sender, e));
        }
    }
}