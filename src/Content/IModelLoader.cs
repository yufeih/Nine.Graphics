namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;

    public interface IModelLoader
    {
        /// <summary>
        /// Loads the target resource into a model.
        /// </summary>
        Task<ModelContent> Load(string name);
    }
}
