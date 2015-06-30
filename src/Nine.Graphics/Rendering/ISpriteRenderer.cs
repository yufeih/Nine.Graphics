namespace Nine.Graphics.Rendering
{
    using System.Numerics;
    using Nine.Graphics.Primitives;

    public interface ISpriteRenderer
    {
        void Draw(
            Slice<Sprite> sprites,
            Slice<Matrix3x2>? transforms = null,
            Slice<int>? indices = null,
            TextureId texture = default(TextureId));
    }
}
