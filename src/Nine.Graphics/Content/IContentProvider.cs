namespace Nine.Graphics.Content
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IContentProvider
    {
        /// <summary>
        /// Opens a stream for the given content name.
        /// </summary>
        Task<Stream> Open(string name);
    }
}
