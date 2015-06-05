namespace Nine.Graphics
{
    using System;

    public interface IGraphicsHost : IDisposable
    {
        int Width { get; }
        int Height { get; }

        bool BeginFrame();
        void EndFrame();

        TextureContent GetTexture();
    }
}
