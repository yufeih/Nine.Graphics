namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;
    using OpenTK.Graphics.OpenGL;

    public class TextureFactory : ITexturePreloader
    {
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        enum LoadState { None, Loading, Loaded, Failed, Missing }

        struct Entry
        {
            public LoadState LoadState;
            public TextureSlice Slice;

            public override string ToString() => $"{ LoadState }: { Slice }";
        }

        private readonly ITextureLoader loader;
        private Entry[] textures;

        public TextureFactory(ITextureLoader loader, int capacity = 1024)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            this.loader = loader;
            this.textures = new Entry[capacity];
        }

        public TextureSlice GetTexture(TextureId textureId)
        {
            if (textureId.Id <= 0) return null;

            if (textures.Length <= textureId.Id)
            {
                Array.Resize(ref textures, MathHelper.NextPowerOfTwo(TextureId.Count));
            }

            var entry = textures[textureId.Id];
            if (entry.LoadState == LoadState.None)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                LoadTexture(textureId);
#pragma warning restore CS4014

                // Ensures the method returns a valid result when the
                // async load method succeeded synchroniously.
                entry = textures[textureId.Id];
            }

            return entry.LoadState != LoadState.None ? entry.Slice : null;
        }

        public Task Preload(IEnumerable<TextureId> textures)
        {
            if (!textures.Any()) return Task.FromResult(0);

            var maxId = textures.Max(texture => texture.Id);
            if (this.textures.Length <= maxId)
            {
                Array.Resize(ref this.textures, MathHelper.NextPowerOfTwo(TextureId.Count));
            }

            return Task.WhenAll(textures.Select(texture => LoadTexture(texture)));
        }

        private async Task LoadTexture(TextureId textureId)
        {
            try
            {
                textures[textureId.Id].LoadState = LoadState.Loading;

                var data = await loader.Load(textureId.Name);
                if (data == null)
                {
                    await LoadTexture(TextureId.Missing);
                    textures[textureId.Id].Slice = textures[TextureId.Missing.Id].Slice;
                    textures[textureId.Id].LoadState = LoadState.Missing;
                    return;
                }

                if (syncContext != null)
                {
                    // Ensure PlatformCreateTexture is called on the thread that creates this TextureFactory
                    textures[textureId.Id].Slice = await CreateTexture(data).ConfigureAwait(false);
                }
                else
                {
                    textures[textureId.Id].Slice = PlatformCreateTexture(data);
                }
                textures[textureId.Id].LoadState = LoadState.Loaded;
            }
            catch
            {
                await LoadTexture(TextureId.Error);
                textures[textureId.Id].Slice = textures[TextureId.Error.Id].Slice;
                textures[textureId.Id].LoadState = LoadState.Failed;
            }
        }

        private Task<TextureSlice> CreateTexture(TextureContent data)
        {
            var tcs = new TaskCompletionSource<TextureSlice>();
            syncContext.Post(x =>
            {
                try
                {
                    tcs.SetResult(PlatformCreateTexture(data));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }, null);
            return tcs.Task;
        }

        private TextureSlice PlatformCreateTexture(TextureContent data)
        {
            var texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Pixels);

            return new TextureSlice(texture, data.Width, data.Height, 0, data.Width, 0, data.Height);
        }
    }
}
