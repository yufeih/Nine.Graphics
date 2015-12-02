#if DX
namespace Nine.Graphics.DirectX
#else
namespace Nine.Graphics.OpenGL
#endif
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;
    using Rendering;

    public partial class ModelFactory : IModelPreloader
    {
        enum LoadState { None, Loading, Loaded, Failed, Missing }

        struct Entry
        {
            public LoadState LoadState;
            public Model Slice;

            public override string ToString() => $"{ LoadState }: { Slice }";
        }

        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;
        private readonly IGraphicsHost graphicsHost;
        private readonly IModelLoader loader;
        private Entry[] models;

        public ModelFactory(IGraphicsHost graphicsHost, IModelLoader loader, int capacity = 1024)
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            this.graphicsHost = graphicsHost;
            this.loader = loader;
            this.models = new Entry[capacity];
        }

        public Model GetModel(ModelId modelId)
        {
            if (modelId.Id <= 0) return null;

            if (models.Length <= modelId.Id)
            {
                Array.Resize(ref models, MathHelper.NextPowerOfTwo(ModelId.Count));
            }

            var entry = models[modelId.Id];
            if (entry.LoadState == LoadState.None)
            {
                LoadModel(modelId);

                // Ensures the method returns a valid result when the
                // async load method succeeded synchroniously.
                entry = models[modelId.Id];
            }

            return entry.LoadState != LoadState.None ? entry.Slice : null;
        }

        private async Task LoadModel(ModelId modelId)
        {
            try
            {
                models[modelId.Id].LoadState = LoadState.Loading;

                var data = await loader.Load(modelId.Name);
                if (data == null)
                {
                    await LoadModel(ModelId.Missing);
                    models[modelId.Id].Slice = models[TextureId.Missing.Id].Slice;
                    models[modelId.Id].LoadState = LoadState.Missing;
                    return;
                }

                models[modelId.Id].Slice = PlatformCreateModel(data);
                models[modelId.Id].LoadState = LoadState.Loaded;
            }
            catch
            {
                await LoadModel(ModelId.Error);
                models[modelId.Id].Slice = models[ModelId.Error.Id].Slice;
                models[modelId.Id].LoadState = LoadState.Failed;
            }
        }

        Task IModelPreloader.Preload(params ModelId[] models)
        {
            if (models.Length <= 0) return Task.FromResult(0);
            if (syncContext == null) throw new ArgumentNullException(nameof(SynchronizationContext));

            var tcs = new TaskCompletionSource<int>();

            syncContext.Post(async _ =>
            {
                var maxId = models.Max(model => model.Id);
                if (this.models.Length <= maxId)
                {
                    Array.Resize(ref this.models, MathHelper.NextPowerOfTwo(ModelId.Count));
                }

                await Task.WhenAll(models
                    .Where(modelId => this.models[modelId.Id].LoadState == LoadState.None)
                    .Select(LoadModel));

                tcs.SetResult(0);

            }, null);

            return tcs.Task;
        }
    }
}
