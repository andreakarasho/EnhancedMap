using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Managers;
using EnhancedMapServerNetCore.Network;

namespace EnhancedMapServerNetCore
{
    public static class Core
    {
        private static readonly EventWaitHandle _waitHandle = new AutoResetEvent(false);
        private static Assembly _assembly;
        private static string _rootPath;
        private static ServerHandler _serverHandler;
        public static Server Server { get; private set; }
        public static byte Protocol => 0x02;

        public static string Arguments
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (HeadLess)
                    sb.Append("-headless ");
                if (External)
                    sb.Append("-external ");
                if (LogFile)
                    sb.Append("-log ");

                return sb.ToString();
            }
        }

        public static bool HeadLess { get; private set; }
        public static bool External { get; private set; }
        public static bool LogFile { get; private set; }

        public static string RootPath
        {
            get
            {
                if (_rootPath == null)
                    _rootPath = Path.GetDirectoryName(_assembly.Location);
                return _rootPath;
            }
        }

        public static bool IsRunning { get; private set; }


        public static event EventHandler ServerInizialized;
        public static event EventHandler<bool> ServerShuttingDown;


        private static void Main(string[] args)
        {
            LogFile file = null;

            foreach (string arg in args)
            {
                var a = arg.ToLower().Trim();
                if (a == "-headless")
                    HeadLess = true;
                else if (a == "-external")
                    External = true;
                else if (a == "-log")
                {
                    LogFile = true;
                    file = new LogFile(Environment.CurrentDirectory, "log.txt");
                }
            }

            _assembly = Assembly.GetEntryAssembly();


            Log.Start(LogTypes.All, file);
            Log.Message(LogTypes.None, "Server started.");
            Log.Message(LogTypes.None, "");
            Log.Message(LogTypes.None, "");

            Log.Message(LogTypes.None, "Info:");
            Log.Message(LogTypes.None, "\t- OS:\t" + RuntimeInformation.OSDescription + " " + RuntimeInformation.OSArchitecture);
            Log.Message(LogTypes.None, "\t- Process architecture:\t" + RuntimeInformation.ProcessArchitecture);
            Log.Message(LogTypes.None, "\t- Cores count:\t" + Environment.ProcessorCount);
            Log.Message(LogTypes.None, "\t- Root folder:\t" + RootPath);


            Log.Message(LogTypes.None, "");
            Log.Message(LogTypes.None, "");

            AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
            {
                string msg = e.ExceptionObject.ToString();

                Log.Message(LogTypes.Error, msg);

                using (LogFile crashfile = new LogFile(Environment.CurrentDirectory, "crash.txt"))
                {
                    await crashfile.WriteAsync(msg);
                }

                Shutdown(true);
            };


            SettingsManager.Init();
            RoomManager.Init();
            AccountManager.Init();


            IsRunning = true;
            ServerInizialized.Raise();

            if (RoomManager.Get("General") == null)
                RoomManager.Add(new Room("General", "general"));


            using (Server = new Server(SettingsManager.Configuration))
            {
                ServerHandler serverHandler = _serverHandler = new ServerHandler(Server);
                SaveManager.Init();
                ConsoleManager.Init();

                while (IsRunning)
                {
                    _waitHandle.WaitOne(1);

                    CoroutineManager.Update();

                    serverHandler.Slice();

                    Server.Flush();
                    Server.CheckSessionsAlive();
                    Server.ReleaseDisposed();
                }
            }

            Log.Message(LogTypes.Info, "Server closed.");
            _waitHandle.WaitOne();
        }

        public static void Set()
        {
            _waitHandle.Set();
        }

        public static void Exit()
        {
            Shutdown();
            Process.GetCurrentProcess().Kill();
        }

        public static void Restart()
        {
            Shutdown();
            ProcessStartInfo info = new ProcessStartInfo {FileName = "dotnet", WorkingDirectory = RootPath, Arguments = _assembly.GetName().Name + ".dll", UseShellExecute = true, RedirectStandardOutput = false, RedirectStandardError = false, CreateNoWindow = true};
            Process.Start(info);
            Process.GetCurrentProcess().Kill();
        }

        private static void Shutdown(bool crashed = false)
        {
            IsRunning = false;
            //Server.Stop();
            ServerShuttingDown.Raise(crashed);
            SettingsManager.Save(true);
            SettingsManager.Save();
            Log.Stop();
        }
    }
}