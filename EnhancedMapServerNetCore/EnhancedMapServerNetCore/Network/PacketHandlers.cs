using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancedMapServerNetCore.Network
{
    public class PacketHandlers
    {
        private readonly List<PacketHandler>[] _handlers = new List<PacketHandler>[0x100];


        static PacketHandlers()
        {
            Handlers.Add(0x1F, LoginRequest);
            Handlers.Add(0x21, RemoteAdminCmds);
            Handlers.Add(0x23, ChatMsg);
            Handlers.Add(0x25, UserData);
            Handlers.Add(0x27, AlertAdvice);
            Handlers.Add(0x2B, ProtocolRequest);
            Handlers.Add(0x31, SharedLabel);
        }

        private PacketHandlers()
        {
            for (int i = 0; i < _handlers.Length; i++)
                _handlers[i] = new List<PacketHandler>();
        }

        public static PacketHandlers Handlers { get; } = new PacketHandlers();

        public void Add(byte id, Action<Session, Packet> handler)
        {
            lock (_handlers)
            {
                _handlers[id].Add(new PacketHandler(handler));
            }
        }

        public void OnPacket(Session client, Packet p)
        {
            lock (_handlers)
            {
                for (int i = 0; i < _handlers[p.ID].Count; i++)
                {
                    p.MoveToData();
                    _handlers[p.ID][i].Callback(client, p);
                }
            }
        }


        private static void LoginRequest(Session client, Packet p)
        {
            if (client == null || client.IsAccepted)
                return;

            byte userlen = p.ReadByte();
            byte passwordlen = p.ReadByte();

            string username = p.ReadASCII(userlen);
            string password = p.ReadASCII(passwordlen);

            if (!AccountManager.CanConnect(username, password, client))
                client.Dispose();
        }

        private static void RemoteAdminCmds(Session client, Packet p)
        {
            if (client == null || !client.IsAccepted)
                return;

            User user = UserManager.Get(client.Guid);
            if (user == null)
            {
                Log.Message(LogTypes.Warning, "User not found");
                return;
            }

            if (user.Account == null)
            {
                Log.Message(LogTypes.Warning, $"Account not found for {user}");
                return;
            }

            if (client.IsDisposed)
            {
                Log.Message(LogTypes.Warning, $"Session disposed {client.Guid}");
                return;
            }

            p.Skip(1);

            string keyword = p.ReadASCII(p.ReadByte());
            int count = p.ReadByte();

            string[] args = new string[count];
            for (int i = 0; i < count; i++)
                args[i] = p.ReadASCII(p.ReadByte());

            CommandManager.Execute(keyword, user, args);
        }

        private static void ChatMsg(Session client, Packet p)
        {
            if (client == null || !client.IsAccepted)
                return;

            ushort msglen = p.ReadUShort();
            int color = (int)p.ReadUInt();
            string message = p.ReadASCII(msglen);

            User user = UserManager.Get(client.Guid);

            user.SendToUsersInRoom(new PChatMessage(message, color, user.Name));
            client.Send(new PChatMessageResponse(true));
        }

        private static void UserData(Session client, Packet p)
        {
            if (client == null || !client.IsAccepted)
                return;

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            byte map = p.ReadByte();

            ushort hp = p.ReadUShort();
            ushort stam = p.ReadUShort();
            ushort mana = p.ReadUShort();

            ushort maxhp = p.ReadUShort();
            ushort maxstam = p.ReadUShort();
            ushort maxmana = p.ReadUShort();

            byte flag = p.ReadByte();
            //bool isdead = p.ReadBool();
            p.Skip(1);
            bool ispanic = p.ReadBool();

            uint color = p.ReadUInt();
            byte fontlen = p.ReadByte();
            string font = p.ReadASCII(fontlen);

            byte[] fontsize = new byte[4];
            for (int i = 0; i < 4; i++)
                fontsize[i] = p.ReadByte();
            byte fontStyle = p.ReadByte();


            User user = UserManager.Get(client.Guid);

            user.SendToUsersInRoom(new PUserData(x, y, map, hp, stam, mana, maxhp, maxstam, maxmana, flag, ispanic, color, fontlen, font, fontsize, fontStyle, user.Name));
        }

        private static void AlertAdvice(Session client, Packet p)
        {
            if (client == null || !client.IsAccepted)
                return;
            User user = UserManager.Get(client.Guid);
            user.SendToUsersInRoom(new PAlertAdvice(p.ReadUShort(), p.ReadUShort(), user.Name));
        }

        private static void ProtocolRequest(Session client, Packet p)
        {
            if (client == null || client.IsAccepted)
                return;

            if (p.ReadByte() == 0)
                client.Send(new PProtocolResponse());
            else
                client.Dispose();
        }

        private static void SharedLabel(Session client, Packet p)
        {
            if (client == null || !client.IsAccepted)
                return;

            bool toremove = p.ReadBool();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            byte map = p.ReadByte();

            User user = UserManager.Get(client.Guid);

            if (toremove)
            {
                var slabel = user.SharedLabels.FirstOrDefault(s => s.X == x && s.Y == y && s.Map == map);
                if (slabel != null)
                {
                    user.SharedLabels.Remove(slabel);
                    user.SendToUsersInRoom(new PSharedLabel(x, y, map, user.Name));
                }
            }
            else
            {
                string description = p.ReadASCII(p.ReadByte());
                SharedLabel label = new SharedLabel(x, y, map, description);
                user.SharedLabels.Add(label);
                user.SendToUsersInRoom(new PSharedLabel(x, y, map, description, user.Name));
            }
        }
    }
}