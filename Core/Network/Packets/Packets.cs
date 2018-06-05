using EnhancedMap.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EnhancedMap.Core.Network
{
    public sealed class PSharedLabel : PacketWriter
    {
        public PSharedLabel(ushort x, ushort y, byte map) : base(0x31)
        {        
            WriteBool(true);
            WriteUShort(x); WriteUShort(y); WriteByte(map);
        }

        public PSharedLabel(ushort x, ushort y, byte map, string description) : base(0x31)
        {
            WriteBool(false);
            WriteUShort(x); WriteUShort(y); WriteByte(map);
            WriteByte((byte)(description.Length + 0));
            WriteASCII(description);
        }
    }

    public sealed class PProtocolRequest : PacketWriter
    {
        public PProtocolRequest() : base(0x2B)
        {
            WriteByte(0x00);
        }
    }

    public sealed class PAlert : PacketWriter
    {
        public PAlert(ushort x, ushort y) : base(0x27)
        {
            WriteUShort(x);
            WriteUShort(y);
        }
    }

    public sealed class PLoginRequest : PacketWriter
    {
        public PLoginRequest(string username, string password) : base(0x1F)
        {
            int offset = NetworkManager.SocketClient.Protocol == Protocol.Protocol03 ? 0 : 1;

            WriteByte((byte)(username.Length + 0));
            WriteByte((byte)(password.Length + 0));
            WriteASCII(username);
            WriteASCII(password);
        }
    }

    public sealed class PRemoteMessage : PacketWriter
    {       
        public PRemoteMessage(ServerMessageType type, string keyword, List<string> args) : base(0x21)
        {
            WriteByte((byte)type);
            WriteByte((byte)(keyword.Length + 0));
            WriteASCII(keyword);
            WriteByte((byte)args.Count);

            foreach (string a in args)
            {
                WriteByte((byte)(a.Length + 0));
                WriteASCII(a);
            }
        }
    }

    public sealed class PChatMessage : PacketWriter
    {
        public PChatMessage(string message, Color messagecolor) : base(0x23)
        {
            WriteUShort((ushort)(message.Length + 0));
            WriteUInt((uint)messagecolor.ToArgb());
            WriteASCII(message);

            PacketHandlers.AddMessage(message);
        }
    }

    public sealed class PPlayerData : PacketWriter
    {
        public PPlayerData(ushort x, ushort y, byte map, ushort hp, ushort stam, ushort mana, ushort maxhp, ushort maxstam,
            ushort maxmana, byte flag, bool death, bool panic, int color, string fontName, float fontSize, byte fontStyle) : base(0x25)
        {
            int offset = NetworkManager.SocketClient.Protocol == Protocol.Protocol03 ? 0 : 1;

            WriteUShort(x); WriteUShort(y); WriteByte(map);
            WriteUShort(hp); WriteUShort(stam); WriteUShort(mana); WriteUShort(maxhp); WriteUShort(maxstam); WriteUShort(maxmana);
            WriteByte(flag);
            WriteBool(death);
            WriteBool(panic);
            WriteUInt((uint)color);

            WriteByte((byte)(fontName.Length + 0));
            WriteASCII(fontName);
            WriteFloat(fontSize);
            WriteByte(fontStyle);       
        }
    }
}