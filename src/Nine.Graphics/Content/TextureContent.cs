namespace Nine.Graphics.Content
{
    using System;
    using System.Diagnostics;

    public class TextureContent
    {
        public readonly int Width;
        public readonly int Height;

        public readonly byte[] Pixels;

        public readonly int Left;
        public readonly int Right;
        public readonly int Top;
        public readonly int Bottom;

        public readonly bool IsTransparent;

        public TextureContent(
            int width, int height, byte[] pixels, bool isTransparent = true)
            : this(width, height, pixels, 0, width, 0, height, isTransparent)
        { }

        public TextureContent(
            int width, int height, byte[] pixels, int left, int right, int top, int bottom, bool isTransparent = true)
        {
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));

            Debug.Assert(width > 0);
            Debug.Assert(height > 0);

            this.Pixels = pixels;

            this.Width = width;
            this.Height = height;

            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;

            this.IsTransparent = isTransparent;
        }

        public override string ToString()
        {
            return $"{ Width }x{ Height }, { Pixels?.Length / 1000 }k";
        }
    }
}
