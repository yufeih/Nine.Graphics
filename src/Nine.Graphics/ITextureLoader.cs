namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    
    public interface ITextureLoader
    {
        Task<TextureContent> Load(string name);
    }
}
