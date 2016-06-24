namespace Nine.Graphics.DirectX
{
    using SharpDX.Direct3D12;
    using System;

    partial class TestGraphicsHost
    {
        public Device Device { get; }

        private void PlatformBeginFrame()
        {
            throw new NotImplementedException();

        }

        private void PlatformEndFrame(byte[] pixels)
        {
            throw new NotImplementedException();
        }
    }
}
