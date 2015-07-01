namespace Nine.Graphics.Rendering
{
    using System.Numerics;

    public interface ISpriteRenderer
    {
        void Draw(Matrix4x4 projection, Slice<Sprite> sprites, Slice<Matrix3x2>? transforms = null, Slice<int>? indices = null);
    }
}
