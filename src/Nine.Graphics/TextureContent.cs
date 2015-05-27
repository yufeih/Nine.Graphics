namespace Nine.Graphics
{
    using System;

    public class TextureContent
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Pixels;

        public TextureContent(int width, int height, byte[] pixels)
        {
            if (width <= 0) throw new ArgumentException(nameof(width));
            if (height <= 0) throw new ArgumentException(nameof(height));
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));

            this.Width = width;
            this.Height = height;
            this.Pixels = pixels;
        }
    }
}
