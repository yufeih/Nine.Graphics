namespace Nine.Graphics
{
    using Nine.Imaging;
    using System.Numerics;

    public struct TextSprite
    {
        public readonly string Text;
        public readonly FontId Font;
        public readonly float FontSize;
        public readonly float Opacity;
        public readonly Vector2 Position;
        public readonly float Depth;
        public readonly Color Color;
        public readonly Matrix3x2 Transform;
        public readonly bool HasTransform;
        public readonly bool IsVisible;

        public static readonly Sprite Default = new Sprite(default(TextureId));
        
        public TextSprite(
            string text,
            FontId font = default(FontId),
            float fontSize = 12,
            Vector2 position = default(Vector2),
            float opacity = 1.0f,
            bool isVisible = true,
            float depth = 0,
            Color? color = null,
            Matrix3x2? transform = null)
        {
            this.Text = text;
            this.Font = font;
            this.FontSize = fontSize;
            this.IsVisible = isVisible;
            this.Opacity = opacity;
            this.Position = position;
            this.Depth = depth;
            this.Color = color ?? Color.White;
            this.Transform = transform ?? Matrix3x2.Identity;
            this.HasTransform = transform != null;
        }
    }
}
