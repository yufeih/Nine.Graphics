namespace Nine.Graphics
{
    public struct TextureId
    {
        public readonly string Name;
        public readonly int Id;

        public TextureId(string name)
        {
            this.Name = name;
            this.Id = 0;
        }

        public static implicit operator TextureId(string name) => new TextureId(name);
        public static implicit operator string(TextureId textureId) => textureId.Name;
    }
}
