namespace Nine.Graphics
{
    using System;

    public interface IGraphicsHost : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void BeginFrame();
        void EndFrame();

        TextureContent GetTexture();
    }
}
