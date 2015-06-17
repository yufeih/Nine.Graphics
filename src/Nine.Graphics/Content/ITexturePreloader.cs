namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    
    public interface ITexturePreloader
    {
        /// <summary>
        /// Preloads the textures so that they can immediately be used by the graphics system.
        /// </summary>
        Task Preload(params TextureId[] textures);
    }
}
