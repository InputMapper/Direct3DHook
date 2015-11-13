using System.Drawing;

namespace Capture.Hook.Common
{
    public class TextElement : Element
    {
        public TextElement(Font font)
        {
            Font = font;
        }

        public virtual string Text { get; set; }
        public virtual Font Font { get; set; }
        public virtual Color Color { get; set; }
        public virtual Point Location { get; set; }
        public virtual bool AntiAliased { get; set; }
    }
}