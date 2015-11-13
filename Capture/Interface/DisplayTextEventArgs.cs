using System;

namespace Direct3DHookLib.Interface
{
    [Serializable]
    public class DisplayTextEventArgs : MarshalByRefObject
    {
        public DisplayTextEventArgs(string text, TimeSpan duration)
        {
            Text = text;
            Duration = duration;
        }

        public string Text { get; set; }
        public TimeSpan Duration { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Text);
        }
    }
}