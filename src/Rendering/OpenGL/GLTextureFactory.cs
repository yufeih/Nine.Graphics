namespace Nine.Graphics.Rendering
{
    using Nine.Graphics.Content;
    using OpenTK.Graphics.OpenGL;

    public class GLTextureFactory : TextureFactory<int>
    {
        public GLTextureFactory(IGraphicsHost graphicsHost, ITextureLoader loader, int capacity = 1024)
            : base(graphicsHost, loader, capacity)
        { }

        public override Texture<int> CreateTexture(TextureContent data)
        {
            GLDebug.CheckAccess();

            var texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Pixels);

            return new Texture<int>(texture, data.Width, data.Height, data.Left, data.Right, data.Top, data.Bottom, data.IsTransparent);
        }
    }
}
