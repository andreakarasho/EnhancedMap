using EnhancedMap.Core;
using System;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public enum ZoomType
    {
        Plus,
        Nothing,
        Minus
    }

    public partial class EnhancedCanvas : UserControl
    {
        private bool _scrollUpdate;

        public EnhancedCanvas()
        {
            InitializeComponent();

           /* hScrollBar.ValueChanged += OnScrollBarValueChangedX;
            vScrollBar.ValueChanged += OnScrollBarValueChangedY;
            hScrollBar.Scroll += ScrollBar_Scroll;
            vScrollBar.Scroll += ScrollBar_Scroll;
            */
            ECanvas.MouseEnter += ECanvasOnMouseEnter;
            ECanvas.MouseLeave += ECanvasOnMouseLeave;

           // EventSink.ScrollVisibleEvent += EventSink_ScrollVisibleEvent;
            //EventSink.MapChangedEvent += EventSink_MapChangedEvent;
           // Global.FacetChanged += (sender, e) =>
            //{
                /*Map = Global.Maps[e.Current];
                hScrollBar.Maximum = Map.Width + 9;
                vScrollBar.Maximum = Map.Height + 9;
                EventSink.InvokeLocationChanged(new OnLocationChangedEventArgs((int)Global.X, (int)Global.Y));*/
            //};

           // EventSink.LocationChangedEvent += EventSink_LocationChangedEvent;

            Zoom = new ZoomInfo();

            SetStyle( ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public ZoomInfo Zoom { get; private set; }
        public PictureBox ECanvas { get; private set; }

        public bool Scrolls
        {
            get { return hScrollBar.Visible || vScrollBar.Visible; }
            set { hScrollBar.Visible = vScrollBar.Visible = statusStrip.Visible = value; }
        }

       // public MapBase Map { get; set; }
        public bool MouseInCanvas { get; private set; }

       /* private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (_scrollUpdate || Map == null)
                return;

            if (!Global.FreeView)
                Global.FreeView = true;
        }*/

        private void ECanvasOnMouseLeave(object sender, EventArgs eventArgs)
        {
            MouseInCanvas = false;
        }

        private void ECanvasOnMouseEnter(object sender, EventArgs eventArgs)
        {
            MouseInCanvas = true;
        }

        protected override void OnResize(EventArgs e)
        {
            ECanvas.Width = ClientSize.Width - (vScrollBar.Visible ? vScrollBar.Width : 0);
            ECanvas.Height = ClientSize.Height - (hScrollBar.Visible ? hScrollBar.Height : 0);

            if (statusStrip.Visible)
            {
                statusStrip.Top = ClientSize.Height - statusStrip.Height;
                statusStrip.Left = ClientSize.Width - statusStrip.Width;
            }

            base.OnResize(e);
        }

      /*  private void OnScrollBarValueChangedX(object sender, EventArgs e)
        {
            if (_scrollUpdate || Map == null)
                return;

           Global.X = hScrollBar.Value;
            //Global.Y = vScrollBar.Value;
        }*/

       /* private void OnScrollBarValueChangedY(object sender, EventArgs e)
        {
            if (_scrollUpdate || Map == null)
                return;

            //Global.X = hScrollBar.Value;
           Global.Y = vScrollBar.Value;
        }*/

        /*private void EventSink_LocationChangedEvent(OnLocationChangedEventArgs e)
        {
            if (Map != null)
                UpdateScrollbars(e.X, e.Y);
        }*/

       /* private void EventSink_MapChangedEvent(OnMapChangedEventArgs e)
        {
            
        }*/

      /*  private void EventSink_ScrollVisibleEvent(OnScrollVisibleEventArgs e)
        {
            Scrolls = e.Visible;
            OnResize(null);
        }*/

        private void UpdateScrollbars(int x, int y)
        {
            _scrollUpdate = true;

            if (x < 0)
                x = 0;
            else if (x > hScrollBar.Maximum)
                x = hScrollBar.Maximum;

            if (y < 0)
                y = 0;
            else if (y > vScrollBar.Maximum)
                y = vScrollBar.Maximum;

            hScrollBar.Value = x;
            vScrollBar.Value = y;
            hScrollBar.Invalidate();
            vScrollBar.Invalidate();

            _scrollUpdate = false;
        }

        public class ZoomInfo
        {
            private int _index;
            private readonly float[] _zooms = new float[10] { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 4f, 6f, 8f };

            public float CurrentZoom
            {
                get { return SetZoom(ZoomType.Nothing); }
            }

            public float SetZoom(ZoomType type)
            {
                switch (type)
                {
                    case ZoomType.Minus:
                        {
                            _index--;
                            if (_index < 0)
                                _index = 0;
                            return Configuration.MySettings.ZoomValue = _zooms[_index];
                        }
                    case ZoomType.Plus:
                        {
                            _index++;
                            if (_index > _zooms.Length - 1)
                                _index = _zooms.Length - 1;
                            return Configuration.MySettings.ZoomValue = _zooms[_index];
                        }
                    default:
                        return _zooms[_index];
                }
            }

            public float SetZoom(float num)
            {
                for (_index = 0; _index < _zooms.Length; _index++)
                {
                    if (num == _zooms[_index])
                        return Configuration.MySettings.ZoomValue = num;
                }
                return CurrentZoom;
            }
        }
    }
}