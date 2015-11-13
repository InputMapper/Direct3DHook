// Adapted from Frank Luna's "Sprites and Text" example here: http://www.d3dcoder.net/resources.htm 
// checkout his books here: http://www.d3dcoder.net/default.htm

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = System.Drawing.Color;
using Device = SharpDX.Direct3D11.Device;
using Rectangle = SharpDX.Rectangle;

namespace Capture.Hook.DX11
{
    public class DXFont : IDisposable
    {
        private const char StartChar = (char) 33;
        private const char EndChar = (char) 127;
        private const uint NumChars = EndChar - StartChar;
        private readonly Rectangle[] _charRects = new Rectangle[NumChars];
        private Device _device;
        private DeviceContext _deviceContext;
        private ShaderResourceView _fontSheetSRV;
        private Texture2D _fontSheetTex;

        private bool _initialized;
        private int _spaceWidth, _charHeight;
        private readonly int _texWidth;
        private int _texHeight;

        public DXFont(Device device, DeviceContext deviceContext)
        {
            _device = device;
            _deviceContext = deviceContext;
            _initialized = false;
            _fontSheetTex = null;
            _fontSheetSRV = null;
            _texWidth = 1024;
            _texHeight = 0;
            _spaceWidth = 0;
            _charHeight = 0;
        }

        public void Dispose()
        {
            if (_fontSheetTex != null)
                _fontSheetTex.Dispose();
            if (_fontSheetSRV != null)
                _fontSheetSRV.Dispose();

            _fontSheetTex = null;
            _fontSheetSRV = null;
            _device = null;
            _deviceContext = null;
        }

        public bool Initialize(string FontName, float FontSize, FontStyle FontStyle, bool AntiAliased)
        {
            Debug.Assert(!_initialized);
            var font = new Font(FontName, FontSize, FontStyle, GraphicsUnit.Pixel);

            var hint = AntiAliased ? TextRenderingHint.AntiAlias : TextRenderingHint.SystemDefault;

            var tempSize = (int) (FontSize*2);
            using (var charBitmap = new Bitmap(tempSize, tempSize, PixelFormat.Format32bppArgb))
            {
                using (var charGraphics = Graphics.FromImage(charBitmap))
                {
                    charGraphics.PageUnit = GraphicsUnit.Pixel;
                    charGraphics.TextRenderingHint = hint;

                    MeasureChars(font, charGraphics);

                    using (var fontSheetBitmap = new Bitmap(_texWidth, _texHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var fontSheetGraphics = Graphics.FromImage(fontSheetBitmap))
                        {
                            fontSheetGraphics.CompositingMode = CompositingMode.SourceCopy;
                            fontSheetGraphics.Clear(Color.FromArgb(0, Color.Black));

                            BuildFontSheetBitmap(font, charGraphics, charBitmap, fontSheetGraphics);

                            if (!BuildFontSheetTexture(fontSheetBitmap))
                            {
                                return false;
                            }
                        }
                        //System.Drawing.Bitmap bm = new System.Drawing.Bitmap(fontSheetBitmap);
                        //bm.Save(@"C:\temp\test.png");
                    }
                }
            }

            _initialized = true;

            return true;
        }

        private bool BuildFontSheetTexture(Bitmap fontSheetBitmap)
        {
            BitmapData bmData;

            bmData = fontSheetBitmap.LockBits(new System.Drawing.Rectangle(0, 0, _texWidth, _texHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var texDesc = new Texture2DDescription();
            texDesc.Width = _texWidth;
            texDesc.Height = _texHeight;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = Format.B8G8R8A8_UNorm;
            texDesc.SampleDescription.Count = 1;
            texDesc.SampleDescription.Quality = 0;
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;


            DataBox data;
            data.DataPointer = bmData.Scan0;
            data.RowPitch = _texWidth*4;
            data.SlicePitch = 0;

            _fontSheetTex = new Texture2D(_device, texDesc, new[] {data});
            if (_fontSheetTex == null)
                return false;

            var srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = Format.B8G8R8A8_UNorm;
            srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            srvDesc.Texture2D.MipLevels = 1;
            srvDesc.Texture2D.MostDetailedMip = 0;

            _fontSheetSRV = new ShaderResourceView(_device, _fontSheetTex, srvDesc);
            if (_fontSheetSRV == null)
                return false;

            fontSheetBitmap.UnlockBits(bmData);

            return true;
        }

        private void MeasureChars(Font font, Graphics charGraphics)
        {
            var allChars = new char[NumChars];

            for (var i = (char) 0; i < NumChars; ++i)
                allChars[i] = (char) (StartChar + i);

            SizeF size;
            size = charGraphics.MeasureString(new string(allChars), font, new PointF(0, 0), StringFormat.GenericDefault);

            _charHeight = (int) (size.Height + 0.5f);

            var numRows = (int) (size.Width/_texWidth) + 1;
            _texHeight = (numRows*_charHeight) + 1;

            var sf = StringFormat.GenericDefault;
            sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            size = charGraphics.MeasureString(" ", font, 0, sf);
            _spaceWidth = (int) (size.Width + 0.5f);
        }

        private void BuildFontSheetBitmap(Font font, Graphics charGraphics, Bitmap charBitmap,
            Graphics fontSheetGraphics)
        {
            var whiteBrush = Brushes.White;
            var fontSheetX = 0;
            var fontSheetY = 0;


            for (var i = 0; i < NumChars; ++i)
            {
                charGraphics.Clear(Color.FromArgb(0, Color.Black));
                charGraphics.DrawString(((char) (StartChar + i)).ToString(), font, whiteBrush, new PointF(0.0f, 0.0f));

                var minX = GetCharMinX(charBitmap);
                var maxX = GetCharMaxX(charBitmap);
                var charWidth = maxX - minX + 1;

                if (fontSheetX + charWidth >= _texWidth)
                {
                    fontSheetX = 0;
                    fontSheetY += _charHeight + 1;
                }

                _charRects[i] = new Rectangle(fontSheetX, fontSheetY, charWidth, _charHeight);

                fontSheetGraphics.DrawImage(charBitmap, fontSheetX, fontSheetY,
                    new System.Drawing.Rectangle(minX, 0, charWidth, _charHeight), GraphicsUnit.Pixel);

                fontSheetX += charWidth + 1;
            }
        }

        private int GetCharMaxX(Bitmap charBitmap)
        {
            var width = charBitmap.Width;
            var height = charBitmap.Height;

            for (var x = width - 1; x >= 0; --x)
            {
                for (var y = 0; y < height; ++y)
                {
                    Color color;

                    color = charBitmap.GetPixel(x, y);
                    if (color.A > 0)
                        return x;
                }
            }

            return width - 1;
        }

        private int GetCharMinX(Bitmap charBitmap)
        {
            var width = charBitmap.Width;
            var height = charBitmap.Height;

            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    Color color;

                    color = charBitmap.GetPixel(x, y);
                    if (color.A > 0)
                        return x;
                }
            }

            return 0;
        }

        public ShaderResourceView GetFontSheetSRV()
        {
            Debug.Assert(_initialized);

            return _fontSheetSRV;
        }

        public Rectangle GetCharRect(char c)
        {
            Debug.Assert(_initialized);

            return _charRects[c - StartChar];
        }

        public int GetSpaceWidth()
        {
            Debug.Assert(_initialized);

            return _spaceWidth;
        }

        public int GetCharHeight()
        {
            Debug.Assert(_initialized);

            return _charHeight;
        }

        private enum STYLE
        {
            STYLE_NORMAL = 0,
            STYLE_BOLD = 1,
            STYLE_ITALIC = 2,
            STYLE_BOLD_ITALIC = 3,
            STYLE_UNDERLINE = 4,
            STYLE_STRIKEOUT = 8
        };
    }
}