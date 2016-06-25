namespace Nine.Graphics.Rendering
{
    using System.Threading.Tasks;
    
    public interface IFontPreloader
    {
        Task Preload(params FontId[] fonts);
    }
}
