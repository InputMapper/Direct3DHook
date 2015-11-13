using System.Drawing;
using Direct3DHookLib.Hook.Common;

namespace Direct3DHookLib.Hook.Common
{
    public class ImageElement : Element
    {
        private readonly bool _ownsBitmap;

        public ImageElement(string filename) :
            this(new Bitmap(filename), true)
        {
            Filename = filename;
        }

        public ImageElement(Bitmap bitmap, bool ownsImage = false)
        {
            Tint = Color.White;
            Bitmap = bitmap;
            _ownsBitmap = ownsImage;
            Scale = 1.0f;
        }

        public virtual Bitmap Bitmap { get; set; }

        /// <summary>
        ///     This value is multiplied with the source color (e.g. White will result in same color as source image)
        /// </summary>
        /// <remarks>
        ///     Defaults to <see cref="System.Drawing.Color.White" />.
        /// </remarks>
        public virtual Color Tint { get; set; }

        /// <summary>
        ///     The location of where to render this image element
        /// </summary>
        public virtual Point Location { get; set; }

        public float Angle { get; set; }

        public float Scale { get; set; }

        public string Filename { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_ownsBitmap)
                {
                    SafeDispose(Bitmap);
                    Bitmap = null;
                }
            }
        }
    }
}