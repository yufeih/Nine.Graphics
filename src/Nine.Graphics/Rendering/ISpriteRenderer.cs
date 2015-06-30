namespace Nine.Graphics.Rendering
{
    using System.Numerics;

    public interface ISpriteRenderer
    {
        void Draw(Slice<Sprite> sprites, Matrix3x2? camera = null, Slice<Matrix3x2>? transforms = null, Slice<int>? indices = null);
    }
}
