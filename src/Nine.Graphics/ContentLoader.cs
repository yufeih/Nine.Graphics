namespace Nine.Graphics
{
    using System.IO;
    using System.Threading.Tasks;

    public class ContentLoader : IContentLoader
    {
        public Task<Stream> Load(string name)
        {
            if (!File.Exists(name))
                return Task.FromResult<Stream>(null);
            return Task.FromResult<Stream>(File.OpenRead(name));
        }
    }
}
