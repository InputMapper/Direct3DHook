using System;

namespace Direct3DHookLib.Hook
{
    public class TextDisplay
    {
        private readonly long _startTickCount;

        public TextDisplay()
        {
            _startTickCount = DateTime.Now.Ticks;
            Display = true;
        }

        public bool Display { get; set; }
        public string Text { get; set; }
        public TimeSpan Duration { get; set; }

        public float Remaining
        {
            get
            {
                if (Display)
                {
                    return Math.Abs(DateTime.Now.Ticks - _startTickCount)/(float) Duration.Ticks;
                }
                return 0;
            }
        }

        /// <summary>
        ///     Must be called each frame
        /// </summary>
        public void Frame()
        {
            if (Display && Math.Abs(DateTime.Now.Ticks - _startTickCount) > Duration.Ticks)
            {
                Display = false;
            }
        }
    }
}