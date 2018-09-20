using System;
using System.Collections.Concurrent;
using System.Linq;
using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Network;

namespace EnhancedMapServerNetCore.Managers
{
    public static class UserManager
    {
        private static readonly ConcurrentDictionary<Guid, User> _users = new ConcurrentDictionary<Guid, User>();

        public static User Get(Guid guid)
        {
            _users.TryGetValue(guid, out var user);
            return user;
        }

        public static User Get(string name)
        {
            return _users.Values.FirstOrDefault(s => s.Name == name);
        }


        public static bool Add(Account account, Session session)
        {
            User user = Get(session.Guid);
            if (user != null)
                Log.Message(LogTypes.Panic, $"User '{user}' already exists.");
            else
            {
                user = new User(account, session);
                if (_users.TryAdd(session.Guid, user))
                {
                    InsertIntoRoom(user);
                    return true;
                }

                Log.Message(LogTypes.Panic, $"Impossible to add user session '{user}'");
            }

            return false;
        }

        public static bool Add(string name, Session session)
        {
            User user = Get(session.Guid);
            if (user != null)
                Log.Message(LogTypes.Panic, $"User '{user}' already exists.");
            else
            {
                user = new User(name, session);
                if (_users.TryAdd(session.Guid, user))
                {
                    InsertIntoRoom(user);
                    return true;
                }

                Log.Message(LogTypes.Panic, $"Impossible to add user session '{user}'");
            }

            return false;
        }

        public static void Remove(Guid guid)
        {
            if (_users.TryRemove(guid, out var user)) RemoveFromRoom(user);
        }

        public static void InsertIntoRoom(User user)
        {
            lock (user.Room.Users)
            {
                if (user.Room.Users.Contains(user))
                {
                    Log.Message(LogTypes.Panic, $"User '{user}' already into room '{user.Room}'. Disconnected to prevent.");
                    user.Session.Dispose();
                }
                else
                {
                    user.Room.Users.Add(user);
                    user.SendToUsersInRoom(new PUserConnection(user.Name, true));

                    Log.Message(LogTypes.Info, $"User '{user}' joined into room '{user.Room}'");
                }
            }
        }

        public static void RemoveFromRoom(User user)
        {
            lock (user.Room.Users)
            {
                user.SendToUsersInRoom(new PUserConnection(user.Name, false));
                if (user.Room.Users.Remove(user)) Log.Message(LogTypes.Info, $"User '{user}' removed from room '{user.Room}'");
            }
        }
    }
}