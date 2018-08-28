using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Managers;

namespace EnhancedMapServerNetCore.Network
{
    public sealed class PUserData : PacketWriter
    {
        public PUserData(in ushort x, in ushort y, in byte map, in ushort hp, in ushort stam, in ushort mana, in ushort maxhp, in ushort maxstam, in ushort maxmana, in byte flag, in bool ispanic, in uint color, in byte fontlen, in string font, in byte[] fontsize, in byte fontstyle, in string name) : base(0x25)
        {
            WriteUShort(x);
            WriteUShort(y);
            WriteByte(map);
            WriteUShort(hp);
            WriteUShort(stam);
            WriteUShort(mana);
            WriteUShort(maxhp);
            WriteUShort(maxstam);
            WriteUShort(maxmana);
            WriteByte(flag);
            WriteByte(0);
            WriteBool(ispanic);
            WriteUInt(color);
            WriteByte(fontlen);
            WriteASCII(font);
            for (int i = 0; i < fontsize.Length; i++)
                WriteByte(fontsize[i]);
            WriteByte(fontstyle);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }

    public sealed class PLoginResponse : PacketWriter
    {
        public PLoginResponse(in ACCOUNT_LEVEL level, in string name) : base(0x1F)
        {
            WriteByte(1);
            WriteByte((byte) level);
            WriteByte((byte) name.Length);
            WriteASCII(name);
            WriteByte(Core.Protocol);
        }
    }

    public sealed class PServerResponseCmdToClient : PacketWriter
    {
        public PServerResponseCmdToClient(in string msg, in SERVER_MESSAGE_TYPE type) : base(0x21)
        {
            WriteByte((byte) type);
            WriteUShort((ushort) msg.Length);
            WriteASCII(msg);
        }
    }

    public sealed class PChatMessage : PacketWriter
    {
        public PChatMessage(in string message, in int color, in string name) : base(0x23)
        {
            WriteUShort((ushort) message.Length);
            WriteUInt((uint) color);
            WriteASCII(message);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }

    public sealed class PAlertAdvice : PacketWriter
    {
        public PAlertAdvice(in ushort x, in ushort y, in string name) : base(0x27)
        {
            WriteUShort(x);
            WriteUShort(y);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }

    public sealed class PUserConnection : PacketWriter
    {
        public PUserConnection(in string name, in bool connected) : base(0x29)
        {
            WriteBool(connected);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }

    public sealed class PProtocolResponse : PacketWriter
    {
        public PProtocolResponse() : base(0x2B)
        {
            WriteByte(Core.Protocol);
        }
    }

    public sealed class PChatMessageResponse : PacketWriter
    {
        public PChatMessageResponse(in bool allow) : base(0x2D)
        {
            WriteBool(allow);
        }
    }

    public sealed class PServerMessage : PacketWriter
    {
        public PServerMessage(in string message, in uint color) : base(0x2F)
        {
            WriteUShort((ushort) message.Length);
            WriteASCII(message);
            WriteUInt(color);
        }
    }

    public sealed class PSharedLabel : PacketWriter
    {
        public PSharedLabel(in ushort x, in ushort y, in byte map, in string description, in string name) : base(0x31)
        {
            WriteBool(false);
            WriteUShort(x);
            WriteUShort(y);
            WriteByte(map);
            WriteByte((byte) description.Length);
            WriteASCII(description);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }

        public PSharedLabel(in ushort x, in ushort y, in byte map, in string name) : base(0x31)
        {
            WriteBool(true);
            WriteUShort(x);
            WriteUShort(y);
            WriteByte(map);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }
}