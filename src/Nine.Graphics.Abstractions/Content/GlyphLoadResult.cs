namespace Nine.Graphics.Content
{
    public struct GlyphLoadResult
    {
        public readonly TextureContent Texture;
        public readonly bool CreatesNewTexture;

        public GlyphLoadResult(TextureContent texture, bool createsNewTexture)
        {
            this.Texture = texture;
            this.CreatesNewTexture = createsNewTexture;
        }
    }
}
