using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core.Network.Packets;
using EnhancedMap.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EnhancedMap.Core.Network
{
    public enum ServerMessageType
    {
        Normal = 0x00,
    }

    public class PacketHandlers
    {
        private static Queue<string> _messagesQueue = new Queue<string>();

        public static PacketHandlers Handlers { get; } = new PacketHandlers();

        private PacketHandlers()
        {
            for (int i = 0; i < _handlers.Length; i++)
                _handlers[i] = new List<PacketHandler>();
        }

        private readonly List<PacketHandler>[] _handlers = new List<PacketHandler>[0x100];

        public void Add(byte id, Action<Packet> handler)
        {
            lock (_handlers)
                _handlers[id].Add(new PacketHandler(handler));
        }

        public void OnPacket(Packet p)
        {
            lock (_handlers)
            {
                for (int i = 0; i < _handlers[p.ID].Count; i++)
                {
                    p.MoveToData();
                    _handlers[p.ID][i].Callback(p);
                }
            }
        }

        static PacketHandlers()
        {
            Handlers.Add(0x1F, NewLoginAccepted);
            Handlers.Add(0x21, ResponseFromServer);
            Handlers.Add(0x23, NewChatMessage);
            Handlers.Add(0x25, PlayerData);
            Handlers.Add(0x27, NewAlert);
            Handlers.Add(0x29, UserConnection);
            Handlers.Add(0x2B, ProtocolChoose);
            Handlers.Add(0x2D, ChatMessageResponse);
            Handlers.Add(0x2F, ServerMessage);
            Handlers.Add(0x31, SharedLabel);
        }


        public static void AddMessage(string msg) => _messagesQueue.Enqueue(msg);
        public static void ClearMessages() => _messagesQueue.Clear();


        private static void ServerMessage(Packet p)
        {
            UOClientManager.SysMessage("[EnhancedMapServer]: " + p.ReadASCII(p.ReadUShort()), (int)p.ReadUInt());
        }

        private static void ChatMessageResponse(Packet p)
        {
            if (p.ReadBool())
            {
                if (_messagesQueue.Count < 1)
                    return;

                lock (_messagesQueue)
                {
                    string msg = _messagesQueue.Dequeue();

                    if (!string.IsNullOrEmpty(msg))
                    {
                        UOClientManager.SysMessage("[Chat][" + Global.PlayerInstance.Name + "]: " + msg, 83);
                        ChatManager.Add(Global.PlayerInstance.Name, msg);
                    }
                }
            }
        }

        private static void SharedLabel(Packet p)
        {
            bool toremove = p.ReadBool();
            short x = (short)p.ReadUShort();
            short y = (short)p.ReadUShort();
            byte map = p.ReadByte();

            if (toremove)
            {
                UserObject user = RenderObjectsManager.GetUser(p.ReadASCII(p.ReadByte()));
                SharedLabelObject label = RenderObjectsManager.Get<SharedLabelObject>().FirstOrDefault(s => s.Parent == user && s.Position.X == x && s.Position.Y == y && s.Map == map);
                label?.Dispose();

                UOClientManager.SysMessage(string.Format("[SharedLabel][{0}] Removed a shared label!", user.Name), 83);
            }
            else
            {
                string description = p.ReadASCII(p.ReadByte());
                string username = p.ReadASCII(p.ReadByte());
                UserObject user = RenderObjectsManager.GetUser(username);
                if (user == null)
                    RenderObjectsManager.AddUser(user = new UserObject(username));

                RenderObjectsManager.AddSharedLabel(new SharedLabelObject(user, x, y, map, description));
                UOClientManager.SysMessage(string.Format("[SharedLabel][{0}] Added a shared label!", user.Name), 83);
            }
        }

        private static void ProtocolChoose(Packet p)
        {
            NetworkManager.SocketClient.Protocol = (Protocol)p.ReadByte();
            NetworkManager.SocketClient.Send(new PLoginRequest(Global.SettingsCollection["username"].ToString(), Global.SettingsCollection["password"].ToString()));
        }

        private static void NewAlert(Packet p)
        {
            int x = (short)p.ReadUShort();
            int y = (short)p.ReadUShort();
            string username = p.ReadASCII(p.ReadByte());

            UserObject user = RenderObjectsManager.GetUser(username);
            SignalObject signal = new SignalObject(x, y);
            RenderObjectsManager.AddSignal(signal);

            UOClientManager.SysMessage(string.Format("[Alert][{0}] sends an alert!", user.Name), 83);
        }

        private static void NewLoginAccepted(Packet p)
        {
            if (!NetworkManager.SocketClient.IsConnected)
                return;

            if (p.ReadByte() == 0x01)
            {
                NetworkManager.SocketClient.AccessLevel = (AccessLevel)p.ReadByte();
                string username = p.ReadASCII(p.ReadByte());

                if (NetworkManager.SocketClient.Protocol != (Protocol)p.ReadByte())
                {
                    UOClientManager.SysMessage("[Login] Wrong protocol. Update your map!", 83);
                    NetworkManager.Disconnect(false);
                }
                else
                {
                    NetworkManager.SocketClient.CanSend = true;
                    SocketClient.InvokeConnect();
                    UOClientManager.SysMessage("[Login] Accepted on server!", 83);
                }
            }
            else
            {
                UOClientManager.SysMessage("[Login] Refused from server :(", 83);
                NetworkManager.Disconnect(false);
            }
        }

        private static void NewChatMessage(Packet p)
        {
            ushort msglen = p.ReadUShort();
            int color = (int)p.ReadUInt();
            string message = p.ReadASCII(msglen);

            string username = p.ReadASCII(p.ReadByte());
            UserObject user = RenderObjectsManager.GetUser(username);
            ChatManager.Add(username, message);
            user.UpdateLifeTime();
            UOClientManager.SysMessage("[Chat][" + username + "]: " + message, 83);
        }

        public static void PlayerData(Packet p)
        {
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            byte facet = p.ReadByte();

            ushort hits = p.ReadUShort();
            ushort stam = p.ReadUShort();
            ushort mana = p.ReadUShort();
            ushort maxhits = p.ReadUShort();
            ushort maxstam = p.ReadUShort();
            ushort maxmana = p.ReadUShort();

            byte flag = p.ReadByte();
            p.Skip(1);
            bool panic = p.ReadBool();

            Color msgCol = Color.FromArgb((int)p.ReadUInt());

            string fontName = p.ReadASCII(p.ReadByte());

            float fontSize;
            unsafe
            {
                uint n = p.ReadUInt();
                fontSize = *(float*)(&n);
            }
             

            FontStyle fontStyle = (FontStyle)p.ReadByte();

            FontFamily f = FontFamily.Families.FirstOrDefault(s => s.Name == fontName);
            if (f == null)
                fontName = "Arial";
            else if (!f.IsStyleAvailable(fontStyle))
                fontStyle = FontStyle.Regular;

            string username = p.ReadASCII(p.ReadByte());
            UserObject user = RenderObjectsManager.GetUser(username);
            if (user == null)
                RenderObjectsManager.AddUser(user = new UserObject(username));

            user.UpdatePosition(x, y);
            user.Map = facet;
            user.Hits.Set(hits, maxhits);
            user.Stamina.Set(stam, maxstam);
            user.Mana.Set(mana, maxmana);
            
            switch(flag)
            {
                case 1:
                    user.IsPoisoned = true; break;
                case 2:
                    user.IsYellowHits = true; break;
                case 3:
                    user.IsParalyzed = true; break;
                case 4:
                    if (!user.IsDead)
                        RenderObjectsManager.AddDeathObject(new DeathObject(user, user.Position, user.Map));
                    user.IsDead = true; break;
                case 5:
                    user.IsHidden = true; break;
                default:
                    user.IsHidden = user.IsPoisoned = user.IsYellowHits = user.IsParalyzed = user.IsDead = false;
                    break;
            }

            if (user.InPanic && panic) // already panic, ignore
            {
                if (DateTime.Now > user.LastPanicUpdate)
                {
                    UOClientManager.SysMessage($"[Panic][{user.Name}] Needs help to: {user.Position} - Map: {user.Map}", 83);
                    user.LastPanicUpdate = DateTime.Now.AddSeconds(5);
                }
            }
            else if (!user.InPanic && panic) // receive panic signal
            {
                if (Global.SettingsCollection["panicsounds"].ToBool())
                    SoundsManager.Play(SOUNDS_TYPE.PANIC);
                UOClientManager.SysMessage($"[Panic][{user.Name}] Starts to panic!", 83);
                user.LastPanicUpdate = DateTime.Now.AddSeconds(5);
            }
            else if (user.InPanic && !panic) // receve remove panic signal
            {
                UOClientManager.SysMessage($"[Panic][{user.Name}] Stopped to panic.", 83);
            }

            user.InPanic = panic;
            user.Font = new Font(fontName, fontSize, fontStyle, GraphicsUnit.Pixel);
            user.Hue = new SolidBrush(msgCol);

            user.UpdateLifeTime();
        }

        public static void UserConnection(Packet p)
        {
            bool login = p.ReadBool();
            string username = p.ReadASCII(p.ReadByte());

            UserObject user = RenderObjectsManager.GetUser(username);

            if (user == null)
            {
                if (login)
                {
                    UOClientManager.SysMessage("[Login][" + username + "]: Connected.", 83);
                    RenderObjectsManager.AddUser(new UserObject(username));
                }
            }
            else
            {
                if (!login)
                {
                    if (Global.TrackedUser != null && Global.TrackedUser == user)
                        Global.TrackedUser = null;

                    UOClientManager.SysMessage("[Logout][" + username + "]: Disconnected.", 83);
                    user.Dispose();
                }
            }
        }

        public static void ResponseFromServer(Packet p)
        {
            ServerMessageType type = (ServerMessageType)p.ReadByte();
            string msg = p.ReadASCII(p.ReadUShort());
            EventSink.InvokeServerResponseMessage(new ResponseServerEntry()
            {
                Type = type,
                Message = msg
            });
        }

    }
}