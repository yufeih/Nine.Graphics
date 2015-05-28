namespace Nine.Graphics.OpenGL
{
    using System.Diagnostics;

    public class TextureSlice
    {
        public readonly int Texture;
        public readonly int Width;
        public readonly int Height;

        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public TextureSlice(int texture, int width, int height, int left, int right, int top, int bottom)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);

            Debug.Assert(left >= 0 && left <= width);
            Debug.Assert(right >= 0 && right <= width);
            Debug.Assert(right >= left);

            Debug.Assert(top >= 0 && top <= height);
            Debug.Assert(bottom >= 0 && bottom <= height);
            Debug.Assert(bottom >= top);

            this.Texture = texture;
            this.Width = width;
            this.Height = height;
            this.Left = (float)left / width;
            this.Right = (float)right / width;
            this.Top = (float)top / height;
            this.Bottom = (float)bottom / height;
        }
    }
}
