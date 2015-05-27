namespace Nine.Graphics
{
    using System.IO;
    using System.Threading.Tasks;

    public class ContentLoader : IContentLoader
    {
        public Task<Stream> Load(string name)
        {
            return Task.FromResult<Stream>(File.OpenRead(name));
        }
    }
}
