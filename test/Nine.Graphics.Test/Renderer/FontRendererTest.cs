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

    public class FontRendererTest : GraphicsTest
    {
        [Fact]
        public async Task build_font_face()
        {
            var fontLoader = Container.Get<IFontLoader>();

            var glyphs = new TextureContent[100];
            var font = await fontLoader.LoadFont();
            font.LoadGlyph("gAbstractg", glyphs, 0);
        }
    }
}
