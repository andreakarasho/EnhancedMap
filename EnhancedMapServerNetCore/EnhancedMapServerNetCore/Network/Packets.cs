using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Managers;

namespace EnhancedMapServerNetCore.Network
{
    public sealed class PUserData : PacketWriter
    {
        public PUserData(ushort x, ushort y, byte map, ushort hp, ushort stam, ushort mana, ushort maxhp, ushort maxstam, ushort maxmana, byte flag, bool ispanic, uint color, byte fontlen, string font, byte[] fontsize, byte fontstyle, string name) : base(0x25)
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
        public PLoginResponse(ACCOUNT_LEVEL level, string name) : base(0x1F)
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
        public PServerResponseCmdToClient(string msg, SERVER_MESSAGE_TYPE type) : base(0x21)
        {
            WriteByte((byte) type);
            WriteUShort((ushort) msg.Length);
            WriteASCII(msg);
        }
    }

    public sealed class PChatMessage : PacketWriter
    {
        public PChatMessage(string message, int color, string name) : base(0x23)
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
        public PAlertAdvice(ushort x, ushort y, string name) : base(0x27)
        {
            WriteUShort(x);
            WriteUShort(y);
            WriteByte((byte) name.Length);
            WriteASCII(name);
        }
    }

    public sealed class PUserConnection : PacketWriter
    {
        public PUserConnection(string name, bool connected) : base(0x29)
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
        public PChatMessageResponse(bool allow) : base(0x2D)
        {
            WriteBool(allow);
        }
    }

    public sealed class PServerMessage : PacketWriter
    {
        public PServerMessage(string message, uint color) : base(0x2F)
        {
            WriteUShort((ushort) message.Length);
            WriteASCII(message);
            WriteUInt(color);
        }
    }

    public sealed class PSharedLabel : PacketWriter
    {
        public PSharedLabel(ushort x, ushort y, byte map, string description, string name) : base(0x31)
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

        public PSharedLabel(ushort x, ushort y, byte map, string name) : base(0x31)
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