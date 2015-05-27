namespace Nine.Graphics
{
    using System;

    public class TextureContent
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Bytes;

        public TextureContent(int width, int height, byte[] bytes)
        {
            if (width <= 0) throw new ArgumentException(nameof(width));
            if (height <= 0) throw new ArgumentException(nameof(height));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            this.Width = width;
            this.Height = height;
            this.Bytes = bytes;
        }
    }
}
