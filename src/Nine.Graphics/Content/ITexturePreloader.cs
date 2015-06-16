namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    
    public interface ITexturePreloader
    {
        /// <summary>
        /// Preloads the textures so that they can immediately be used by the graphics system.
        /// </summary>
        Task Preload(IEnumerable<TextureId> textures);
    }
}
