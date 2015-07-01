namespace Nine.Graphics
{
    using System.Numerics;

    public struct TextSprite
    {
        public readonly string Text;
        public readonly FontId Font;
        public readonly float FontSize;
        public readonly Vector2 Position;
        public readonly Color Color;

        public static readonly TextSprite Default = new TextSprite(default(TextureId));
        
        public TextSprite(
            string text,
            FontId font = default(FontId),
            float fontSize = 12,
            Vector2 position = default(Vector2),
            Color? color = null)
        {
            this.Text = text;
            this.Font = font;
            this.FontSize = fontSize;
            this.Position = position;
            this.Color = color ?? Color.White;
        }
    }
}
