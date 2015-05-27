namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Nine.Imaging;
    
    public class TextureLoader : ITextureLoader
    {
        private readonly IContentLoader contentLoader;

        public TextureLoader(IContentLoader contentLoader)
        {
            if (contentLoader == null) throw new ArgumentNullException(nameof(contentLoader));

            this.contentLoader = contentLoader;
        }

        public async Task<TextureContent> Load(string name)
        {
            var stream = await contentLoader.Load(name).ConfigureAwait(false);
            if (stream == null) return null;

            var image = new Image(stream);
            return new TextureContent(image.PixelWidth, image.PixelHeight, image.Pixels);
        }
    }
}
