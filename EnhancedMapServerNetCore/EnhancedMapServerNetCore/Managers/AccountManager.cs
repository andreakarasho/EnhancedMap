using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace EnhancedMapServerNetCore.Managers
{
    public static class AccountManager
    {
        private static readonly ConcurrentDictionary<string, Account> _accounts = new ConcurrentDictionary<string, Account>();

        public static IReadOnlyList<Account> Accounts => (IReadOnlyList<Account>)_accounts.Values;

        public static void Init()
        {
            Core.ServerInizialized += (sender, e) => { Load(); };

            Core.ServerShuttingDown += (sender, e) =>
            {
                if (e) // crashed
                {
                }
            };
        }

        public static Account Get(string name)
        {
            _accounts.TryGetValue(name, out var acc);
            return acc;
        }

        /// <summary>
        ///     Add new user by console or remote
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="roomname"></param>
        /// <param name="level"></param>
        public static void Add(string name, string password, string roomname, ACCOUNT_LEVEL level)
        {
            Account account = Get(name);
            if (account != null)
                Log.Message(LogTypes.Warning, $"Account '{name}' already exists.");
            else
            {
                Room room = RoomManager.Get(roomname);
                if (room == null)
                    Log.Message(LogTypes.Panic, $"Room '{roomname}' not exists.");
                else
                {
                    account = new Account(name, password, room, level, true);
                    if (!_accounts.TryAdd(name, account))
                        Log.Message(LogTypes.Panic, $"Impossible to add account '{name}'");
                }
            }
        }

        /// <summary>
        ///     Add user loaded from XML
        /// </summary>
        /// <param name="account"></param>
        public static void Add(Account account)
        {
            if (Get(account.Name) != null)
                Log.Message(LogTypes.Warning, $"Account already exists '{account.Name}'");
            else
            {
                if (!_accounts.TryAdd(account.Name, account))
                    Log.Message(LogTypes.Panic, $"Impossible to add account '{account.Name}'");
            }
        }

        /// <summary>
        ///     Delete an account
        /// </summary>
        /// <param name="name"></param>
        public static void Remove(string name)
        {
            if (!_accounts.TryRemove(name, out var account))
                Log.Message(LogTypes.Panic, $"Account '{name}' not exists");
        }


        public static bool CanConnect(string username, string password, Session session)
        {
            /*if (SettingsManager.Configuration.CredentialsSystem == Configuration.CREDENTIAL_SYSTEM.USERNAME_AND_ID)
            {

            }
            else if (SettingsManager.Configuration.CredentialsSystem == Configuration.CREDENTIAL_SYSTEM.ONLY_PASSWORD)
            {

            }
            else
            {
                throw new Exception("Wrong credentials system. WTF!?");
            }*/

            Account account = Get(username);
            if (account == null)
            {
                Log.Message(LogTypes.Trace, $"Account '{username}' not exists");
                return false;
            }

            if (!account.IsPasswordGood(password))
            {
                Log.Message(LogTypes.Trace, $"Wrong password for '{username}'");
                return false;
            }

            if (account.IsBanned)
            {
                Log.Message(LogTypes.Trace, $"Account '{username}' is banned.");
                return false;
            }

            if (account.IsKicked && account.KickTime > DateTime.Now)
            {
                Log.Message(LogTypes.Trace, $"Account '{username}' is kicked. Kick time left: {account.KickTime.Subtract(DateTime.Now)}");
                return false;
            }

            if (account.IsKicked)
                account.IsKicked = false;

            if (account.Room == null)
            {
                Log.Message(LogTypes.Trace, $"Account '{username}' has not a room assigned.");
                return false;
            }

            var connectedUser = UserManager.Get(username);
            if (connectedUser != null) connectedUser.Session.Dispose();

            Log.Message(LogTypes.Trace, "Account credentials confirmed for '" + username + "'.");

            if (!UserManager.Add(account, session)) return false;

            session.IsAccepted = true;

            session.Send(new PLoginResponse(account.AccountLevel, account.Name));
            User user = UserManager.Get(session.Guid);
            user.SendToUsersInRoom(new PUserConnection(username, true));
            user.RequestSharedLabels();

            return true;
        }

        public static void Load(bool isbackup = false)
        {
            Log.Message(LogTypes.Trace, "Loading accounts...");

            _accounts.Clear();

            string folderPath = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, "Accounts.xml");

            if (!File.Exists(path))
            {
                Log.Message(LogTypes.Warning, $"Accounts files not found at '{path}'");
                return;
            }

            XmlDocument doc = new XmlDocument();
            int loaded = 0;

            try
            {
                doc.Load(path);
                XmlElement root = doc["accounts"];

                foreach (XmlElement account in root.GetElementsByTagName("account"))
                {
                    try
                    {
                        Account a = new Account(account);
                        Add(a);
                        loaded++;
                    }
                    catch
                    {
                        Log.Message(LogTypes.Error, "Impossible to load account");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isbackup)
                {
                    Log.Message(LogTypes.Error, "Accounts.xml file is corrupted. Trying to restore from backup");
                    Load(true);
                    Log.Message(LogTypes.Error, "Accounts.xml restored.");
                }
                else
                    Log.Message(LogTypes.Error, "Impossible to restore Accounts.xml");

                return;
            }

            Log.Message(LogTypes.Trace, $"{loaded} accounts loaded.");
        }

        public static void Save(bool isbackup = false)
        {
            string path = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Accounts.xml");

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, IndentChars = "\t" };

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Saving accounts...");

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("accounts");

                foreach (Account a in _accounts.Values)
                    a.Save(writer);

                writer.WriteEndElement();
            }

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Accounts saved.");
        }
    }
}