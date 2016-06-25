namespace Nine.Graphics.Rendering
{
    using System;
    using SharpDX.Direct3D12;

    public class DXTestGraphicsHost : TestGraphicsHost
    {
        public Device Device { get; }

        public DXTestGraphicsHost(
            int width, int height,
            int testDuration = 1000, float epsilon = 0.001f, string outputPath = null)
            : base("dx", width, height, testDuration, epsilon, outputPath)
        { }

        protected override void BeginFrame()
        {
            throw new NotImplementedException();
        }

        protected override void EndFrame(byte[] pixels)
        {
            throw new NotImplementedException();
        }
    }
}
