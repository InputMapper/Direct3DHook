using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Capture.Hook.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;

namespace Capture.Hook.DX9
{
    internal class DXOverlayEngine : Component
    {
        private readonly Dictionary<string, Font> _fontCache = new Dictionary<string, Font>();
        private readonly Dictionary<Element, Texture> _imageCache = new Dictionary<Element, Texture>();

        private bool _initialised;
        private bool _initialising;
        private Sprite _sprite;

        public DXOverlayEngine()
        {
            Overlays = new List<IOverlay>();
        }

        public List<IOverlay> Overlays { get; set; }

        public Device Device { get; private set; }

        private void EnsureInitiliased()
        {
            Debug.Assert(_initialised);
        }

        public bool Initialise(Device device)
        {
            Debug.Assert(!_initialised);
            if (_initialising)
                return false;

            _initialising = true;

            try
            {
                Device = device;

                _sprite = new Sprite(Device);

                // Initialise any resources required for overlay elements
                IntialiseElementResources();

                _initialised = true;
                return true;
            }
            finally
            {
                _initialising = false;
            }
        }

        private void IntialiseElementResources()
        {
            foreach (var overlay in Overlays)
            {
                foreach (var element in overlay.Elements)
                {
                    var textElement = element as TextElement;
                    var imageElement = element as ImageElement;

                    if (textElement != null)
                    {
                        GetFontForTextElement(textElement);
                    }
                    else if (imageElement != null)
                    {
                        GetImageForImageElement(imageElement);
                    }
                }
            }
        }

        private void Begin()
        {
            _sprite.Begin(SpriteFlags.AlphaBlend);
        }

        /// <summary>
        ///     Draw the overlay(s)
        /// </summary>
        public void Draw()
        {
            EnsureInitiliased();

            Begin();

            foreach (var overlay in Overlays)
            {
                foreach (var element in overlay.Elements)
                {
                    if (element.Hidden)
                        continue;

                    var textElement = element as TextElement;
                    var imageElement = element as ImageElement;

                    if (textElement != null)
                    {
                        var font = GetFontForTextElement(textElement);
                        if (font != null && !string.IsNullOrEmpty(textElement.Text))
                            font.DrawText(_sprite, textElement.Text, textElement.Location.X, textElement.Location.Y,
                                new ColorBGRA(textElement.Color.R, textElement.Color.G, textElement.Color.B,
                                    textElement.Color.A));
                    }
                    else if (imageElement != null)
                    {
                        var image = GetImageForImageElement(imageElement);
                        if (image != null)
                            _sprite.Draw(image,
                                new ColorBGRA(imageElement.Tint.R, imageElement.Tint.G, imageElement.Tint.B,
                                    imageElement.Tint.A), null, null,
                                new Vector3(imageElement.Location.X, imageElement.Location.Y, 0));
                    }
                }
            }

            End();
        }

        private void End()
        {
            _sprite.End();
        }

        private Font GetFontForTextElement(TextElement element)
        {
            Font result = null;

            var fontKey = string.Format("{0}{1}{2}", element.Font.Name, element.Font.Size, element.Font.Style,
                element.AntiAliased);

            if (!_fontCache.TryGetValue(fontKey, out result))
            {
                result = ToDispose(new Font(Device, new FontDescription
                {
                    FaceName = element.Font.Name,
                    Italic = (element.Font.Style & FontStyle.Italic) == FontStyle.Italic,
                    Quality = (element.AntiAliased ? FontQuality.Antialiased : FontQuality.Default),
                    Weight =
                        ((element.Font.Style & FontStyle.Bold) == FontStyle.Bold) ? FontWeight.Bold : FontWeight.Normal,
                    Height = (int) element.Font.SizeInPoints
                }));
                _fontCache[fontKey] = result;
            }
            return result;
        }

        private Texture GetImageForImageElement(ImageElement element)
        {
            Texture result = null;

            if (!string.IsNullOrEmpty(element.Filename))
            {
                if (!_imageCache.TryGetValue(element, out result))
                {
                    result = ToDispose(Texture.FromFile(Device, element.Filename));

                    _imageCache[element] = result;
                }
            }
            return result;
        }

        /// <summary>
        ///     Releases unmanaged and optionally managed resources
        /// </summary>
        /// <param name="disposing">true if disposing both unmanaged and managed</param>
        protected override void Dispose(bool disposing)
        {
            if (true)
            {
                Device = null;
            }
        }

        private void SafeDispose(DisposeBase disposableObj)
        {
            if (disposableObj != null)
                disposableObj.Dispose();
        }
    }
}