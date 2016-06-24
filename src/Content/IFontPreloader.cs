namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;
    
    public interface IFontPreloader
    {
        Task Preload(params FontId[] fonts);
    }
}
