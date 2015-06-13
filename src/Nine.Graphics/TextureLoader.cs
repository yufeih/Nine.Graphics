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
            if (string.IsNullOrEmpty(name)) return null;

            if (name.StartsWith("n:"))
            {
                if (name == TextureId.Black.Name)
                    return new TextureContent(1, 1, new byte[] { 0, 0, 0, 255 });
                if (name == TextureId.White.Name)
                    return new TextureContent(1, 1, new byte[] { 255, 255, 255, 255 });
                if (name == TextureId.Transparent.Name)
                    return new TextureContent(1, 1, new byte[] { 0, 0, 0, 0 });
                if (name == TextureId.Missing.Name)
                    return new TextureContent(2, 2, new byte[] { 255, 0, 255, 255 });
                if (name == TextureId.Error.Name)
                    return new TextureContent(2, 2, new byte[] { 255, 0, 0, 255,  0, 255, 0, 255,  0, 0, 255, 255,  0, 0, 0, 255 });
            }

            using (var stream = await contentLoader.Load(name).ConfigureAwait(false))
            {
                if (stream == null) return null;

                var image = new Image(stream);
                return new TextureContent(image.PixelWidth, image.PixelHeight, image.Pixels);
            }
        }
    }
}
