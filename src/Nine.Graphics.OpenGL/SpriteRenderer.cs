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

        public void Draw(Slice<Sprite> drawables)
        {
            for (var i = drawables.Begin; i < drawables.End; i++)
            {
                var drawable = drawables.Items[i];
                var texture = textureFactory.GetTexture(drawable.Texture);
            }
        }
    }
}
