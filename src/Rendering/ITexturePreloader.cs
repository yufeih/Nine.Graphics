namespace Nine.Graphics.Rendering
{
    using System.Threading.Tasks;
    
    public interface ITexturePreloader
    {
        /// <summary>
        /// Provides a thread safe way to preloads the textures 
        /// so that they can immediately be used by the graphics system.
        /// </summary>
        Task Preload(params TextureId[] textures);
    }
}
