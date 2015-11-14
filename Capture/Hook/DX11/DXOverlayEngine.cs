using System.Collections.Generic;
using System.Diagnostics;
using Direct3DHookLib.Hook.Common;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Direct3DHookLib.Hook.DX11
{
    internal class DXOverlayEngine : Component
    {
        private readonly Dictionary<string, DXFont> _fontCache = new Dictionary<string, DXFont>();
        private readonly Dictionary<Element, DXImage> _imageCache = new Dictionary<Element, DXImage>();
        private Device _device;
        private DeviceContext _deviceContext;

        private bool _initialised;
        private bool _initialising;
        private Texture2D _renderTarget;
        private RenderTargetView _renderTargetView;
        private DXSprite _spriteEngine;

        public DXOverlayEngine()
        {
            Overlays = new List<IOverlay>();
        }

        public List<IOverlay> Overlays { get; set; }

        public bool DeferredContext
        {
            get { return _deviceContext.TypeInfo == DeviceContextType.Deferred; }
        }

        private void EnsureInitiliased()
        {
            Debug.Assert(_initialised);
        }

        public bool Initialise(SwapChain swapChain)
        {
            return Initialise(swapChain.GetDevice<Device>(), swapChain.GetBackBuffer<Texture2D>(0));
        }

        public bool Initialise(Device device, Texture2D renderTarget)
        {
            Debug.Assert(!_initialised);
            if (_initialising)
                return false;

            _initialising = true;

            try
            {
                _device = device;
                _renderTarget = renderTarget;
                try
                {
                    _deviceContext = ToDispose(new DeviceContext(_device));
                }
                catch (SharpDXException)
                {
                    _deviceContext = _device.ImmediateContext;
                }

                _renderTargetView = ToDispose(new RenderTargetView(_device, _renderTarget));

                //if (DeferredContext)
                //{
                //    ViewportF[] viewportf = { new ViewportF(0, 0, _renderTarget.Description.Width, _renderTarget.Description.Height, 0, 1) };
                //    _deviceContext.Rasterizer.SetViewports(viewportf);
                //    _deviceContext.OutputMerger.SetTargets(_renderTargetView);
                //}

                _spriteEngine = new DXSprite(_device, _deviceContext);
                if (!_spriteEngine.Initialize())
                    return false;

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
            //if (!DeferredContext)
            //{
            ViewportF[] viewportf =
            {
                new ViewportF(0, 0, _renderTarget.Description.Width, _renderTarget.Description.Height, 0, 1)
            };
            _deviceContext.Rasterizer.SetViewports(viewportf);
            _deviceContext.OutputMerger.SetTargets(_renderTargetView);
            //}
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
                            _spriteEngine.DrawString(textElement.Location.X, textElement.Location.Y, textElement.Text,
                                textElement.Color, font);
                    }
                    else if (imageElement != null)
                    {
                        var image = GetImageForImageElement(imageElement);
                        if (image != null)
                            _spriteEngine.DrawImage(imageElement.Location.X, imageElement.Location.Y, imageElement.Scale,
                                imageElement.Angle, imageElement.Tint, image);
                    }
                }
            }

            End();
        }

        private void End()
        {
            if (DeferredContext)
            {
                var commandList = _deviceContext.FinishCommandList(true);
                _device.ImmediateContext.ExecuteCommandList(commandList, true);
                commandList.Dispose();
            }
        }

        private DXFont GetFontForTextElement(TextElement element)
        {
            DXFont result = null;

            var fontKey = string.Format("{0}{1}{2}", element.Font.Name, element.Font.Size, element.Font.Style,
                element.AntiAliased);

            if (!_fontCache.TryGetValue(fontKey, out result))
            {
                result = ToDispose(new DXFont(_device, _deviceContext));
                result.Initialize(element.Font.Name, element.Font.Size, element.Font.Style, element.AntiAliased);
                _fontCache[fontKey] = result;
            }
            return result;
        }

        private DXImage GetImageForImageElement(ImageElement element)
        {
            DXImage result = null;

            if (!_imageCache.TryGetValue(element, out result))
            {
                result = ToDispose(new DXImage(_device, _deviceContext));
                result.Initialise(element.Bitmap);
                _imageCache[element] = result;
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
                _device = null;
            }
        }

        private void SafeDispose(DisposeBase disposableObj)
        {
            if (disposableObj != null)
                disposableObj.Dispose();
        }
    }
}