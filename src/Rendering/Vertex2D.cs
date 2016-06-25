namespace Nine.Graphics.Rendering
{
    using System.Numerics;

    public struct Vertex2D
    {
        public Vector2 Position;
        public int Color;
        public Vector2 TextureCoordinate;

        public const int SizeInBytes = 8 + 4 + 8;
    }
}
