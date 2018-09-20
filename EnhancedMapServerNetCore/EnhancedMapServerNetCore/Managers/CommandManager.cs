using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedMapServerNetCore.Configuration;
using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Network;

namespace EnhancedMapServerNetCore.Managers
{
    public enum SERVER_MESSAGE_TYPE
    {
        NORMAL
    }

    public static class CommandManager
    {
        private static readonly Dictionary<string, Tuple<ACCOUNT_LEVEL, Action<User, string[]>>> _commands = new Dictionary<string, Tuple<ACCOUNT_LEVEL, Action<User, string[]>>>
        {
            { "adduser", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, AddUser)},
            { "removeuser", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, RemoveUser)},
            { "addroom", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, AddRoom)},
            { "removeroom", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, RemoveRoom)},
            { "setpassword", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, SetPassword)},
            { "setroom", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetRoom)},
            { "allusersonline", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, AllUsersOnline)},
            { "userinfo", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, UserInfo)},
            { "kick", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, Kick)},
            { "ban", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, Ban)},
            { "enableuser", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, EnableUser)},
            { "allusersinroom", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.ROOM_ADMIN, AllUsersInRoom)},
            { "allusers", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, AllUsers)},
            { "statistics", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, Statistics)},
            { "setprivileges", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetPrivileges)},
            { "setloginsys", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetLoginSystem)},
            { "setroompassword", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetRoomPassword)},
            { "getroompassword", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, GetRoomPassword)},
            { "allrooms", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, AllRooms)},
            { "getport", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, GetPort)},
            { "setkicktime", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetKickTime)},
            { "setmaxusersconnection", new Tuple<ACCOUNT_LEVEL, Action<User, string[]>>(ACCOUNT_LEVEL.SERVER_ADMIN, SetMaxUsersConnection)}
        };


        private static void SendTo(User user, string msg, bool istable = false)
        {
            if (user != null)
                user.Session.Send(new PServerResponseCmdToClient(msg, SERVER_MESSAGE_TYPE.NORMAL));
            else
                Log.Message(istable ? LogTypes.Table : LogTypes.None, msg);
        }


        public static void Execute(string action, User user, params string[] args)
        {
            action = action.ToLower();

            if (_commands.TryGetValue(action, out var tuple))
            {
                if (user.Account.AccountLevel >= tuple.Item1)
                    tuple.Item2(user, args);
                else
                {
                    Log.Message(LogTypes.Warning, $"{user} hasn't privileges to do '{action}'");
                    user.Session.Dispose();
                }
            }
            else
                Log.Message(LogTypes.Warning, $"Invalid action '{action}' from {user}");
        }

        public static void ExecuteFromConsole(string action, params string[] args)
        {
            if (_commands.TryGetValue(action, out var tuple)) tuple.Item2(null, args);
        }

        private static void SetMaxUsersConnection(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            if (ushort.TryParse(args[0], out ushort max))
            {
                SettingsManager.Configuration.MaxActiveConnections = max;
                SendTo(user, "Server max connections: " + max);
            }
            else
                SendTo(user, "Wrong typed number.");
        }

        private static void GetPort(User user, params string[] args)
        {
            SendTo(user, "Server port: " + SettingsManager.Configuration.Port);
        }

        private static void SetKickTime(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            if (uint.TryParse(args[0], out var result))
            {
                if (result <= 0)
                    SendTo(user, "Cannot set a time less or greater than 0");
                else
                    SettingsManager.Configuration.KickTimer = result;
            }
            else
                SendTo(user, "Wrong input.");
        }

        private static void AddUser(User user, params string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
                return;
            if (user != null && args.Length == 3)
            {
                Array.Resize(ref args, 4);
                args[2] = user.Account.Room.Name;
                args[3] = "0";
            }

            string username = args[0];
            string password = args[1];
            string room = args[2];

            if (!int.TryParse(args[3], out var level)) SendTo(user, "Wrong value for access level");

            if (string.IsNullOrWhiteSpace(username))
                SendTo(user, "Username is null");
            else if (username.Length > 16)
                SendTo(user, "Max username length is 16 chars");
            else if (string.IsNullOrWhiteSpace(password))
                SendTo(user, "Password is null");
            else if (password.Length > 16)
                SendTo(user, "Max password length is 16 chars");
            else if (!Utility.IsStringAllowed(username))
                SendTo(user, "Some char is not allowed into: \"" + username + "\"");
            else if (!Utility.IsStringAllowed(password))
                SendTo(user, "Some char is not allowed into: \"" + password + "\"");
            else if (RoomManager.Get(room) == null)
                SendTo(user, "Room not found.");
            else if (AccountManager.Get(username) != null)
                SendTo(user, "User already exist.");
            else if (level < 0 || level > 2)
                SendTo(user, "Wrong value for Access Level");
            else
            {
                AccountManager.Add(username, password, room, (ACCOUNT_LEVEL) level);
                SendTo(user, "User added with parameters:\r\nUsername: " + username + "\r\nPassword: " + password + "\r\nRoom: " + room + "\r\nPrivileges: " + (ACCOUNT_LEVEL) level);
            }
        }

        private static void RemoveUser(User user, params string[] args)
        {
            if (args.Length != 1)
                return;

            string username = args[0];

            Account toremove = AccountManager.Get(username);

            if (toremove == null)
                SendTo(user, "Account not exist");
            else if (user != null && user.Account == toremove)
                SendTo(user, "Cannot kick yourself");
            else if (user != null && user.Account.AccountLevel < ACCOUNT_LEVEL.SERVER_ADMIN && toremove.AccountLevel == ACCOUNT_LEVEL.SERVER_ADMIN)
                SendTo(user, "You have't right privileges");
            else if (user != null && user.Account.AccountLevel <= ACCOUNT_LEVEL.ROOM_ADMIN && user.Account.Room != toremove.Room)
                SendTo(user, "You have't right privileges");
            /*else if (UserManager.Get(toremove.Guid) != null)
                SendTo(user, "Cant't remove a connected user.");*/
            else
            {
                User usertoremove = UserManager.Get(username);
                if (usertoremove != null && usertoremove.Session != null && !usertoremove.Session.IsDisposed)
                    usertoremove.Session.Dispose();

                AccountManager.Remove(username);
                SendTo(user, "Account removed");
            }
        }

        private static void AddRoom(User user, params string[] args)
        {
            if (args.Length <= 0)
                return;

            string name = args[0];
            string psw = args.Length == 2 ? args[1] : string.Empty;

            if (RoomManager.Get(name) != null)
                SendTo(user, "Room already exists.");
            else
            {
                RoomManager.Add(name, psw);
                SendTo(user, "Room added.");
            }
        }

        private static void RemoveRoom(User user, params string[] args)
        {
            if (args.Length <= 0)
                return;

            string name = args[0];
            if (name == "General")
                SendTo(user, "Cannot remove 'General' room");
            else
            {
                Room room = RoomManager.Get(name);
                if (room == null)
                    SendTo(user, "Room not exists.");
                else
                {
                    RoomManager.Remove(name);
                    SendTo(user, "Room removed.");
                }
            }
        }

        private static void SetPassword(User user, params string[] args)
        {
            if (args.Length < 2)
                return;

            string username = args[0];
            string password = args[1];

            if (string.IsNullOrWhiteSpace(username))
                SendTo(user, "Wrong username");
            else if (string.IsNullOrWhiteSpace(password))
                SendTo(user, "Password must containts chars");
            else if (password.Length > 16)
                SendTo(user, "Max password length is 16 chars");
            else if (!Utility.IsStringAllowed(password))
                SendTo(user, "Some char is not allowed into: \"" + password + "\"");
            else
            {
                Account a = AccountManager.Get(username);
                if (a == null)
                    SendTo(user, "User not found");
                else
                {
                    a.CryptPassword(password);
                    SendTo(user, "New password \"" + password + "\" saved");
                }
            }
        }

        private static void SetRoom(User user, params string[] args)
        {
            if (args.Length < 2)
                return;

            string name = args[0];
            string roomname = args[1];

            Account a = AccountManager.Get(name);
            if (a != null)
            {
                Room room = RoomManager.Get(roomname);
                if (room != null)
                {
                    User target = UserManager.Get(name);
                    if (target != null)
                        target.Session.Dispose();
                    a.Room = room;
                    SendTo(user, "New room '" + name + "' saved.");
                }
                else
                    SendTo(user, "Room not exist.");
            }
            else
                SendTo(user, "User not found.");
        }

        private static void AllUsersOnline(User user, params string[] args)
        {
            for (int i = 0; i < Core.Server.Sessions.Count; i++)
            {
                User u = UserManager.Get(Core.Server.Sessions[i].Guid);
                if (u != null) SendTo(user, string.Format("|{0,24}|{1,24}|{2,24}|", u.Name, u.Room.Name, u.Account.AccountLevel), true);
            }
        }

        private static void UserInfo(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            string username = args[0];

            User userI = UserManager.Get(username);
            Account account = userI?.Account ?? AccountManager.Get(username);
            if (account != null)
            {
                string info = $"Name: {account.Name}\r\nPassword: {account.CryptedPassword}\r\nRoom: {account.Room}\r\nEnabled: {!account.IsBanned}\r\nPrivileges: {account.AccountLevel}\r\nStatus: {(userI != null && userI.Session != null ? userI.Session.Address.ToString() : "OFFLINE")}";
                SendTo(user, info);
            }
            else
                SendTo(user, "User not found.");
        }

        private static void AllRooms(User user, params string[] args)
        {
            foreach (Room room in RoomManager.Rooms) SendTo(user, $"{room.Name} - [{room.Users.Count}]");
        }

        private static void Kick(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            string username = args[0];

            User tokick = UserManager.Get(username);
            if (tokick == null)
                SendTo(user, "Cannot kick an offline user");
            else
            {
                Account account = tokick.Account ?? AccountManager.Get(username);

                if (account == null)
                    SendTo(user, "Account not exists.");
                else if (user != null && user.Account == account)
                    SendTo(user, "Cannot kick yourself.");
                else if (user != null && (user.Account.AccountLevel < ACCOUNT_LEVEL.SERVER_ADMIN && account.AccountLevel == ACCOUNT_LEVEL.SERVER_ADMIN || user.Account.AccountLevel <= ACCOUNT_LEVEL.ROOM_ADMIN && user.Room != account.Room))
                    SendTo(user, "You haven't right privileges");
                else
                {
                    tokick.Session.Dispose();
                    account.IsKicked = true;

                    SendTo(user, "User kicked for: " + SettingsManager.Configuration.KickTimer + " sec.");
                }
            }
        }

        private static void Ban(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            string username = args[0];

            Account account = AccountManager.Get(username);
            if (account == null)
                SendTo(user, "Account not exists.");
            else if (user != null && user.Account == account)
                SendTo(user, "Cannot ban yourself.");
            else if (user != null && (account.AccountLevel < ACCOUNT_LEVEL.SERVER_ADMIN && account.AccountLevel == ACCOUNT_LEVEL.SERVER_ADMIN || user.Account.AccountLevel <= ACCOUNT_LEVEL.ROOM_ADMIN && user.Room != account.Room))
                SendTo(user, "You haven't right privileges.");
            else
            {
                User toban = UserManager.Get(username);
                toban?.Session.Dispose();

                account.IsBanned = true;
                SendTo(user, "Account banned.");
            }
        }

        private static void EnableUser(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            string username = args[0];

            Account account = AccountManager.Get(username);
            if (account == null)
                SendTo(user, "Account not exists.");
            else if (user != null && user.Account == account)
                SendTo(user, "Cannot ban yourself.");
            else if (user != null && (account.AccountLevel < ACCOUNT_LEVEL.SERVER_ADMIN && account.AccountLevel == ACCOUNT_LEVEL.SERVER_ADMIN || user.Account.AccountLevel <= ACCOUNT_LEVEL.ROOM_ADMIN && user.Room != account.Room))
                SendTo(user, "You haven't right privileges.");
            else
            {
                if (account.IsBanned)
                {
                    account.IsBanned = false;
                    SendTo(user, "Account can now login.");
                }
                else
                    SendTo(user, "Account already enabled to login.");
            }
        }

        private static void AllUsersInRoom(User user, params string[] args)
        {
            if (user == null && args.Length == 0)
                return;

            string room = args.Length == 0 ? user?.Room.Name : args[0];

            var list = AccountManager.Accounts.Where(s => s.Room != null && s.Room.Name == room).ToList();

            foreach (var s in list)
                SendTo(user, $"{s.Name} - {s.AccountLevel}");
        }

        private static void AllUsers(User user, params string[] args)
        {
            var list = AccountManager.Accounts.ToList();
            foreach (var s in list)
                SendTo(user, string.Format("|{0,24}|{1,24}|{2,24}|", s.Name, s.Room.Name, s.AccountLevel), true);
        }

        private static void Statistics(User user, params string[] args)
        {
        }

        private static void SetPrivileges(User user, params string[] args)
        {
            if (args.Length < 2)
                return;

            Account account = AccountManager.Get(args[0]);
            if (account == null)
                SendTo(user, "Account not exists.");
            else if (user != null && account == user.Account)
                SendTo(user, "Cannot change your account privileges.");
            else
            {
                if (!int.TryParse(args[1], out int level) || level < 0 || level > 2)
                    SendTo(user, "Wrong data inserted.");
                else
                {
                    ACCOUNT_LEVEL acclevel = (ACCOUNT_LEVEL) level;
                    account.AccountLevel = acclevel;
                    SendTo(user, $"New privileges setted right: {account.Name} - {acclevel}");
                }
            }
        }

        private static void SetLoginSystem(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            if (int.TryParse(args[0], out int result) && (result == 0 || result == 1))
            {
                SettingsManager.Configuration.CredentialsSystem = (CREDENTIAL_SYSTEM) result;
                SendTo(user, "Done");
            }
            else
                SendTo(user, "Something wrong...");
        }

        private static void SetRoomPassword(User user, params string[] args)
        {
            if (args.Length < 2 || SettingsManager.Configuration.CredentialsSystem == CREDENTIAL_SYSTEM.USERNAME_AND_ID)
                return;

            string name = args[0];
            string password = args[1].Trim();

            if (string.IsNullOrEmpty(name))
                SendTo(user, "Null name.");
            else if (string.IsNullOrEmpty(password))
                SendTo(user, "Null password.");
            else if (RoomManager.Rooms.Any(s => s.Password == password))
                SendTo(user, $"Password '{password}' already setted for another room. Please insert a different password.");
            else
            {
                Room room = RoomManager.Get(name);
                if (room == null)
                    SendTo(user, "Room not exists.");
                else
                {
                    room.Password = password;
                    SendTo(user, "Done!");
                }
            }
        }

        private static void GetRoomPassword(User user, params string[] args)
        {
            if (args.Length < 1 || SettingsManager.Configuration.CredentialsSystem == CREDENTIAL_SYSTEM.USERNAME_AND_ID)
                return;

            Room room = RoomManager.Get(args[0]);
            if (room == null)
                SendTo(user, "Room not exists.");
            else
                SendTo(user, string.IsNullOrEmpty(room.Password) ? "Password not exists for this room" : "Password: " + room.Password);
        }

        private static void SendMessage(User user, params string[] args)
        {
            if (args.Length < 1)
                return;

            SendTo(user, args[0]);
        }
    }
}