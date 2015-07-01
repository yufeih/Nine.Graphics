namespace Nine.Graphics.Rendering
{
    using System;
    using Nine.Graphics.Content;

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
