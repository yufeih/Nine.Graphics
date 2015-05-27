namespace Nine.Graphics
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IContentLoader
    {
        Task<Stream> Load(string name);
    }
}
