namespace Nine.Graphics.DirectX
{
    using System;
    using System.Numerics;

    partial class DynamicPrimitiveRenderer
    {
        public DynamicPrimitiveRenderer(TextureFactory textureFactory, int initialBufferCapacity = 32)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.PlatformCreateBuffers(initialBufferCapacity);
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers(int initialBufferCapacity)
        {
            throw new NotImplementedException();
        }

        private void PlatformCreateShaders()
        {
            throw new NotImplementedException();
        }

        private void PlatformUpdateBuffers()
        {
            throw new NotImplementedException();
        }

        private void PlatformBeginDraw(ref Matrix4x4 wvp)
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawBatch(PrimitiveGroupEntry entry)
        {
            throw new NotImplementedException();
        }

        private void PlatformEndDraw()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose()
        {
            throw new NotImplementedException();
        }
    }
}
