namespace Nine.Graphics
{
    using System;
    using System.Numerics;
    using Xunit;
    using Nine.Imaging;
    using System.Threading.Tasks;
    using Nine.Graphics.Rendering;
    using Nine.Graphics.Content;
    using Nine.Injection;
    using Microsoft.Framework.Runtime;
    using System.Collections.Generic;

    public class FontRendererTest : GraphicsTest
    {
        [Fact]
        public async Task build_default_ascii_table()
        {
            var fontLoader = Container.Get<IFontLoader>();
            var font = await fontLoader.LoadFont();
            var textureCount = 0;

            GlyphLoadResult lastGlyph = new GlyphLoadResult();

            for (var c = 0; c < 128; c++)
            {
                var glyph = font.LoadGlyph((char)c);
                if (glyph.Texture != null)
                {
                    lastGlyph = glyph;
                }
            }
            var output = $"{ OutputPath }/{ nameof(FontRendererTest) }/{ nameof(build_default_ascii_table) }-{ textureCount++ }.png";
            SaveFrame(ExpandMonoTexture(lastGlyph.Texture), output);
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
