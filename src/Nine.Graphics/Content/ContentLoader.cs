namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    using Microsoft.Framework.Caching.Memory;

    public abstract class ContentLoader<T> : IContentLoader<T>
    {
        private readonly MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public async Task<T> Load(string name)
        {
            T result;
            if (cache.TryGetValue(name, out result)) return result;

            result = await LoadContent(name).ConfigureAwait(false);
            cache.Set(name, result);
            return result;
        }

        protected abstract Task<T> LoadContent(string name);
    }
}
