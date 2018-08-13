using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using EnhancedMap.GUI.Animations;

namespace EnhancedMap.Core.MapObjects
{
    public class SignalObject : RenderObject
    {
        private const int RATE = 25;
        private static readonly Pen _pen = new Pen(Color.Cyan);
        private AnimationManager _animationManager;

        public SignalObject(int x, int y) : base("signal")
        {
            _animationManager = new AnimationManager(false) {Increment = 0.05, AnimationType = AnimationType.EaseOut};
            UpdatePosition(x, y);
            /*_animationManager.OnAnimationProgress += (sender) =>
            {

            };

            _animationManager.OnAnimationFinished += (sender) =>
            {

            };*/

            if (Global.SettingsCollection["alertsounds"].ToBool())
                SoundsManager.Play(SOUNDS_TYPE.SIGNAL);

            LifeTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(2));
        }

        public bool IsAnimating => _animationManager != null && _animationManager.IsAnimating();

        // need to refresh
        public override bool Render(Graphics g, int x, int y, int canvasW, int canvasH)
        {
            if (IsDisposing)
                return false;

            if (IsEndOfLife)
            {
                Dispose();
                _animationManager = null;
                return false;
            }

            if (IsAnimating)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int rX = RelativePosition.X;
                int rY = RelativePosition.Y;
                (rX, rY) = Geometry.RotatePoint(rX, rY, Global.Zoom, 1, Global.Angle);

                rX += x;
                rY += y;

                for (int i = 0; i < _animationManager.GetAnimationCount(); i++)
                {
                    double animationValue = _animationManager.GetProgress(i);
                    Point animationSource = _animationManager.GetSource(i);
                    int size = (int) (animationValue * 2 * RATE);

                    g.DrawEllipse(_pen, new RectangleF(rX + animationSource.X - size / 2, rY + animationSource.Y - size / 2, size, size));
                }

                g.SmoothingMode = SmoothingMode.None;
            }
            else
                _animationManager.StartNewAnimation(AnimationDirection.In, new Point(0, 0));

            return true;
        }
    }
}