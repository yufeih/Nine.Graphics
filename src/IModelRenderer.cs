namespace Nine.Graphics
{
    using System;
    using System.Numerics;

    public interface IModelRenderer
    {
        void Draw(Matrix4x4 view, Matrix4x4 projection, Slice<Model> models);
    }
}
