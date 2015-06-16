namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    
    public interface ITextureLoader
    {
        /// <summary>
        /// Loads the target resource into a texture.
        /// </summary>
        Task<TextureContent> Load(string name);
    }
}
