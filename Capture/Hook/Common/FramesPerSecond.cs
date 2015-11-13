using System;
using System.Drawing;

namespace Direct3DHookLib.Hook.Common
{
    public class FramesPerSecond : TextElement
    {
        private string _fpsFormat = "{0:N0} fps";

        private int _frames;
        private float _lastFrameRate;
        private int _lastTickCount;

        public FramesPerSecond(Font font)
            : base(font)
        {
        }

        public override string Text
        {
            get { return string.Format(_fpsFormat, GetFPS()); }
            set { _fpsFormat = value; }
        }

        /// <summary>
        ///     Must be called each frame
        /// </summary>
        public override void Frame()
        {
            _frames++;
            if (Math.Abs(Environment.TickCount - _lastTickCount) > 1000)
            {
                _lastFrameRate = (float) _frames*1000/Math.Abs(Environment.TickCount - _lastTickCount);
                _lastTickCount = Environment.TickCount;
                _frames = 0;
            }
        }

        /// <summary>
        ///     Return the current frames per second
        /// </summary>
        /// <returns></returns>
        public float GetFPS()
        {
            return _lastFrameRate;
        }
    }
}