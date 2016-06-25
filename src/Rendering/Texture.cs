namespace Nine.Graphics.Rendering
{
    using System.Diagnostics;

    public class Texture<T>
    {
        public readonly T PlatformTexture;

        public readonly int Width;
        public readonly int Height;

        public readonly int SourceWidth;
        public readonly int SourceHeight;

        public readonly float UvLeft;
        public readonly float UvRight;
        public readonly float UvTop;
        public readonly float UvBottom;

        public readonly bool IsTransparent;

        public Texture(T texture, int width, int height, bool isTransparent)
            : this(texture, width, height, 0, width, 0, height, isTransparent)
        { }

        public Texture(T texture, int sourceWidth, int sourceHeight, int left, int right, int top, int bottom, bool isTransparent)
        {
            Debug.Assert(sourceWidth > 0);
            Debug.Assert(sourceHeight > 0);

            Debug.Assert(left >= 0 && left <= sourceWidth);
            Debug.Assert(right >= 0 && right <= sourceWidth);
            Debug.Assert(right >= left);

            Debug.Assert(top >= 0 && top <= sourceHeight);
            Debug.Assert(bottom >= 0 && bottom <= sourceHeight);
            Debug.Assert(bottom >= top);

            this.PlatformTexture = texture;
            this.SourceWidth = sourceWidth;
            this.SourceHeight = sourceHeight;

            this.Width = right - left;
            this.Height = bottom - top;

            this.UvLeft = (float)left / sourceWidth;
            this.UvRight = (float)right / sourceWidth;
            this.UvTop = (float)top / sourceHeight;
            this.UvBottom = (float)bottom / sourceHeight;

            this.IsTransparent = isTransparent;
        }

        public override string ToString()
        {
            if (Width == SourceWidth && Height == SourceHeight)
            {
                return $"{ Width }x{ Height }, { PlatformTexture } ";
            }
            return $"{ Width }x{ Height }, source:{ SourceWidth }x{ SourceHeight }, { PlatformTexture }";
        }
    }
}
