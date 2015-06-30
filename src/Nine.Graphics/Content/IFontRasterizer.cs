namespace Nine.Graphics.Content
{
    using System.Text;

    public interface IFontRasterizer
    {
        void LoadGlyph(string text, TextureContent[] glyphs, int startIndex);
        void LoadGlyph(StringBuilder text, TextureContent[] glyphs, int startIndex);
    }
}
