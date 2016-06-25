namespace Nine.Graphics.Rendering
{
    using System.Threading.Tasks;

    public interface IModelPreloader
    {
        /// <summary>
        /// Provides a thread safe way to preloads the models 
        /// so that they can immediately be used by the graphics system.
        /// </summary>
        Task Preload(params ModelId[] textures);
    }
}
