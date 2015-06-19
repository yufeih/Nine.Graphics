namespace Nine.Graphics.Content
{
    using System.Diagnostics;

    public class TextureContent
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Pixels;
        public readonly bool IsTransparent;

        public TextureContent(int width, int height, byte[] pixels, bool isTransparent = true)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            Debug.Assert(pixels != null);

            this.Width = width;
            this.Height = height;
            this.Pixels = pixels;
            this.IsTransparent = isTransparent;
        }

        public override string ToString()
        {
            return $"{ Width }x{ Height }, { Pixels?.Length / 1000 }k";
        }
    }
}
