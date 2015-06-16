namespace Nine.Graphics.Content
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IContentLocator
    {
        Task<Stream> Open(string name);
    }
}
