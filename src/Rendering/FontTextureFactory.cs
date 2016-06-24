namespace Nine.Graphics.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;

    public abstract class FontTextureFactory<T> : IFontPreloader
    {
        struct FontFace { public IFontFace Font; public bool IsLoaded; }

        private readonly IFontLoader loader;
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;
        private readonly Dictionary<int, FontFace> fontMap = new Dictionary<int, FontFace>();
        private readonly Dictionary<long, Texture<T>> charactorMap = new Dictionary<long, Texture<T>>();

        private Texture<T> textureBuilder;

        public FontTextureFactory(IFontLoader loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            this.loader = loader;
        }

        public Texture<T> GetGlyphTexture(FontId font, char charactor)
        {
            Texture<T> result = null;
            FontFace fontface = new FontFace();

            var key = (font.Id << 32 | charactor);
            if (!charactorMap.TryGetValue(key, out result))
            {
                if (fontMap.TryGetValue(font.Id, out fontface))
                {
                    if (fontface.IsLoaded)
                    {
                        var glyph = fontface.Font.LoadGlyph(charactor);
                        charactorMap[key] = result = CreateGlyphTexture(glyph);
                    }
                }
                else
                {
                    LoadFontFace(font);
                }
            }

            return result;
        }

        private async Task LoadFontFace(FontId font)
        {
            var fontFace = await loader.LoadFont(font);
            fontMap[font.Id] = new FontFace { Font = fontFace, IsLoaded = true };
        }

        private Texture<T> CreateGlyphTexture(GlyphLoadResult glyph)
        {
            if (glyph.Texture == null)
            {
                return null;
            }

            if (glyph.CreatesNewTexture)
            {
                return textureBuilder = Create8BppTexture(glyph.Texture.Width, glyph.Texture.Height, glyph.Texture.Pixels);
            }

            Debug.Assert(textureBuilder != null);

            Update8bppTexture(textureBuilder.PlatformTexture, glyph.Texture.Width, glyph.Texture.Height, glyph.Texture.Pixels);

            return textureBuilder;
        }

        protected abstract Texture<T> Create8BppTexture(int width, int height, byte[] pixels);
        protected abstract void Update8bppTexture(T texture, int width, int height, byte[] pixels);

        Task IFontPreloader.Preload(params FontId[] fonts)
        {
            if (fonts.Length <= 0) return Task.FromResult(0);
            if (syncContext == null) throw new ArgumentNullException(nameof(SynchronizationContext));

            var tcs = new TaskCompletionSource<int>();

            syncContext.Post(async _ =>
            {
                await Task.WhenAll(fonts
                    .Where(fontId => !fontMap.ContainsKey(fontId.Id) || !fontMap[fontId.Id].IsLoaded)
                    .Select(LoadFontFace));

                tcs.SetResult(0);

            }, null);

            return tcs.Task;
        }
    }
}
