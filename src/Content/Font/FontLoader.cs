namespace Nine.Graphics.Content
{
    using SharpFont;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Library = SharpFont.Library;

    public sealed class FontLoader : IFontLoader, IDisposable
    {
        private readonly IContentProvider contentProvider;
        private readonly Library freetype;
        private readonly string defaultFont;
        private readonly int textureSize;
        private readonly Fixed26Dot6 baseFontSize;

        private RectanglePacker packer;
        private byte[] pixels;

        public bool UseSystemFonts { get; set; } = true;

        public FontLoader(
            IContentProvider contentProvider = null, string defaultFont = null, int baseFontSize = 32, int textureSize = 512)
        {
            if (baseFontSize <= 1 || baseFontSize > textureSize) throw new ArgumentOutOfRangeException(nameof(baseFontSize));
            if (textureSize <= 1) throw new ArgumentOutOfRangeException(nameof(textureSize));
            
            this.defaultFont = defaultFont ?? "Consola";
            this.contentProvider = contentProvider;
            this.baseFontSize = baseFontSize;
            this.textureSize = textureSize;
            this.freetype = new Library();
        }

        public async Task<IFontFace> LoadFont(string font)
        {
            font = string.IsNullOrEmpty(font) ? defaultFont : font;

            if (UseSystemFonts)
            {
                var fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    font + ".ttf");

                if (File.Exists(fontPath))
                {
                    return new FontFace(this, freetype.NewFace(fontPath, 0));
                }
            }

            if (contentProvider != null)
            {
                using (var stream = await contentProvider.Open(font))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    return new FontFace(this, freetype.NewMemoryFace(buffer, 0));
                }
            }

            return null;
        }

        public void Dispose()
        {
            freetype.Dispose();
        }

        class FontFace : IFontFace
        {
            private readonly FontLoader parent;
            private readonly Face face;

            public FontFace(FontLoader parent, Face face)
            {
                this.parent = parent;
                this.face = face;
            }

            public GlyphLoadResult LoadGlyph(char charactor)
            {
                var glyph = face.GetCharIndex(charactor);
                if (glyph == 0)
                {
                    return default(GlyphLoadResult);
                }

                face.SetCharSize(parent.baseFontSize, parent.baseFontSize, 72, 72);
                face.LoadGlyph(glyph, LoadFlags.Default, LoadTarget.Normal);

                var point = default(Point);
                var createsNewTexture = false;
                var metrics = face.Glyph.Metrics;
                var textureSize = parent.textureSize;

                if (parent.packer == null)
                {
                    createsNewTexture = true;
                    parent.packer = new RectanglePacker(textureSize, textureSize);
                    parent.pixels = new byte[textureSize * textureSize];
                }

                if (parent.packer.TryPack(metrics.Width.ToInt32(), metrics.Height.ToInt32(), 1, out point))
                {
                    FillGlyph(face, parent.pixels, textureSize, point.X, point.Y);

                    return new GlyphLoadResult(new TextureContent(textureSize, textureSize, parent.pixels), createsNewTexture);
                }
                else
                {
                    parent.packer = new RectanglePacker(textureSize, textureSize);

                    if (parent.packer.TryPack(metrics.Width.ToInt32(), metrics.Height.ToInt32(), 1, out point))
                    {
                        parent.pixels = new byte[textureSize * textureSize];

                        FillGlyph(face, parent.pixels, textureSize, point.X, point.Y);

                        return new GlyphLoadResult(new TextureContent(textureSize, textureSize, parent.pixels), true);
                    }
                }

                return default(GlyphLoadResult);
            }

            private unsafe void FillGlyph(Face face, byte[] pixels, int width, int startX, int startY)
            {
                face.Glyph.RenderGlyph(RenderMode.Mono);

                using (var bitmap = face.Glyph.Bitmap)
                {
                    if (bitmap.Width <= 0 || bitmap.Rows <= 0)
                    {
                        return;
                    }

                    var pSrc = (byte*)bitmap.Buffer;

                    for (var y = 0; y < bitmap.Rows; y++)
                    {
                        var destY = (startY + y) * width;

                        for (var x = 0; x < bitmap.Pitch; x++)
                        {
                            var src = *pSrc;
                            var destX = startX + x * 8;

                            for (var bit = 0; bit < 8; bit++)
                            {
                                var dest = destY + destX + bit;
                                var color = ((src >> (7 - bit)) & 1) == 0
                                    ? byte.MinValue : byte.MaxValue;

                                pixels[dest] = color;
                            }

                            pSrc++;
                        }
                    }
                }
            }
        }
    }
}
