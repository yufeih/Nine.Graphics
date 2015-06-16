#if DX
namespace Nine.Graphics.DirectX
#else
namespace Nine.Graphics.OpenGL
#endif
{
    using System.Diagnostics;

    public class TextureSlice
    {
        public readonly int Texture;
        
        public readonly int Width;
        public readonly int Height;

        public readonly int SourceWidth;
        public readonly int SourceHeight;

        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public TextureSlice(int texture, int sourceWidth, int sourceHeight, int left, int right, int top, int bottom)
        {
            Debug.Assert(sourceWidth > 0);
            Debug.Assert(sourceHeight > 0);

            Debug.Assert(left >= 0 && left <= sourceWidth);
            Debug.Assert(right >= 0 && right <= sourceWidth);
            Debug.Assert(right >= left);

            Debug.Assert(top >= 0 && top <= sourceHeight);
            Debug.Assert(bottom >= 0 && bottom <= sourceHeight);
            Debug.Assert(bottom >= top);

            this.Texture = texture;
            this.SourceWidth = sourceWidth;
            this.SourceHeight = sourceHeight;

            this.Width = right - left;
            this.Height = bottom - top;

            this.Left = (float)left / sourceWidth;
            this.Right = (float)right / sourceWidth;
            this.Top = (float)top / sourceHeight;
            this.Bottom = (float)bottom / sourceHeight;
        }

        public override string ToString()
        {
            if (Width == SourceWidth && Height == SourceHeight)
            {
                return $"{ Width }x{ Height }, glTexture:{ Texture } ";
            }
            return $"{ Width }x{ Height }, source:{ SourceWidth }x{ SourceHeight }, glTexture:{ Texture }";
        }
    }
}
