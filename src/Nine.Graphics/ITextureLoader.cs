namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Nine.Imaging;

    public class TextureData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Bytes;

        public TextureData(int width, int height, byte[] bytes)
        {
            if (width <= 0) throw new ArgumentException(nameof(width));
            if (height <= 0) throw new ArgumentException(nameof(height));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            this.Width = width;
            this.Height = height;
            this.Bytes = bytes;
        }
    }

    public interface ITextureLoader
    {
        Task<TextureData> Load(string name);
    }

    public class TextureLoader : ITextureLoader
    {
        private readonly IContentLoader contentLoader;

        public TextureLoader(IContentLoader contentLoader)
        {
            if (contentLoader == null) throw new ArgumentNullException(nameof(contentLoader));

            this.contentLoader = contentLoader;
        }

        public async Task<TextureData> Load(string name)
        {
            var stream = await contentLoader.Load(name).ConfigureAwait(false);
            if (stream == null) return null;

            var image = new Image(stream);
            return new TextureData(image.PixelWidth, image.PixelHeight, image.Pixels);
        }
    }
}
