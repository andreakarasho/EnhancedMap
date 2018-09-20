using System;
using System.Net;
using System.Threading;
using EnhancedMapServerNetCore.Configuration;

namespace EnhancedMapServerNetCore.Managers
{
    public static class ConsoleManager
    {
        private static readonly string _helpString = @"------ COMMANDS ------
    - /statistics         if ' - profile' args is enabled, get all stats of in/out packets
    - /save               Save all accounts, rooms and server's settings
    - /adduser            Add an user
    - /removeuser         Remove an user
    - /setpassword        Set a password for an user
    - /setroom            Set a room for an user
    - /addroom            Add a room
    - /removeroom         Remove a room
    - /allusers           Show all users saved into XML
    - /allrooms           Show all rooms saved into XML
    - /allusersinroom     Show all users of a specific room
    - /setloginsys        ID + PSW or PSW for each room
    - /getroompassword    Get password for a specific room
    - /setroompassword    Set password for a specific room
    - /allusersonline     Show all connected users
    - /userinfo           Get info about an user
    - /kick               Kick an user from the server
    - /ban                Ban and kick an user from the server
    - /enableuser         Remove ban status
    - /getport            Get the current server port
    - /setport            Set the server port
    - /setkicktime        Set after how many seconds an user can relog
    - /sendmsg            Send a game message to all users
    - /setprivileges      Set account privileges
    - /restart            Restart server
    - /exit               Close server";

        public static void Init()
        {
            ThreadPool.QueueUserWorkItem(a => Read(Console.ReadLine()));
        }


        private static void Read(string input)
        {
            const char FIRST = '/';

            if (string.IsNullOrEmpty(input) || input.Length == 1 || input[0] != FIRST)
                Console.WriteLine(_helpString);
            else
            {
                input = input.ToLower().Remove(0, 1);

                switch (input)
                {
                    case "help":
                    default:
                        Console.WriteLine(_helpString);
                        break;


                    case "save":
                        SaveManager.Save();
                        break;

                    case "allusersonline":
                    case "getroompassword":
                    case "getport":
                    case "statistics":
                    case "allusers":
                    case "allrooms":
                        CommandManager.ExecuteFromConsole(input);
                        break;

                    case "adduser":
                        string[] args = new string[4];

                        Console.Write("Username: ");
                        string result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[0] = result;

                        Console.Write("Password: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[1] = result;

                        Console.Write("Room: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[2] = result;

                        Console.Write("Account level [write '0' for 'Normal', '1' for 'RoomAdmin' or '2' for 'ServerAdmin']: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[3] = result;

                        CommandManager.ExecuteFromConsole(input, args);
                        break;
                    case "removeuser":
                        Console.Write("Username: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] {result};

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "setroompassword":
                    case "addroom":
                        args = new string[2];

                        Console.Write("Room: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[0] = result;

                        if (SettingsManager.Configuration.CredentialsSystem == CREDENTIAL_SYSTEM.ONLY_PASSWORD)
                        {
                            Console.Write("Password: ");
                            result = string.Empty;
                            while (string.IsNullOrEmpty(result))
                                result = Console.ReadLine().Trim();
                            args[1] = result;
                        }

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "allusersinroom":
                    case "removeroom":
                        Console.Write("Room: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] {result};

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "kick":
                    case "userinfo":
                    case "ban":
                    case "enableuser":
                        Console.Write("Username: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] {result};

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "setkicktime":
                        Console.Write("Time (seconds): ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] {result};

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "setpassword":
                        args = new string[2];

                        Console.Write("Username: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[0] = result;

                        Console.Write("Password: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[1] = result;

                        CommandManager.ExecuteFromConsole(input, args);
                        break;
                    case "setroom":
                        args = new string[2];

                        Console.Write("Username: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[0] = result;

                        Console.Write("Room: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[1] = result;

                        CommandManager.ExecuteFromConsole(input, args);
                        break;

                    case "setport":
                        Console.Write("Port (needs a restart of server): ");
                        result = string.Empty;
                        ushort port;
                        while (string.IsNullOrEmpty(result) || !ushort.TryParse(result, out port) || port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                            result = Console.ReadLine().Trim();

                        SettingsManager.Configuration.Port = port;
                        Core.Restart();

                        break;

                    case "exit":
                        Core.Exit();
                        break;
                    case "restart":
                        Core.Restart();
                        break;
                    case "sendmsg":
                        Console.Write("Message: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] {result};
                        CommandManager.ExecuteFromConsole(input, args);
                        break;
                    case "setprivileges":
                        args = new string[2];

                        Console.Write("Username: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[0] = result;

                        Console.Write("Account level [write '0' for 'Normal', '1' for 'RoomAdmin' or '2' for 'ServerAdmin']: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args[1] = result;

                        CommandManager.ExecuteFromConsole(input, args);
                        break;
                    case "setloginsys":


                        break;

                    case "setmaxusersconnection":
                        Console.Write("Max users: ");
                        result = string.Empty;
                        while (string.IsNullOrEmpty(result))
                            result = Console.ReadLine().Trim();
                        args = new string[1] { result };
                        CommandManager.ExecuteFromConsole(input, args);

                        break;
                }
            }

            Init();
        }
    }
}