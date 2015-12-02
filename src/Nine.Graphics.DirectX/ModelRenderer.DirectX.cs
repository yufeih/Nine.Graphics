namespace Nine.Graphics.DirectX
{
    using System;
    using System.Numerics;

    partial class ModelRenderer
    {
        private void PlatformBeginDraw(ref Matrix4x4 view, ref Matrix4x4 projection)
        {
            throw new NotImplementedException();
        }

        private unsafe void PlatformDraw(Graphics.Model* model)
        {
            throw new NotImplementedException();
        }

        private void PlatformEndDraw()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose()
        {
            throw new NotImplementedException();
        }
    }
}
