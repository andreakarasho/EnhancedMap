using System;
using System.Collections.Generic;
using System.Text;

namespace EnhancedMapServerNetCore.Network
{
    public class PacketHandler
    {
        public PacketHandler(Action<Session, Packet> callback)
        {
            Callback = callback;
        }

        public Action<Session, Packet> Callback { get; }
    }
}
