namespace Nine.Graphics.Rendering
{
    using System;
    using Nine.Graphics.Content;
    using OpenTK.Graphics.OpenGL;

    public class GLFontTextureFactory : FontTextureFactory<int>
    {
        public GLFontTextureFactory(IFontLoader loader) : base(loader) { }

        protected override Texture<int> Create8BppTexture(int width, int height, byte[] pixels)
        {
            GLDebug.CheckAccess();

            var texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);

            return new Texture<int>(texture, width, height, true);
        }

        protected override void Update8bppTexture(int texture, int width, int height, byte[] pixels)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);
        }

        protected override Texture<int> Create8BppTexture(int width, int height, byte[] pixels)
        {
            throw new NotImplementedException();
        }
    }
}
