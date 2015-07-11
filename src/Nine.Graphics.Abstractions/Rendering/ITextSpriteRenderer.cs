namespace Nine.Graphics.Rendering
{
    using System;
    using System.Numerics;

    public interface ITextSpriteRenderer
    {
        void Draw(Matrix4x4 projection, Slice<TextSprite> textSprites, Slice<Matrix3x2>? transforms = null);
    }
}
