namespace Nine.Graphics.OpenGL
{
    using OpenTK.Graphics.OpenGL;

    partial class FontTextureFactory
    {
        private Texture PlatformCreate8BppTexture(int width, int height, byte[] pixels)
        {
            GLDebug.CheckAccess();

            var texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);

            return new Texture(texture, width, height, true);
        }

        private void PlatformUpdate8bppTexture(int texture, int width, int height, byte[] pixels)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);
        }
    }
}
