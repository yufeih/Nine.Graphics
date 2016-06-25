namespace Nine.Graphics.Rendering
{
    using System;
    using System.Numerics;

    public abstract class TextSpriteRenderer<T> : ITextSpriteRenderer, IDisposable
    {
        private readonly FontTextureFactory<T> _textureFactory;
        
        public TextSpriteRenderer(FontTextureFactory<T> textureFactory)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            _textureFactory = textureFactory;
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
