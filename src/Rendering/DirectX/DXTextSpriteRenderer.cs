namespace Nine.Graphics.Rendering
{
    using System;
    using SharpDX.Direct3D12;

    public class DXTextSpriteRenderer : TextSpriteRenderer<DXTexture>
    {
        public DXTextSpriteRenderer(DXFontTextureFactory textureFactory) 
            : base(textureFactory)
        {
        }
    }
}
