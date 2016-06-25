namespace Nine.Graphics.Rendering
{
    using System;
    using System.Numerics;

    public class GLModelRenderer : ModelRenderer<int>
    {
        protected override void BeginDraw(ref Matrix4x4 view, ref Matrix4x4 projection)
        {
            throw new NotImplementedException();
        }

        protected override unsafe void Draw(Graphics.Model* model)
        {
            throw new NotImplementedException();
        }

        protected override void EndDraw()
        {
            throw new NotImplementedException();
        }
    }
}
