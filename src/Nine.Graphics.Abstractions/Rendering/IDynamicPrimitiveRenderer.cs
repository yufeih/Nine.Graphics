namespace Nine.Graphics.Rendering
{
    using System.Numerics;
    
    public enum PrimitiveType
    {
        Points          = 0,
        Lines           = 1,
        LineLoop        = 2,
        LineStrip       = 3,
        Triangles       = 4,
        TriangleStrip   = 5,
        TriangleFan     = 6,
        Quads           = 7,
        QuadStrip       = 8,
        Polygon         = 9,
    }

    public interface IDynamicPrimitiveRenderer
    {
        void BeginPrimitive(PrimitiveType primitiveType, TextureId? texture, Matrix4x4? world = null, float lineWidth = 1);
        void EndPrimitive();

        void AddVertex(Vector3 position, Color color);
        void AddVertex(Vector3 position, Color color, Vector2 uv);
        void AddVertex(Vector3 position, Vector3 color);
        void AddVertex(Vector3 position, Vector3 color, Vector2 uv);
        void AddVertex(Vector3 position, Vector4 color, Vector2 uv);

        void AddIndex(int index);

        void Clear();

        void Draw(Matrix4x4 projection, Matrix4x4 view);
    }
}
