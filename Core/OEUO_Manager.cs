using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core
{
    public static class OEUO_Manager
    {
        private static IntPtr _clientHandle;

        public static int ClientIndex { get; set; }
        public static bool IsOpen => _clientHandle != IntPtr.Zero && CliNr > 0;
        public static IntPtr ClientHwnd => _clientHandle;

        public static bool Attach(int c = 1)
        {
            if (!File.Exists("uo.dll") || Version() != 3)
                return false;

            try
            {
                Close();
            }
            catch { }

            if (c < 1)
                c = 1;

            _clientHandle = Open();
            if (_clientHandle == IntPtr.Zero)
                return false;

            SetTop(_clientHandle, 0);
            PushStrVal(_clientHandle, "Set");
            PushStrVal(_clientHandle, "CliNr");
            PushInteger(_clientHandle, c);
            if (Execute(_clientHandle) != 0)
                return false;

            InitGet();
            PushStrVal(_clientHandle, "CliNr");
            if (Execute(_clientHandle) != 0)
                return false;

            if (GetInteger(_clientHandle, c) <= 0)
                return false;

            ClientIndex = c;

            return true;
        }

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

        public static void Close()
        {
            if (!File.Exists("uo.dll") || _clientHandle == IntPtr.Zero)
                return;
            Close(_clientHandle);
        }

        private static void InitGet()
        {
            SetTop(_clientHandle, 0);
            PushStrVal(_clientHandle, "Get");
        }

        private static bool ReadBool(string cmd)
        {
            InitGet();
            PushStrVal(_clientHandle, cmd);
            return Execute(_clientHandle) == 0 && GetBoolean(_clientHandle, ClientIndex);
        }

        private static int ReadInt(string cmd)
        {
            InitGet();
            PushStrVal(_clientHandle, cmd);
            return Execute(_clientHandle) == 0 ? GetInteger(_clientHandle, ClientIndex) : 0;
        }

        private static string ReadString(string cmd)
        {
            InitGet();
            PushStrVal(_clientHandle, cmd);
            return Execute(_clientHandle) == 0 ? Marshal.PtrToStringAnsi(GetString(_clientHandle, ClientIndex)) : string.Empty;
        }

        private static void WriteInt(string cmd, int value)
        {
            InitGet();
            PushStrVal(_clientHandle, cmd);
            PushInteger(_clientHandle, value);
            Execute(_clientHandle);
        }

        private const string DLL = "uo.dll";

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
