using System;

namespace Capture.Hook
{
    /// <summary>
    ///     Used to determine the FPS
    /// </summary>
    public class FramesPerSecond
    {
        private int _frames;
        private float _lastFrameRate;
        private int _lastTickCount;

        /// <summary>
        ///     Must be called each frame
        /// </summary>
        public void Frame()
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