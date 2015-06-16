namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    
    public interface IContentLoader<T>
    {
        Task<T> Load(string name);
    }
}
