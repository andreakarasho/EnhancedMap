using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EnhancedMap.Core
{
    public static class OEUO_Manager
    {
        private const string DLL = "uo.dll";

        public static int ClientIndex { get; set; }
        public static bool IsOpen => ClientHwnd != IntPtr.Zero && CliNr > 0;
        public static IntPtr ClientHwnd { get; private set; }

        public static int CliTop => ReadInt("CliTop");
        public static int CliLeft => ReadInt("CliLeft");
        public static int CliXRes => ReadInt("CliXRes");
        public static int CliYRes => ReadInt("CliYRes");
        public static string CharStatus => ReadString("CharStatus");
        public static int Hits => ReadInt("Hits");
        public static int MaxHits => ReadInt("MaxHits");
        public static int Stamina => ReadInt("Stamina");
        public static int MaxStamina => ReadInt("MaxStam");
        public static int Mana => ReadInt("Mana");
        public static int MaxMana => ReadInt("MaxMana");
        public static int CliNr => ReadInt("CliNr");
        public static bool CliLogged => ReadBool("CliLogged");
        public static int CursKind => ReadInt("CursKind");

        public static bool Attach(int c = 1)
        {
            if (!File.Exists("uo.dll") || Version() != 3)
                return false;

            try
            {
                Close();
            }
            catch
            {
            }

            if (c < 1)
                c = 1;

            ClientHwnd = Open();
            if (ClientHwnd == IntPtr.Zero)
                return false;

            SetTop(ClientHwnd, 0);
            PushStrVal(ClientHwnd, "Set");
            PushStrVal(ClientHwnd, "CliNr");
            PushInteger(ClientHwnd, c);
            if (Execute(ClientHwnd) != 0)
                return false;

            InitGet();
            PushStrVal(ClientHwnd, "CliNr");
            if (Execute(ClientHwnd) != 0)
                return false;

            if (GetInteger(ClientHwnd, c) <= 0)
                return false;

            ClientIndex = c;

            return true;
        }

        public static void Close()
        {
            if (!File.Exists("uo.dll") || ClientHwnd == IntPtr.Zero)
                return;
            Close(ClientHwnd);
        }

        private static void InitGet()
        {
            SetTop(ClientHwnd, 0);
            PushStrVal(ClientHwnd, "Get");
        }

        private static bool ReadBool(string cmd)
        {
            InitGet();
            PushStrVal(ClientHwnd, cmd);
            return Execute(ClientHwnd) == 0 && GetBoolean(ClientHwnd, ClientIndex);
        }

        private static int ReadInt(string cmd)
        {
            InitGet();
            PushStrVal(ClientHwnd, cmd);
            return Execute(ClientHwnd) == 0 ? GetInteger(ClientHwnd, ClientIndex) : 0;
        }

        private static string ReadString(string cmd)
        {
            InitGet();
            PushStrVal(ClientHwnd, cmd);
            return Execute(ClientHwnd) == 0 ? Marshal.PtrToStringAnsi(GetString(ClientHwnd, ClientIndex)) : string.Empty;
        }

        private static void WriteInt(string cmd, int value)
        {
            InitGet();
            PushStrVal(ClientHwnd, cmd);
            PushInteger(ClientHwnd, value);
            Execute(ClientHwnd);
        }

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr Open();

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int Version();

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void Close(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void Clean(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int Query(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int Execute(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetTop(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetType(IntPtr handle, int index);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void SetTop(IntPtr handle, int index);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void PushStrVal(IntPtr handle, string value);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void PushInteger(IntPtr handle, int value);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern void PushBoolean(IntPtr handle, bool value);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetString(IntPtr handle, int index);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetInteger(IntPtr handle, int index);

        [DllImport(DLL, CallingConvention = CallingConvention.Winapi)]
        public static extern bool GetBoolean(IntPtr handle, int index);
    }
}