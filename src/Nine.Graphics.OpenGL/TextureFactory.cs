namespace Nine.Graphics.OpenGL
{
    using System;
    using OpenTK.Graphics.OpenGL4;

    public class TextureFactory
    {
        private readonly ITextureLoader loader;

        public TextureFactory(ITextureLoader loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            this.loader = loader;
        }

        public int GetTexture(TextureId textureId)
        {
            return 0;
        }
    }
}
