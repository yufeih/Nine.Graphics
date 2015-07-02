#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using Nine.Graphics.Rendering;

    public sealed partial class TextSpriteRenderer : ITextSpriteRenderer, IDisposable
    {
        private readonly FontTextureFactory textureFactory;
        
        public TextSpriteRenderer(FontTextureFactory textureFactory)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
        }

        public unsafe void Draw(Matrix4x4 projection, Slice<TextSprite> textSprites, Slice<Matrix3x2>? transforms = null)
        {
            if (textSprites.Length <= 0)
            {
                return;
            }
        }
        
        public void Dispose()
        {

        }
    }
}
