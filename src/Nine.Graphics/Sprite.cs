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

        public TextureId Texture;

        public Sprite() { }
        public Sprite(TextureId texture) { this.Texture = texture; }
    }
}
