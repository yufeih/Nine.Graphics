namespace Nine.Graphics
{
    using System.Numerics;

    public class Sprite
    {
        public bool IsVisible { get; set; }

        public float Opacity { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Scale { get; set; }

        public float Rotation { get; set; }

        public TextureId Texture { get; set; }

        public Sprite() { }
        public Sprite(TextureId texture) { this.Texture = texture; }
    }
}
