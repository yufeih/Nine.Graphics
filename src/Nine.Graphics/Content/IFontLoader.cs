namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;

    public interface IFontLoader
    {
        Task<IFontRasterizer> LoadFont(string font = null);
    }
}
