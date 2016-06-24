namespace Nine.Graphics.Rendering
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;

    public abstract class TextureFactory<T> : ITexturePreloader
    {
        enum LoadState { None, Loading, Loaded, Failed, Missing }

        struct Entry
        {
            public LoadState LoadState;
            public Texture<T> Slice;

            public override string ToString() => $"{ LoadState }: { Slice }";
        }

        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private Entry[] _textures;

        public IGraphicsHost GraphicsHost { get; }
        public ITextureLoader TextureLoader { get; }

        public TextureFactory(IGraphicsHost graphicsHost, ITextureLoader loader, int capacity = 1024)
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            GraphicsHost = graphicsHost;
            TextureLoader = loader;

            _textures = new Entry[capacity];
        }

        public abstract Texture<T> CreateTexture(TextureContent data);

        public Texture<T> GetTexture(TextureId textureId)
        {
            if (textureId.Id <= 0) return null;

            if (_textures.Length <= textureId.Id)
            {
                Array.Resize(ref _textures, MathHelper.NextPowerOfTwo(TextureId.Count));
            }

            var entry = _textures[textureId.Id];
            if (entry.LoadState == LoadState.None)
            {
                LoadTexture(textureId);

                // Ensures the method returns a valid result when the
                // async load method succeeded synchroniously.
                entry = _textures[textureId.Id];
            }

            return entry.LoadState != LoadState.None ? entry.Slice : null;
        }

        private async Task LoadTexture(TextureId textureId)
        {
            try
            {
                _textures[textureId.Id].LoadState = LoadState.Loading;

                var data = await TextureLoader.Load(textureId.Name);
                if (data == null)
                {
                    await LoadTexture(TextureId.Missing);
                    _textures[textureId.Id].Slice = _textures[TextureId.Missing.Id].Slice;
                    _textures[textureId.Id].LoadState = LoadState.Missing;
                    return;
                }

                _textures[textureId.Id].Slice = CreateTexture(data);
                _textures[textureId.Id].LoadState = LoadState.Loaded;
            }
            catch
            {
                await LoadTexture(TextureId.Error);
                _textures[textureId.Id].Slice = _textures[TextureId.Error.Id].Slice;
                _textures[textureId.Id].LoadState = LoadState.Failed;
            }
        }

        Task ITexturePreloader.Preload(params TextureId[] textures)
        {
            if (textures.Length <= 0) return Task.FromResult(0);
            if (_syncContext == null) throw new ArgumentNullException(nameof(SynchronizationContext));

            var tcs = new TaskCompletionSource<int>();

            _syncContext.Post(async _ =>
            {
                var maxId = textures.Max(texture => texture.Id);
                if (this._textures.Length <= maxId)
                {
                    Array.Resize(ref this._textures, MathHelper.NextPowerOfTwo(TextureId.Count));
                }

                await Task.WhenAll(textures
                    .Where(textureId => this._textures[textureId.Id].LoadState == LoadState.None)
                    .Select(LoadTexture));

                tcs.SetResult(0);

            }, null);

            return tcs.Task;
        }
    }
}
