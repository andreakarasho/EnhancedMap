using System;
using System.Collections.Generic;
using EnhancedMapServerNetCore.Network;

namespace EnhancedMapServerNetCore.Internals
{
    public class User : IEquatable<User>
    {
        public User(string name, Session socket)
        {
            Name = name;
            Session = socket;
        }

        public User(Account account, Session socket) : this(account.Name, socket)
        {
            Account = account;
            Room = Account.Room;
        }

        public string Name { get; }
        public Guid Guid => Session.Guid;
        public Session Session { get; }
        public Account Account { get; }
        public Room Room { get; }
        public List<SharedLabel> SharedLabels { get; } = new List<SharedLabel>();

        public bool Equals(User other)
        {
            return other.Guid.Equals(Guid);
        }

        public void SendToUsersInRoom(PacketWriter packet)
        {
            for (int i = 0; i < Room.Users.Count; i++)
            {
                var user = Room.Users[i];
                if (user != null && user.Session != null && !user.Session.IsDisposed && user != this) user.Session.Send(packet);
            }
        }


        public void RequestSharedLabels()
        {
            for (int i = 0; i < Room.Users.Count; i++)
            {
                User user = Room.Users[i];
                if (user != null && user.Session != null && !user.Session.IsDisposed)
                {
                    for (int j = 0; j < user.SharedLabels.Count; j++)
                    {
                        SharedLabel label = user.SharedLabels[j];
                        Session.Send(new PSharedLabel(label.X, label.Y, label.Map, label.Description, user.Name));
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{Name} - {Guid}";
        }
    }
}