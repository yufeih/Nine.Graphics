namespace Nine.Graphics.Content
{
    using Microsoft.Framework.Runtime;
    using SharpFont;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using Library = SharpFont.Library;

    using Texture = System.String;//TODO:

    public sealed class FontLoader : IDisposable
    {
        private readonly IContentProvider contentProvider;
        private readonly NuGetDependencyResolver dependencyResolver;
        private readonly Lazy<Library> freetype;
        private readonly string defaultFont;
        private readonly int textureSize;
        private readonly Fixed26Dot6 baseFontSize;

        private readonly RectanglePacker packer;
        private readonly Dictionary<int, Texture> charactorMap = new Dictionary<int, Texture>();

        public bool UseSystemFonts { get; set; } = true;

        public FontLoader(
            NuGetDependencyResolver nuget, IContentProvider contentProvider = null,
            string defaultFont = null, int baseFontSize = 72, int textureSize = 512)
        {
            if (nuget == null) throw new ArgumentNullException(nameof(nuget));
            if (baseFontSize <= 1 || baseFontSize > textureSize) throw new ArgumentOutOfRangeException(nameof(baseFontSize));
            if (textureSize <= 1) throw new ArgumentOutOfRangeException(nameof(textureSize));

            this.dependencyResolver = nuget;
            this.defaultFont = defaultFont ?? "Consola";
            this.contentProvider = contentProvider;
            this.baseFontSize = baseFontSize;
            this.textureSize = textureSize;
            this.packer = new RectanglePacker(textureSize, textureSize);
            this.freetype = new Lazy<Library>(CreateFreeType);
        }

        public void LoadGlyph(string text, FontId font, Texture[] result, int startIndex)
        {
            for (var i = 0; i < text.Length; i++)
            {
                result[startIndex + i] = LoadGlyph(text[i], font);
            }
        }

        public void LoadGlyph(StringBuilder text, FontId font, Texture[] result, int startIndex)
        {
            for (var i = 0; i < text.Length; i++)
            {
                result[startIndex + i] = LoadGlyph(text[i], font);
            }
        }

        private Texture LoadGlyph(char charactor, FontId font)
        {
            Texture slice;

            var hash = (font.Id << 16) | charactor;
            if (!charactorMap.TryGetValue(hash, out slice))
            {
                charactorMap[hash] = slice = CreateGlyph(charactor, font);
            }

            return slice;
        }

        private Texture CreateGlyph(char charactor, FontId font)
        {
            var face = CreateFontFace(font);
            var glyph = face.GetCharIndex(charactor);
            if (glyph == 0)
            {
                return null;
            }

            face.SetCharSize(baseFontSize, baseFontSize, 72, 72);
            face.LoadGlyph(glyph, LoadFlags.Default, LoadTarget.Normal);

            Point point;
            var metrics = face.Glyph.Metrics;
            if (packer.TryPack(metrics.Width.ToInt32(), metrics.Height.ToInt32(), 1, out point))
            {
                var pixels = new byte[textureSize * textureSize];
                FillGlyph(face, pixels, textureSize, 0, 0);
            }

            // return new Texture(0, textureSize, textureSize, true);
            return null;
        }

        private unsafe void FillGlyph(Face face, byte[] pixels, int width, int startX, int startY)
        {
            // Use default for 8-bit anti-aliased pixmap ??
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

        private Texture PlatformCreateTexture(int width, int height, byte[] bitmap)
        {
            return null;
        }

        private Face CreateFontFace(string font)
        {
            font = string.IsNullOrEmpty(font) ? defaultFont : font;

            if (UseSystemFonts)
            {
                var fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    font + ".ttf");

                return freetype.Value.NewFace(fontPath, 0);
            }

            return null;
        }

        private Library CreateFreeType()
        {
            var sharpFontDependencies = dependencyResolver.Dependencies.FirstOrDefault(d => d.Resolved && d.Identity.Name == "SharpFont.Dependencies");
            if (sharpFontDependencies == null) throw new InvalidOperationException("Cannot load SharpFont.Dependencies");

            var freetypePath = Path.Combine(sharpFontDependencies.Path, "bin/msvc9");
            var arch = IntPtr.Size == 8 ? "x64" : "x86";

            Interop.LoadLibrary(Path.Combine(freetypePath, arch, "freetype6.dll"));

            return new Library();
        }

        public void Dispose()
        {
            if (freetype.IsValueCreated)
            {
                freetype.Value.Dispose();
            }
        }
    }
}
