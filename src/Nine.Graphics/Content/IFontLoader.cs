namespace Nine.Graphics.Content
{
    using System.Threading.Tasks;

    public interface IFontLoader
    {
        Task<IFontFace> LoadFont(string font = null);
    }
}
