namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Framework.Runtime;
    using Nine.Graphics.Content;
    using Nine.Injection;
    using Xunit;

    public class FontRendererTest : GraphicsTest
    {
        [Theory]
        [MemberData(nameof(Containers))]
        public async Task build_default_ascii_table(Lazy<IContainer> container)
        {
            var textureCount = 0;
            var fontLoader = container.Value.Get<IFontLoader>();

            GlyphLoadResult lastGlyph = new GlyphLoadResult();

            var font = await fontLoader.LoadFont();
            for (var c = '\0'; c <= '\u00FF'; c++)
            {
                var glyph = font.LoadGlyph(c);
                if (glyph.Texture != null)
                {
                    lastGlyph = glyph;
                }
            }

            SaveFrame(
                ExpandMonoTexture(lastGlyph.Texture),
                $"{ OutputPath }/{ nameof(FontRendererTest) }/{ nameof(build_default_ascii_table) }-{ textureCount++ }.png");
        }

        //[Theory]
        //[MemberData(nameof(Containers))]
        public async Task build_full_unicode_table(Lazy<IContainer> container)
        {
            var textureCount = 0;
            var fontLoader = container.Value.Get<IFontLoader>();

            GlyphLoadResult lastGlyph = new GlyphLoadResult();

            var font = await fontLoader.LoadFont("simhei");
            for (var c = char.MinValue; c < char.MaxValue; c++)
            {
                var glyph = font.LoadGlyph(c);
                if (glyph.CreatesNewTexture && lastGlyph.Texture != null)
                {
                    SaveFrame(
                        ExpandMonoTexture(lastGlyph.Texture),
                        $"{ OutputPath }/{ nameof(FontRendererTest) }/{ nameof(build_full_unicode_table) }-{ textureCount++ }.png");
                }

                if (glyph.Texture != null)
                {
                    lastGlyph = glyph;
                }
            }

            if (lastGlyph.Texture != null)
            {
                SaveFrame(
                    ExpandMonoTexture(lastGlyph.Texture),
                    $"{ OutputPath }/{ nameof(FontRendererTest) }/{ nameof(build_full_unicode_table) }-{ textureCount++ }.png");
            }
        }

        private TextureContent ExpandMonoTexture(TextureContent texture)
        {
            var pixels = new byte[texture.Width * texture.Height * 4];
            for (var i = 0; i < texture.Pixels.Length; i++)
            {
                pixels[i * 4 + 0] = pixels[i * 4 + 1] = pixels[i * 4 + 2] = texture.Pixels[i];
                pixels[i * 4 + 3] = 255;
            }
            return new TextureContent(texture.Width, texture.Height, pixels, texture.IsTransparent);
        }
    }
}
