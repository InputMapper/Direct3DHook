using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Rectangle = System.Drawing.Rectangle;

namespace Capture.Hook.DX11
{
    public class DXImage : Component
    {
        private DeviceContext _deviceContext;
        private bool _initialised;
        private Texture2D _tex;
        private ShaderResourceView _texSRV;

        public DXImage(Device device, DeviceContext deviceContext) : base("DXImage")
        {
            Device = device;
            _deviceContext = deviceContext;
            _tex = null;
            _texSRV = null;
            Width = 0;
            Height = 0;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Device Device { get; private set; }

        public bool Initialise(Bitmap bitmap)
        {
            RemoveAndDispose(ref _tex);
            RemoveAndDispose(ref _texSRV);

            //Debug.Assert(bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmData;

            Width = bitmap.Width;
            Height = bitmap.Height;

            bmData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                var texDesc = new Texture2DDescription();
                texDesc.Width = Width;
                texDesc.Height = Height;
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
                data.RowPitch = bmData.Stride; // _texWidth * 4;
                data.SlicePitch = 0;

                _tex = ToDispose(new Texture2D(Device, texDesc, new[] {data}));
                if (_tex == null)
                    return false;

                var srvDesc = new ShaderResourceViewDescription();
                srvDesc.Format = Format.B8G8R8A8_UNorm;
                srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                srvDesc.Texture2D.MipLevels = 1;
                srvDesc.Texture2D.MostDetailedMip = 0;

                _texSRV = ToDispose(new ShaderResourceView(Device, _tex, srvDesc));
                if (_texSRV == null)
                    return false;
            }
            finally
            {
                bitmap.UnlockBits(bmData);
            }

            _initialised = true;

            return true;
        }

        public ShaderResourceView GetSRV()
        {
            Debug.Assert(_initialised);
            return _texSRV;
        }
    }
}