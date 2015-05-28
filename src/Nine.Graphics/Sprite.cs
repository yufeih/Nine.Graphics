namespace Nine.Graphics
{
    using System.Numerics;

    public class Sprite
    {
        public bool IsVisible;

        public float Opacity = 1.0f;

        public Vector2 Position;

        public Vector2 Scale;

        public float Rotation;

        public Vector2 Origin;

        public TextureId Texture;

        public float Depth;

        public int Color; // TODO: Turn into Color

        public Sprite() { }
        public Sprite(TextureId texture) { this.Texture = texture; }
    }
}
