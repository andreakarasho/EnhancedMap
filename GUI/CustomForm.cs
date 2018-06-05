using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public class CustomForm : Form, ICustomControl
    {
        private enum ButtonState
        {
            None,

            XOver,
            MaxOver,
            MinOver,

            XDown,
            MaxDown,
            MinDown
        }

        private enum ResizeDirection
        {
            BottomLeft,
            Left,
            Right,
            BottomRight,
            Bottom,
            Top,
            TopLeft,
            TopRight,
            None
        }

        private readonly Cursor[] _resizeCursors = { Cursors.SizeNESW, Cursors.SizeWE, Cursors.SizeNWSE, Cursors.SizeWE, Cursors.SizeNS };

        private Rectangle _minimizeButton, _maximizeButton, _closeButton, _statusBarBounds;
        private ButtonState _currentState;
        private bool _maximized;
        private Size _previousSize;
        private Point _previousLocation;
        private bool _headerMouseDown;
        private ResizeDirection _resizeDir;

        private float _yTitle;

        public CustomForm()
        {
            BackColor = ColorsTable.Black3;
            FormBorderStyle = FormBorderStyle.None;
            CloseBox = true;

            Sizable = true;
            DoubleBuffered = true;

            this.MinimumSize = new Size(25 * 3 + 100, 25);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Application.AddMessageFilter(new MouseMessageFilter());
            MouseMessageFilter.MouseMove += OnGlobalMouseMove;
        }

        public bool CloseBox { get; set; }
        public bool Sizable { get; set; }
        public MouseState MouseState { get; set; }
        //public bool ShowStatusBar { get; set; }

        protected override CreateParams CreateParams
        {
            get
            {
                var par = base.CreateParams;
                // WS_SYSMENU: Trigger the creation of the system menu
                // WS_MINIMIZEBOX: Allow minimizing from taskbar
                par.Style = par.Style | Native.WS_MINIMIZEBOX | Native.WS_SYSMENU; // Turn on the WS_MINIMIZEBOX style flag
                return par;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            _yTitle = CreateGraphics().MeasureString("M", Font).Height;
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _minimizeButton = new Rectangle((Width - 14 / 2) - 3 * Native.STATUS_BAR_BUTTON_WIDTH, 0, Native.STATUS_BAR_BUTTON_WIDTH, Native.STATUS_BAR_BUTTON_WIDTH);
            _maximizeButton = new Rectangle((Width - 14 / 2) - 2 * Native.STATUS_BAR_BUTTON_WIDTH, 0, Native.STATUS_BAR_BUTTON_WIDTH, Native.STATUS_BAR_BUTTON_WIDTH);
            _closeButton = new Rectangle((Width - 14 / 2) - Native.STATUS_BAR_BUTTON_WIDTH, 0, Native.STATUS_BAR_BUTTON_WIDTH, Native.STATUS_BAR_BUTTON_WIDTH);

            _statusBarBounds = new Rectangle(0, 0, Width, 25);
            //_actionBarBounds = new Rectangle(0, 25, Width, 30);
        }

        protected void OnGlobalMouseMove(object sender, MouseEventArgs e)
        {
            if (IsDisposed) return;
            // Convert to client position and pass to Form.MouseMove
            var clientCursorPos = PointToClient(e.Location);
            var newE = new MouseEventArgs(MouseButtons.None, 0, clientCursorPos.X, clientCursorPos.Y, 0);
            OnMouseMove(newE);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            UpdateButtonState(e);
            if (e.Button == MouseButtons.Left)
                ResizeForm(_resizeDir);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            UpdateButtonState(e, true);
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _currentState = ButtonState.None;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Sizable && !_maximized)
            {
                //True if the mouse is hovering over a child control
                bool isChildUnderMouse = GetChildAtPoint(e.Location) != null;

                if (e.Location.X < Native.BORDER_WIDTH && e.Location.Y > Height - Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.BottomLeft;
                    Cursor = Cursors.SizeNESW;
                }
                else if (e.Location.X < Native.BORDER_WIDTH && e.Location.Y < Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.TopLeft;
                    Cursor = Cursors.SizeNWSE;
                }
                else if (e.Location.X > Width - Native.BORDER_WIDTH && e.Location.Y < Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.TopRight;
                    Cursor = Cursors.SizeNESW;
                }
                else if (e.Location.X < Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Left;
                    Cursor = Cursors.SizeWE;
                }
                else if (e.Location.X > Width - Native.BORDER_WIDTH && e.Location.Y > Height - Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.BottomRight;
                    Cursor = Cursors.SizeNWSE;
                }
                else if (e.Location.X > Width - Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Right;
                    Cursor = Cursors.SizeWE;
                }
                else if (e.Location.Y > Height - Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Bottom;
                    Cursor = Cursors.SizeNS;
                }
                else if (e.Location.Y < Native.BORDER_WIDTH && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Top;
                    Cursor = Cursors.SizeNS;
                }
              
                else
                {
                    _resizeDir = ResizeDirection.None;

                    //Only reset the cursor when needed, this prevents it from flickering when a child control changes the cursor to its own needs
                    if (_resizeCursors.Contains(Cursor))
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            //Console.WriteLine(e.Location);
            UpdateButtonState(e);
        }

        private void UpdateButtonState(MouseEventArgs e, bool isup = false)
        {
            var oldState = _currentState;
            bool showMin = MinimizeBox && ControlBox;
            bool showMax = MaximizeBox && ControlBox;

            if (e.Button == MouseButtons.Left && !isup)
            {
                if (showMin && !showMax && _maximizeButton.Contains(e.Location))
                    _currentState = ButtonState.MinDown;
                else if (showMin && showMax && _minimizeButton.Contains(e.Location))
                    _currentState = ButtonState.MinDown;
                else if (showMax && _maximizeButton.Contains(e.Location))
                    _currentState = ButtonState.MaxDown;
                else if (ControlBox && _closeButton.Contains(e.Location))
                    _currentState = ButtonState.XDown;
                else
                    _currentState = ButtonState.None;
            }
            else
            {
                if (showMin && !showMax && _maximizeButton.Contains(e.Location))
                {
                    _currentState = ButtonState.MinOver;
                    if (oldState == ButtonState.MinDown && isup)
                        WindowState = FormWindowState.Minimized;
                }
                else if (showMin && showMax && _minimizeButton.Contains(e.Location))
                {
                    _currentState = ButtonState.MinOver;
                    if (oldState == ButtonState.MinDown && isup)
                        WindowState = FormWindowState.Minimized;
                }
                else if (MaximizeBox && ControlBox && _maximizeButton.Contains(e.Location))
                {
                    _currentState = ButtonState.MaxOver;
                    if (oldState == ButtonState.MaxDown && isup)
                        MaximizeWindow(!_maximized);
                }
                else if (ControlBox && _closeButton.Contains(e.Location))
                {
                    _currentState = ButtonState.XOver;
                    if (oldState == ButtonState.XDown && isup)
                        Close();
                }
                else
                    _currentState = ButtonState.None;
            }

            if (oldState != _currentState)
                Invalidate();
        }


        private void MaximizeWindow(bool maximize)
        {
            if (!MaximizeBox || !ControlBox) return;

            _maximized = maximize;

            if (maximize)
            {
                var monitorHandle = Native.MonitorFromWindow(Handle, Native.MONITOR_DEFAULTTONEAREST);
                var monitorInfo = new Native.MONITORINFOEX();
                Native.GetMonitorInfo(new HandleRef(null, monitorHandle), monitorInfo);
                _previousSize = Size;
                _previousLocation = Location;
                Size = new Size(monitorInfo.rcWork.Width, monitorInfo.rcWork.Height);
                Location = new Point(monitorInfo.rcWork.Left, monitorInfo.rcWork.Top);
            }
            else
            {
                Size = _previousSize;
                Location = _previousLocation;
            }
        }


        private void ResizeForm(ResizeDirection direction)
        {
            if (DesignMode) return;
            var dir = -1;
            switch (direction)
            {
                case ResizeDirection.BottomLeft:
                    dir = Native.HTBOTTOMLEFT;
                    break;
                case ResizeDirection.Left:
                    dir = Native.HTLEFT;
                    break;
                case ResizeDirection.Right:
                    dir = Native.HTRIGHT;
                    break;
                case ResizeDirection.BottomRight:
                    dir = Native.HTBOTTOMRIGHT;
                    break;
                case ResizeDirection.Bottom:
                    dir = Native.HTBOTTOM;
                    break;
                case ResizeDirection.Top:
                    dir = Native.HTTOP;
                    break;
                case ResizeDirection.TopLeft:
                    dir = Native.HTTOPLEFT;
                    break;
                case ResizeDirection.TopRight:
                    dir = Native.HTTOPRIGHT;
                    break;
            }

            Native.ReleaseCapture();
            if (dir != -1)
            {
                Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, dir, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(BackColor);

            g.FillRectangle(new SolidBrush(ColorsTable.Black0), _statusBarBounds);


            int offset = 0;
            if (ShowIcon && Icon != null)
            {
                offset += 15;
                g.DrawImage(Icon.ToBitmap(), 4, 4, 18, 18);
            }
            
            Rectangle rect = new Rectangle(offset + 14, Native.STATUS_BAR_HEIGHT / 2 - (int)_yTitle / 2, Width, Native.ACTION_BAR_HEIGHT);
            g.DrawString(Text, Font, Brushes.White, rect, new StringFormat() { Alignment = StringAlignment.Near });


            using (var borderPen = new Pen(ColorsTable.Black0, 1))
            {
                g.DrawLine(borderPen, new Point(0, Native.STATUS_BAR_HEIGHT), new Point(0, Height - 2));
                g.DrawLine(borderPen, new Point(Width - 1, Native.STATUS_BAR_HEIGHT), new Point(Width - 1, Height - 2));
                g.DrawLine(borderPen, new Point(0, Height - 1), new Point(Width - 1, Height - 1));
            }

            bool showMin = MinimizeBox && ControlBox;
            bool showMax = MaximizeBox && ControlBox;

            g.FillRectangle(new SolidBrush(ColorsTable.Black0), _minimizeButton.X, 0, _statusBarBounds.Width - _minimizeButton.X, _statusBarBounds.Height);

            if (_currentState == ButtonState.MinOver && showMin)
                g.FillRectangle(new SolidBrush(ColorsTable.Black1), showMax ? _minimizeButton : _maximizeButton);

            if (_currentState == ButtonState.MinDown && showMin)
                g.FillRectangle(new SolidBrush(ColorsTable.Black2), showMax ? _minimizeButton : _maximizeButton);

            if (_currentState == ButtonState.MaxOver && showMax)
                g.FillRectangle(new SolidBrush(ColorsTable.Black1), _maximizeButton);

            if (_currentState == ButtonState.MaxDown && showMax)
                g.FillRectangle(new SolidBrush(ColorsTable.Black2), _maximizeButton);

            if (_currentState == ButtonState.XOver && ControlBox)
                g.FillRectangle(new SolidBrush(ColorsTable.Black1), _closeButton);

            if (_currentState == ButtonState.XDown && ControlBox)
                g.FillRectangle(new SolidBrush(ColorsTable.Black2), _closeButton);



            using (Pen pen = new Pen(Brushes.White, 2))
            {
                if (showMin)
                {
                    int x = showMax ? _minimizeButton.X : _maximizeButton.X;
                    int y = showMax ? _minimizeButton.Y : _maximizeButton.Y;

                    g.DrawLine(
                        pen,
                        x + (int)(_minimizeButton.Width * 0.33),
                        y + (int)(_minimizeButton.Height * 0.66),
                        x + (int)(_minimizeButton.Width * 0.66),
                        y + (int)(_minimizeButton.Height * 0.66)
                   );
                }

                if (showMax)
                {
                    g.DrawRectangle(
                        pen,
                        _maximizeButton.X + (int)(_maximizeButton.Width * 0.33),
                        _maximizeButton.Y + (int)(_maximizeButton.Height * 0.36),
                        (int)(_maximizeButton.Width * 0.39),
                        (int)(_maximizeButton.Height * 0.31)
                   );
                }

                if (ControlBox)
                {
                    g.DrawLine(
                        pen,
                        _closeButton.X + (int)(_closeButton.Width * 0.33),
                        _closeButton.Y + (int)(_closeButton.Height * 0.33),
                        _closeButton.X + (int)(_closeButton.Width * 0.66),
                        _closeButton.Y + (int)(_closeButton.Height * 0.66)
                   );

                    g.DrawLine(
                        pen,
                        _closeButton.X + (int)(_closeButton.Width * 0.66),
                        _closeButton.Y + (int)(_closeButton.Height * 0.33),
                        _closeButton.X + (int)(_closeButton.Width * 0.33),
                        _closeButton.Y + (int)(_closeButton.Height * 0.66));
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == Native.WM_LBUTTONDBLCLK)
            {
                MaximizeWindow(!_maximized);
            }
            else if (m.Msg == Native.WM_MOUSEMOVE && _maximized &&
                (_statusBarBounds.Contains(PointToClient(Cursor.Position)) /*|| _actionBarBounds.Contains(PointToClient(Cursor.Position))*/) &&
                !(_minimizeButton.Contains(PointToClient(Cursor.Position)) || _maximizeButton.Contains(PointToClient(Cursor.Position)) || _closeButton.Contains(PointToClient(Cursor.Position))))
            {
                if (_headerMouseDown)
                {
                    _maximized = false;
                    _headerMouseDown = false;

                    var mousePoint = PointToClient(Cursor.Position);
                    if (mousePoint.X < Width / 2)
                        Location = mousePoint.X < _previousSize.Width / 2 ?
                            new Point(Cursor.Position.X - mousePoint.X, Cursor.Position.Y - mousePoint.Y) :
                            new Point(Cursor.Position.X - _previousSize.Width / 2, Cursor.Position.Y - mousePoint.Y);
                    else
                        Location = Width - mousePoint.X < _previousSize.Width / 2 ?
                            new Point(Cursor.Position.X - _previousSize.Width + Width - mousePoint.X, Cursor.Position.Y - mousePoint.Y) :
                            new Point(Cursor.Position.X - _previousSize.Width / 2, Cursor.Position.Y - mousePoint.Y);

                    Size = _previousSize;
                    Native.ReleaseCapture();
                    Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, Native.HT_CAPTION, 0);
                }
            }
            else if (m.Msg == Native.WM_LBUTTONDOWN &&
                (_statusBarBounds.Contains(PointToClient(Cursor.Position)) /*|| _actionBarBounds.Contains(PointToClient(Cursor.Position))*/) &&
                !(_minimizeButton.Contains(PointToClient(Cursor.Position)) || _maximizeButton.Contains(PointToClient(Cursor.Position)) || _closeButton.Contains(PointToClient(Cursor.Position))))
            {
                if (!_maximized)
                {
                    Native.ReleaseCapture();
                    Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, Native.HT_CAPTION, 0);
                }
                else
                {
                    _headerMouseDown = true;
                }
            }
            else if (m.Msg == Native.WM_RBUTTONDOWN)
            {
                Point cursorPos = PointToClient(Cursor.Position);

                if (_statusBarBounds.Contains(cursorPos) && !_minimizeButton.Contains(cursorPos) &&
                    !_maximizeButton.Contains(cursorPos) && !_closeButton.Contains(cursorPos))
                {
                    // Show default system menu when right clicking titlebar
                    var id = Native.TrackPopupMenuEx(Native.GetSystemMenu(Handle, false), Native.TPM_LEFTALIGN | Native.TPM_RETURNCMD, Cursor.Position.X, Cursor.Position.Y, Handle, IntPtr.Zero);

                    // Pass the command as a WM_SYSCOMMAND message
                    if (!MaximizeBox && id == 61488)
                        return;
                    Native.SendMessage(Handle, Native.WM_SYSCOMMAND, id, 0);
                }
            }
            else if (m.Msg == Native.WM_NCLBUTTONDOWN)
            {
                // This re-enables resizing by letting the application know when the
                // user is trying to resize a side. This is disabled by default when using WS_SYSMENU.
                if (!Sizable) return;

                byte bFlag = 0;

                // Get which side to resize from
                if (Native._resizingLocationsToCmd.ContainsKey((int)m.WParam))
                    bFlag = (byte)Native._resizingLocationsToCmd[(int)m.WParam];

                if (bFlag != 0)
                    Native.SendMessage(Handle, Native.WM_SYSCOMMAND, 0xF000 | bFlag, (int)m.LParam);
            }
            else if (m.Msg == Native.WM_LBUTTONUP)
            {
                _headerMouseDown = false;
            }
        }

        public class MouseMessageFilter : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public static event MouseEventHandler MouseMove;

            public bool PreFilterMessage(ref Message m)
            {

                if (m.Msg == WM_MOUSEMOVE)
                {
                    if (MouseMove != null)
                    {
                        int x = Control.MousePosition.X, y = Control.MousePosition.Y;
                        MouseMove(null, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
                    }
                }
                return false;
            }
        }
    }
}
