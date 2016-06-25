namespace Nine.Graphics.Rendering
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Numerics;

    public class GLTextSpriteRenderer : TextSpriteRenderer<int>
    {
        public GLTextSpriteRenderer(GLFontTextureFactory textureFactory)
            : base(textureFactory)
        {
        }
    }
}
