using System;

namespace EnhancedMapServerNetCore.Network
{
    public class PacketHandler
    {
        public PacketHandler(in Action<Session, Packet> callback)
        {
            Callback = callback;
        }

        public Action<Session, Packet> Callback { get; }
    }
}