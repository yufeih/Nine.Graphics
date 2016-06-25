namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;
    using Nine.Imaging;
    using Nine.Injection;
    using Xunit;

    class FontTest : DrawingTest<FontTest>, IDrawingTest
    {
        public IEnumerable<Drawing> GetDrawings()
        {
            yield break;
        }

        [Fact]
        public async Task build_default_ascii_table()
        {
            var textureCount = 0;
            var fontLoader = OpenGlContainer.Get<IFontLoader>();

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
                $"{ nameof(build_default_ascii_table) }-{ textureCount++ }.png");
        }

        [Fact]
        public async Task build_full_unicode_table(Lazy<IContainer> container)
        {
            var textureCount = 0;
            var fontLoader = OpenGlContainer.Get<IFontLoader>();

            GlyphLoadResult lastGlyph = new GlyphLoadResult();

            var font = await fontLoader.LoadFont("simhei");
            for (var c = char.MinValue; c < char.MaxValue; c++)
            {
                var glyph = font.LoadGlyph(c);
                if (glyph.CreatesNewTexture && lastGlyph.Texture != null)
                {
                    SaveFrame(
                        ExpandMonoTexture(lastGlyph.Texture),
                        $"{ nameof(build_full_unicode_table) }-{ textureCount++ }.png");
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
                    $"{ nameof(build_full_unicode_table) }-{ textureCount++ }.png");
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

        private void SaveFrame(TextureContent textureContent, string path)
        {
            path = $"{OutputPath}/{nameof(FontTest)}/{path}";

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (var stream = File.OpenWrite(path))
            {
                var img = new Image(textureContent.Width, textureContent.Height, textureContent.Pixels);
                img.SaveAsPng(stream);
            }
        }
    }
}
