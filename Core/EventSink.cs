using System;
using EnhancedMap.Core.Network;

namespace EnhancedMap.Core
{
    public struct ResponseServerEntry
    {
        public ServerMessageType Type;
        public string Message;
    }

    public sealed class EventSink
    {
        public static event EventHandler<ResponseServerEntry> ServerResponseMessageEvent;

        public static void InvokeServerResponseMessage(ResponseServerEntry e)
        {
            ServerResponseMessageEvent.Raise(e);
        }
    }
}