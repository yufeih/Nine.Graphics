namespace Nine.Graphics.DirectX
{
    using System;
    using SharpDX.Direct3D12;

    partial class FontTextureFactory
    {
        private Texture PlatformCreate8BppTexture(int width, int height, byte[] pixels)
        {
            throw new NotImplementedException();
        }

        private void PlatformUpdate8bppTexture(Resource texture, int width, int height, byte[] pixels)
        {
            throw new NotImplementedException();
        }
    }
}
