namespace Nine.Graphics
{
    using System.Text;
    using System.Numerics;

    public struct TextSprite
    {
        public readonly string Text;
        public readonly StringBuilder TextBuilder;
        public readonly FontId Font;
        public readonly float FontSize;
        public readonly float Border;
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Color BorderColor;

        public static readonly TextSprite Default = new TextSprite(default(TextureId));
        
        public TextSprite(
            string text,
            FontId font = default(FontId),
            float fontSize = 12,
            Vector2 position = default(Vector2),
            Color? color = null,
            float border = 0,
            Color? borderColor = null,
            StringBuilder textBuilder = null)
        {
            this.Text = text;
            this.Font = font;
            this.FontSize = fontSize;
            this.Position = position;
            this.Color = color ?? Color.White;
            this.Border = border;
            this.BorderColor = borderColor ?? Color.White;
            this.TextBuilder = textBuilder;
        }
    }
}
