namespace Nine.Graphics
{
    using System.Numerics;
    
    public struct Sprite
    {
        public readonly bool IsVisible;
        public readonly float Opacity;
        public readonly Vector2 Position;
        public readonly Vector2 Scale;
        public readonly float Rotation;
        public readonly Vector2 Origin;
        public readonly TextureId Texture;
        public readonly float Depth;
        public readonly int Color; // TODO: Turn into Color

        public static readonly Sprite Default = new Sprite(default(TextureId));
        
        public Sprite(
            TextureId texture,
            Vector2 position = default(Vector2),
            float opacity = 1.0f,
            Vector2? scale = null,
            float rotation = 0,
            Vector2 origin = default(Vector2),
            bool isVisible = true,
            float depth = 0,
            int color = -1)
        {
            this.Texture = texture;
            this.IsVisible = isVisible;
            this.Opacity = opacity;
            this.Position = position;
            this.Scale = scale ?? Vector2.One;
            this.Rotation = rotation;
            this.Origin = origin;
            this.Depth = depth;
            this.Color = color;
        }
    }
}
