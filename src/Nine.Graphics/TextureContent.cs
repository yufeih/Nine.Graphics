namespace Nine.Graphics
{
    using System.Diagnostics;

    public class TextureContent
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Pixels;

        public TextureContent(int width, int height, byte[] pixels)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            Debug.Assert(pixels != null);

            this.Width = width;
            this.Height = height;
            this.Pixels = pixels;
        }

        public override string ToString()
        {
            return $"{ Width }x{ Height }, { Pixels?.Length / 1000 }k";
        }
    }
}
