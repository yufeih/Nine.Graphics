namespace Nine.Graphics.OpenGL
{
    using System;

    public class SpriteRenderer : IRenderer<Sprite>
    {
        private readonly TextureFactory textureFactory;

        public SpriteRenderer(TextureFactory textureFactory)
        {
            this.textureFactory = textureFactory;
        }

        public void Draw(Sprite[] drawables)
        {
            for (var i = 0; i < drawables.Length; i++)
            {
                var drawable = drawables[i];
                var texture = textureFactory.GetTexture(drawable.Texture);
            }
        }
    }
}
