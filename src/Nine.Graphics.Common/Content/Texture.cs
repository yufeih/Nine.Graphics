#if DX
namespace Nine.Graphics.Content.DirectX
{
    using PlatformTexture = SharpDX.Direct3D12.Resource;
#else
namespace Nine.Graphics.Content.OpenGL
{
    using PlatformTexture = System.Int32;
#endif
    using System.Diagnostics;

    public partial class Texture
    {
        public readonly PlatformTexture PlatformTexture;

        public readonly int Width;
        public readonly int Height;

        public readonly int SourceWidth;
        public readonly int SourceHeight;

        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public readonly bool IsTransparent;

        public Texture(PlatformTexture texture, int width, int height, bool isTransparent)
            : this(texture, width, height, 0, width, 0, height, isTransparent)
        { }

        public Texture(PlatformTexture texture, int sourceWidth, int sourceHeight, int left, int right, int top, int bottom, bool isTransparent)
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

            this.Left = (float)left / sourceWidth;
            this.Right = (float)right / sourceWidth;
            this.Top = (float)top / sourceHeight;
            this.Bottom = (float)bottom / sourceHeight;

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
