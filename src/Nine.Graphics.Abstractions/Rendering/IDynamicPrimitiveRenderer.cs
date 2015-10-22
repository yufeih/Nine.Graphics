namespace Nine.Graphics.Rendering
{
    using System.Numerics;

    public interface IDynamicPrimitiveRenderer
    {
        void Draw(Matrix4x4 projection, Matrix4x4 view);
    }
}
