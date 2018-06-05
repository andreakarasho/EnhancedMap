using EnhancedMap.Core.MapObjects;
using EnhancedMap.Diagnostic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.Core
{
    public enum MSG_RECV
    {
        USELESS_0 = 0x400 + 301,
        USELESS_1,
        LOGIN,
        USELESS_2,
        MANA_MAXMANA,
        USELESS_3,
        USELESS_4,
        LOGOUT,
        HP_MAXHP,
        STAM_MAXSTAM,
        HOUSES_BOATS_INFO,
        DEL_HOUSES_BOATS_INFO,
        FACET_CHANGED,
        USELESS_5,
        USELESS_6,

        // custom
        MOBILE_ADD_STRCT,
        MOBILE_REMOVE_STRCT
    }

    public enum MSG_SEND
    {
        ATTACH_TO_UO = 0x400 + 200,
        USELESS_0,
        GET_LOCATION_INFO,
        USELESS_1,
        GET_STATS_INFO,
        USELESS_2,
        USELESS_3,
        SEND_SYS_MSG,
        GET_HOUSES_BOATS_INFO,
        ADD_CMD,
        GET_PLAYER_SERIAL,
        GET_SHARD_NAME,
        USELESS_4,
        GET_UO_HWND,
        GET_FLAGS,
        USELESS_5,
        USELESS_6,

        // custom
        GET_FACET = 0x400 + 500,
        GET_HP,
        GET_STAM,
        GET_MANA,
        GET_MOBILES
    }


    public static class UOClientManager
    {
        const string CLASS_NAME = "UOASSIST-TP-MSG-WND";
        const string UO_CLASSIC_CLIENT = "client";
        const string UO_ENHANCED_CLIENT = "UOSA";
        const string UO_ORION_CLIENT = "OrionUO";


        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        private static IntPtr _maphWnd;
        private static bool _isScanning;

        static UOClientManager()
        {

        }

        public static bool IsAttached { get; private set; }
        public static IntPtr hWnd { get; private set; }

        public static void Initialize(IntPtr maphWnd)
        {
            _maphWnd = maphWnd;
        }

        public static FormWindowState GetProcessWindowState(FormWindowState current)
        {
            int c = Global.SettingsCollection["clienttype"].ToInt();
            string clientName = string.Empty;
            switch (c)
            {
                case 0:
                    clientName = UO_CLASSIC_CLIENT;
                    break;
                case 1:
                    clientName = UO_ENHANCED_CLIENT; 
                    break;
                case 2:
                    clientName = UO_ORION_CLIENT;
                    break;
                default:
                    return current;
            }

            Process[] p = Process.GetProcessesByName(clientName);
            if (p.Length <= 0)
                return current;

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(p[0].MainWindowHandle, ref placement);

            switch (placement.showCmd)
            {
                case ShowWindowCommands.Minimized:
                    return FormWindowState.Minimized;
                default:
                    return FormWindowState.Normal;
            }
        }

        public static bool AttachToClient(IntPtr ptr = default)
        {
            if (_isScanning)
                return false;

            _isScanning = true;

            if (Global.SettingsCollection["clienttype"].ToInt() == 1)
            {
                Process[] clients = Process.GetProcessesByName(UO_ENHANCED_CLIENT);
                if (clients.Length > 0)
                {
                    foreach (Process p in clients)
                    {
                        if (ptr != default && ptr != p.MainWindowHandle)
                            continue;

                        if (IsAttached && ptr == default)
                        {
                            _isScanning = false;
                            return true;
                        }

                        IsAttached = true;
                        hWnd = p.MainWindowHandle;
                        return true;
                    }
                }
                else
                {
                    if (IsAttached)
                        IsAttached = false;
                    _isScanning = false;
                    return false;
                }
            }
            else
            {
                // nb: HANDLE is ASSISTUO WINDOW, NOT CLIENT.EXE!!
                List<IntPtr> clients = FindWindowsWithText(CLASS_NAME).ToList();

                if (clients.Count == 0)
                {
                    if (IsAttached)
                    {
                        IsAttached = false;
                    }
                    _isScanning = false;
                    return false;
                }

                if (IsAttached && ptr == default)
                {
                    _isScanning = false;
                    return true;
                }

                foreach (IntPtr hwnd in clients)
                {
                    if (ptr != default && ptr != hwnd)
                        continue;

                    GetWindowThreadProcessId(hwnd, out uint pid);
                    Process p = Process.GetProcessById((int)pid);
                    if (p != null)
                    {
                        Logger.Good("Assistants founds: " + p.MainWindowTitle);

                        StringBuilder sb = new StringBuilder(510);
                        int f = SendMessageA(hwnd, 13, 510, sb);
                        f = SendMessage(hwnd, (int)MSG_SEND.ATTACH_TO_UO, _maphWnd.ToInt32(), 1);

                        if (f > 1)
                        {
                            int f1 = f;
                            for (int i = 0; i < f1; i++)
                                f = SendMessage(hwnd, (int)MSG_SEND.ATTACH_TO_UO, _maphWnd.ToInt32(), 1);
                        }

                        if (f == 1)
                        {
                            f = SendMessage(hwnd, (int)MSG_SEND.GET_HOUSES_BOATS_INFO, 0, 0);
                            if (f != 1)
                                Logger.Warn("Unable to get Houses and Boats info from assistants. Probably assistants doesn't support this feature.");

                            OEUO_Manager.Attach();
                            IsAttached = true;
                            hWnd = hwnd;
                            _isScanning = false;

                            //Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_MOBILES, 0, 0);

                            return true;
                        }

                    }
                }
            }


            _isScanning = false;
            return false;
        }

        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }


        private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr h, IntPtr lParam)
            {
                if (filter(h, lParam))
                {
                   // Logger.Log("ASSISTUO-CLASS: 0x" + h.ToString("X"));
                    windows.Add(h);
                }
                return true;

            }, IntPtr.Zero);
            return windows;
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return string.Empty;
        }

        public static Dictionary<IntPtr, string> GetClientsWindowTitles()
        {
            _isScanning = true;

            List<IntPtr> clients = Global.SettingsCollection["clienttype"].ToInt() == 1 ? Process.GetProcessesByName(UO_ENHANCED_CLIENT).Select(s => s.MainWindowHandle).ToList() : FindWindowsWithText(CLASS_NAME).ToList();
            Dictionary<IntPtr, string> titles = new Dictionary<IntPtr, string>();

            foreach (IntPtr ptr in clients)
            {
                GetWindowThreadProcessId(ptr, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                if (p != null)
                {
                    titles[ptr] = p.MainWindowTitle;
                }
            }
            _isScanning = false;

            return titles;
        }

        public static void SysMessage(string msg, int col = 999)
        {
            if (hWnd != IntPtr.Zero && IsAttached)
                SendMessage(hWnd, (int)MSG_SEND.SEND_SYS_MSG, Get_Hiword_Loword(1, col), GlobalAddAtom(msg));
        }

        private static int Get_Hiword_Loword(int hi, int lo)
        {
            int i = hi * 65536;
            i = i | (lo & 65535);
            return i;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern short GlobalAddAtom(string atomName);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int SendMessageA(IntPtr hWnd, int msg, int wParam, StringBuilder sb);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }
    }
}
