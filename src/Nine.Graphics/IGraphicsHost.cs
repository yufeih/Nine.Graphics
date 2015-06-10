namespace Nine.Graphics
{
    using System;

    public interface IGraphicsHost : IDisposable
    {
        int Width { get; }
        int Height { get; }

        IntPtr WindowHandle { get; }

        bool BeginFrame();
        void EndFrame();

        TextureContent GetTexture();
    }
}
