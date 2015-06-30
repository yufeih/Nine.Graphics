namespace Nine.Graphics
{
    using Nine.Imaging;
    using System.Numerics;

    public struct Sprite
    {
        public readonly Vector2 Position;
        public readonly Vector2 Scale;
        public readonly Vector2 Size;
        public readonly float Rotation;
        public readonly Vector2 Origin;
        public readonly TextureId Texture;
        public readonly Color Color;

        public static readonly Sprite Default = new Sprite(default(TextureId));
        
        public Sprite(
            TextureId texture,
            Vector2 position = default(Vector2),
            Vector2? scale = null,
            Vector2? size = null,
            float rotation = 0,
            Vector2 origin = default(Vector2),
            Color? color = null)
        {
            this.Texture = texture;
            this.Position = position;
            this.Scale = scale ?? Vector2.One;
            this.Rotation = rotation;
            this.Size = size ?? Vector2.Zero;
            this.Origin = origin;
            this.Color = color ?? Color.White;
        }
    }
}
