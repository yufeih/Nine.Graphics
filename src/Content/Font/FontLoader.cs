namespace Nine.Graphics.Content
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;
    using SharpFont;

    public sealed class FontLoader : IFontLoader, IDisposable
    {
        private readonly IContentProvider _contentProvider;
        private readonly Lazy<Library> _freetype;
        private readonly string _defaultFont;
        private readonly int _textureSize;
        private readonly Fixed26Dot6 _baseFontSize;

        private RectanglePacker _packer;
        private byte[] _pixels;

        public bool UseSystemFonts { get; set; } = true;

        public FontLoader(
            IContentProvider contentProvider = null, string defaultFont = null, int baseFontSize = 32, int textureSize = 512)
        {
            if (baseFontSize <= 1 || baseFontSize > textureSize) throw new ArgumentOutOfRangeException(nameof(baseFontSize));
            if (textureSize <= 1) throw new ArgumentOutOfRangeException(nameof(textureSize));
            
            _defaultFont = defaultFont ?? "Consola";
            _contentProvider = contentProvider;
            _baseFontSize = baseFontSize;
            _textureSize = textureSize;
            _freetype = new Lazy<Library>(() => new Library());
        }

        public async Task<IFontFace> LoadFont(string font)
        {
            font = string.IsNullOrEmpty(font) ? _defaultFont : font;

            if (UseSystemFonts)
            {
                var fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    font + ".ttf");

                if (File.Exists(fontPath))
                {
                    return new FontFace(this, _freetype.Value.NewFace(fontPath, 0));
                }
            }

            if (_contentProvider != null)
            {
                using (var stream = await _contentProvider.Open(font))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    return new FontFace(this, _freetype.Value.NewMemoryFace(buffer, 0));
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (_freetype.IsValueCreated)
            {
                _freetype.Value.Dispose();
            }
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

                face.SetCharSize(parent._baseFontSize, parent._baseFontSize, 72, 72);
                face.LoadGlyph(glyph, LoadFlags.Default, LoadTarget.Normal);

                var point = default(Point);
                var createsNewTexture = false;
                var metrics = face.Glyph.Metrics;
                var textureSize = parent._textureSize;

                if (parent._packer == null)
                {
                    createsNewTexture = true;
                    parent._packer = new RectanglePacker(textureSize, textureSize);
                    parent._pixels = new byte[textureSize * textureSize];
                }

                if (parent._packer.TryPack(metrics.Width.ToInt32(), metrics.Height.ToInt32(), 1, out point))
                {
                    FillGlyph(face, parent._pixels, textureSize, point.X, point.Y);

                    return new GlyphLoadResult(new TextureContent(textureSize, textureSize, parent._pixels), createsNewTexture);
                }
                else
                {
                    parent._packer = new RectanglePacker(textureSize, textureSize);

                    if (parent._packer.TryPack(metrics.Width.ToInt32(), metrics.Height.ToInt32(), 1, out point))
                    {
                        parent._pixels = new byte[textureSize * textureSize];

                        FillGlyph(face, parent._pixels, textureSize, point.X, point.Y);

                        return new GlyphLoadResult(new TextureContent(textureSize, textureSize, parent._pixels), true);
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
