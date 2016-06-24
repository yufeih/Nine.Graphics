#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using System;
    using System.Numerics;

    public sealed partial class ModelRenderer : IModelRenderer, IDisposable
    {
        public unsafe void Draw(Matrix4x4 view, Matrix4x4 projection, Slice<Graphics.Model> models)
        {
            fixed (Graphics.Model* pModel = &models.Items[models.Begin])
            {
                Graphics.Model* model = pModel;

                PlatformBeginDraw(ref view, ref projection);

                for (int i = 0; i < models.Length; i++)
                {
                    PlatformDraw(model);
                    model++;
                }

                PlatformEndDraw();
            }
        }

        public void Dispose()
        {
            PlatformDispose();
        }
    }
}
