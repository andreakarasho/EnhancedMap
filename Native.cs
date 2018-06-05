using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;


namespace EnhancedMap
{
    public static class Native
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTBOTTOM = 15;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int BORDER_WIDTH = 7;
        public const int WMSZ_TOP = 3;
        public const int WMSZ_TOPLEFT = 4;
        public const int WMSZ_TOPRIGHT = 5;
        public const int WMSZ_LEFT = 1;
        public const int WMSZ_RIGHT = 2;
        public const int WMSZ_BOTTOM = 6;
        public const int WMSZ_BOTTOMLEFT = 7;
        public const int WMSZ_BOTTOMRIGHT = 8;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MONITORINFOEX
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];
        }

        public static readonly Dictionary<int, int> _resizingLocationsToCmd = new Dictionary<int, int>
        {
            {HTTOP,         WMSZ_TOP},
            {HTTOPLEFT,     WMSZ_TOPLEFT},
            {HTTOPRIGHT,    WMSZ_TOPRIGHT},
            {HTLEFT,        WMSZ_LEFT},
            {HTRIGHT,       WMSZ_RIGHT},
            {HTBOTTOM,      WMSZ_BOTTOM},
            {HTBOTTOMLEFT,  WMSZ_BOTTOMLEFT},
            {HTBOTTOMRIGHT, WMSZ_BOTTOMRIGHT}
        };

        public const int STATUS_BAR_BUTTON_WIDTH = STATUS_BAR_HEIGHT;
        public const int STATUS_BAR_HEIGHT = 25;
        public const int ACTION_BAR_HEIGHT = 40;

        public const uint TPM_LEFTALIGN = 0x0000;
        public const uint TPM_RETURNCMD = 0x0100;

        public const int WM_SYSCOMMAND = 0x0112;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_SYSMENU = 0x00080000;

        public const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern unsafe void* memcpy(void* to, void* from, int len);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern int WaitForSingleObject(IntPtr hHandle, int timeout = -1);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern uint SignalObjectAndWait(IntPtr hToSignal, IntPtr hToWaitOn, int timeout = -1, bool alertable = false);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateProcess(
           [MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, StringBuilder lpCommandLine, SECURITY_ATTRIBUTES lpProcessAttributes,
           SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
           [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, STARTUPINFO lpStartupInfo, PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool inherit, int pId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr address, IntPtr size, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, IntPtr size, IntPtr written);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr address, [Out] byte[] buffer, IntPtr size, IntPtr read);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, IntPtr dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, IntPtr size, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(IntPtr mmf, uint access, uint offsetHigh, uint offsetLow, IntPtr toMap);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [In][Out] IntPtr[] modules, uint size, out uint needed, uint flags);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        public sealed class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle;
        }
        [StructLayout(LayoutKind.Sequential)]
        public sealed class STARTUPINFO
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);
            public STARTUPINFO()
            {
                this.cb = Marshal.SizeOf(this);
            }

            public void Dispose()
            {
                if ((this.hStdInput != null) && !this.hStdInput.IsInvalid)
                {
                    this.hStdInput.Close();
                    this.hStdInput = null;
                }
                if ((this.hStdOutput != null) && !this.hStdOutput.IsInvalid)
                {
                    this.hStdOutput.Close();
                    this.hStdOutput = null;
                }
                if ((this.hStdError != null) && !this.hStdError.IsInvalid)
                {
                    this.hStdError.Close();
                    this.hStdError = null;
                }
            }
        }

        private static bool IsNt
        {
            get { return (Environment.OSVersion.Platform == PlatformID.Win32NT); }
        }

        public const uint CREATE_SUSPENDED = 0x00000004;
        public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        public const uint CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000;
        public const uint CREATE_DEFAULT_ERROR_MODE = 0x04000000;
        public const uint CREATE_NO_WINDOW = 0x08000000;
        public const int INVALID_HANDLE_VALUE = -1;

        internal sealed class OrdinalCaseInsensitiveComparer : IComparer
        {
            public static readonly OrdinalCaseInsensitiveComparer Default = new OrdinalCaseInsensitiveComparer();
            public int Compare(object a, object b)
            {
                string str = a as string;
                string str2 = b as string;
                if ((str != null) && (str2 != null))
                {
                    return string.CompareOrdinal(str.ToUpperInvariant(), str2.ToUpperInvariant());
                }
                return Comparer.Default.Compare(a, b);
            }
        }

        public sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeProcessHandle() : base(true) { }

            public SafeProcessHandle(IntPtr handle)
                : base(true)
            {
                base.SetHandle(handle);
            }

            internal void InitialSetHandle(IntPtr h)
            {
                base.handle = h;
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }

        public sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeThreadHandle() : base(true) { }

            internal void InitialSetHandle(IntPtr h)
            {
                base.SetHandle(h);
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }

        public sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeLocalMemHandle() : base(true) { }

            public SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                base.SetHandle(existingHandle);
            }

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string StringSecurityDescriptor, int StringSDRevision, out SafeLocalMemHandle pSecurityDescriptor, IntPtr SecurityDescriptorSize);

            [DllImport("kernel32.dll")]
            private static extern IntPtr LocalFree(IntPtr hMem);

            protected override bool ReleaseHandle()
            {
                return (LocalFree(base.handle) == IntPtr.Zero);
            }
        }

        public static byte[] EnvironmentToByteArray(StringDictionary sd, bool unicode)
        {
            string[] array = new string[sd.Count];
            byte[] bytes = null;
            sd.Keys.CopyTo(array, 0);
            string[] strArray2 = new string[sd.Count];
            sd.Values.CopyTo(strArray2, 0);
            Array.Sort(array, strArray2, OrdinalCaseInsensitiveComparer.Default);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < sd.Count; i++)
            {
                builder.Append(array[i]);
                builder.Append('=');
                builder.Append(strArray2[i]);
                builder.Append('\0');
            }
            builder.Append('\0');
            if (unicode)
            {
                bytes = Encoding.Unicode.GetBytes(builder.ToString());
            }
            else
            {
                bytes = Encoding.Default.GetBytes(builder.ToString());
            }
            if (bytes.Length > 0xFFFF)
            {
                throw new InvalidOperationException("EnvironmentBlockTooLong");
            }
            return bytes;
        }

        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
        {
            StringBuilder sb = new StringBuilder();
            executableFileName = executableFileName.Trim();
            bool ready = executableFileName.StartsWith("\"", StringComparison.Ordinal) && executableFileName.EndsWith("\"", StringComparison.Ordinal);
            if (!ready)
            {
                sb.Append("\"");
            }
            sb.Append(executableFileName);
            if (!ready)
            {
                sb.Append("\"");
            }
            if (!string.IsNullOrEmpty(arguments))
            {
                sb.Append(" ");
                sb.Append(arguments);
            }
            return sb;
        }

        public static bool CreateProcess(ProcessStartInfo startInfo, bool createSuspended, out SafeProcessHandle process, out SafeThreadHandle thread, out int processID, out int threadID)
        {
            StringBuilder cmdLine = BuildCommandLine(startInfo.FileName, startInfo.Arguments);
            STARTUPINFO lpStartupInfo = new STARTUPINFO();
            PROCESS_INFORMATION lpProcessInformation = new PROCESS_INFORMATION();
            SafeProcessHandle processHandle = new SafeProcessHandle();
            SafeThreadHandle threadHandle = new SafeThreadHandle();
            GCHandle environment = new GCHandle();
            uint creationFlags = 0;
            bool success;

            creationFlags = CREATE_DEFAULT_ERROR_MODE | CREATE_PRESERVE_CODE_AUTHZ_LEVEL;


            if (createSuspended)
                creationFlags |= CREATE_SUSPENDED;

            if (startInfo.CreateNoWindow)
                creationFlags |= CREATE_NO_WINDOW;

            IntPtr pinnedEnvironment = IntPtr.Zero;

            if (startInfo.EnvironmentVariables != null)
            {
                bool unicode = false;
                if (IsNt)
                {
                    creationFlags |= CREATE_UNICODE_ENVIRONMENT;
                    unicode = true;
                }
                environment = GCHandle.Alloc(EnvironmentToByteArray(startInfo.EnvironmentVariables, unicode), GCHandleType.Pinned);
                pinnedEnvironment = environment.AddrOfPinnedObject();
            }

            string workingDirectory = startInfo.WorkingDirectory;
            if (workingDirectory == "")
                workingDirectory = Environment.CurrentDirectory;

            success = CreateProcess(null, cmdLine, null, null, false, creationFlags, pinnedEnvironment, workingDirectory, lpStartupInfo, lpProcessInformation);

            if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess.ToInt32() != INVALID_HANDLE_VALUE))
                processHandle.InitialSetHandle(lpProcessInformation.hProcess);

            if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread.ToInt32() != INVALID_HANDLE_VALUE))
                threadHandle.InitialSetHandle(lpProcessInformation.hThread);

            if (environment.IsAllocated)
                environment.Free();

            lpStartupInfo.Dispose();

            if (success && !processHandle.IsInvalid && !threadHandle.IsInvalid)
            {
                process = processHandle;
                thread = threadHandle;
                processID = (int)lpProcessInformation.dwProcessId;
                threadID = (int)lpProcessInformation.dwThreadId;
                return true;
            }

            process = null;
            thread = null;
            processID = 0;
            threadID = 0;
            return false;
        }


        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);
        [DllImport("uxtheme.dll", ExactSpelling = true)]
        public static extern Int32 CloseThemeData(IntPtr hTheme);
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenThemeData(IntPtr hWnd, String classList);
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);
        [DllImport("user32")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern int BitBlt(
      IntPtr hdcDest,
      int nXDest,
      int nYDest,
      int nWidth,         // width of destination rectangle
      int nHeight,        // height of destination rectangle
      IntPtr hdcSrc,      // handle to source DC
      int nXSrc,          // x-coordinate of source upper-left corner
      int nYSrc,          // y-coordinate of source upper-left corner
      int dwRop  // raster operation code
      );
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSysColorBrush(int nIndex);
        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);
        [DllImport("uxtheme.dll", ExactSpelling = true)]
        public static extern Int32 DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, IntPtr pClipRect);
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }
        public enum SystemMetric
        {
            SM_CXSCREEN = 0,  // 0x00
            SM_CYSCREEN = 1,  // 0x01
            SM_CXVSCROLL = 2,  // 0x02
            SM_CYHSCROLL = 3,  // 0x03
            SM_CYCAPTION = 4,  // 0x04
            SM_CXBORDER = 5,  // 0x05
            SM_CYBORDER = 6,  // 0x06
            SM_CXDLGFRAME = 7,  // 0x07
            SM_CXFIXEDFRAME = 7,  // 0x07
            SM_CYDLGFRAME = 8,  // 0x08
            SM_CYFIXEDFRAME = 8,  // 0x08
            SM_CYVTHUMB = 9,  // 0x09
            SM_CXHTHUMB = 10, // 0x0A
            SM_CXICON = 11, // 0x0B
            SM_CYICON = 12, // 0x0C
            SM_CXCURSOR = 13, // 0x0D
            SM_CYCURSOR = 14, // 0x0E
            SM_CYMENU = 15, // 0x0F
            SM_CXFULLSCREEN = 16, // 0x10
            SM_CYFULLSCREEN = 17, // 0x11
            SM_CYKANJIWINDOW = 18, // 0x12
            SM_MOUSEPRESENT = 19, // 0x13
            SM_CYVSCROLL = 20, // 0x14
            SM_CXHSCROLL = 21, // 0x15
            SM_DEBUG = 22, // 0x16
            SM_SWAPBUTTON = 23, // 0x17
            SM_CXMIN = 28, // 0x1C
            SM_CYMIN = 29, // 0x1D
            SM_CXSIZE = 30, // 0x1E
            SM_CYSIZE = 31, // 0x1F
            SM_CXSIZEFRAME = 32, // 0x20
            SM_CXFRAME = 32, // 0x20
            SM_CYSIZEFRAME = 33, // 0x21
            SM_CYFRAME = 33, // 0x21
            SM_CXMINTRACK = 34, // 0x22
            SM_CYMINTRACK = 35, // 0x23
            SM_CXDOUBLECLK = 36, // 0x24
            SM_CYDOUBLECLK = 37, // 0x25
            SM_CXICONSPACING = 38, // 0x26
            SM_CYICONSPACING = 39, // 0x27
            SM_MENUDROPALIGNMENT = 40, // 0x28
            SM_PENWINDOWS = 41, // 0x29
            SM_DBCSENABLED = 42, // 0x2A
            SM_CMOUSEBUTTONS = 43, // 0x2B
            SM_SECURE = 44, // 0x2C
            SM_CXEDGE = 45, // 0x2D
            SM_CYEDGE = 46, // 0x2E
            SM_CXMINSPACING = 47, // 0x2F
            SM_CYMINSPACING = 48, // 0x30
            SM_CXSMICON = 49, // 0x31
            SM_CYSMICON = 50, // 0x32
            SM_CYSMCAPTION = 51, // 0x33
            SM_CXSMSIZE = 52, // 0x34
            SM_CYSMSIZE = 53, // 0x35
            SM_CXMENUSIZE = 54, // 0x36
            SM_CYMENUSIZE = 55, // 0x37
            SM_ARRANGE = 56, // 0x38
            SM_CXMINIMIZED = 57, // 0x39
            SM_CYMINIMIZED = 58, // 0x3A
            SM_CXMAXTRACK = 59, // 0x3B
            SM_CYMAXTRACK = 60, // 0x3C
            SM_CXMAXIMIZED = 61, // 0x3D
            SM_CYMAXIMIZED = 62, // 0x3E
            SM_NETWORK = 63, // 0x3F
            SM_CLEANBOOT = 67, // 0x43
            SM_CXDRAG = 68, // 0x44
            SM_CYDRAG = 69, // 0x45
            SM_SHOWSOUNDS = 70, // 0x46
            SM_CXMENUCHECK = 71, // 0x47
            SM_CYMENUCHECK = 72, // 0x48
            SM_SLOWMACHINE = 73, // 0x49
            SM_MIDEASTENABLED = 74, // 0x4A
            SM_MOUSEWHEELPRESENT = 75, // 0x4B
            SM_XVIRTUALSCREEN = 76, // 0x4C
            SM_YVIRTUALSCREEN = 77, // 0x4D
            SM_CXVIRTUALSCREEN = 78, // 0x4E
            SM_CYVIRTUALSCREEN = 79, // 0x4F
            SM_CMONITORS = 80, // 0x50
            SM_SAMEDISPLAYFORMAT = 81, // 0x51
            SM_IMMENABLED = 82, // 0x52
            SM_CXFOCUSBORDER = 83, // 0x53
            SM_CYFOCUSBORDER = 84, // 0x54
            SM_TABLETPC = 86, // 0x56
            SM_MEDIACENTER = 87, // 0x57
            SM_STARTER = 88, // 0x58
            SM_SERVERR2 = 89, // 0x59
            SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
            SM_CXPADDEDBORDER = 92, // 0x5C
            SM_DIGITIZER = 94, // 0x5E
            SM_MAXIMUMTOUCHES = 95, // 0x5F

            SM_REMOTESESSION = 0x1000, // 0x1000
            SM_SHUTTINGDOWN = 0x2000, // 0x2000
            SM_REMOTECONTROL = 0x2001, // 0x2001


            SM_CONVERTABLESLATEMODE = 0x2003,
            SM_SYSTEMDOCKED = 0x2004,
        }
        public struct Point
        {
            public int x;
            public int y;
        }
        public struct WindowPlacement
        {
            public uint length;
            public uint flags;
            public uint showCmd;
            public Point minPosition;
            public Point maxPosition;
            public RECT normalPosition;


        }
        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        [Flags]
        public enum DWMNCRenderingPolicy : uint
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }

    }
}
