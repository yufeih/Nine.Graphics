namespace Nine.Graphics.Rendering
{
    using System;
    using System.Numerics;

    public abstract class ModelRenderer<T> : IModelRenderer
    {
        public unsafe void Draw(Matrix4x4 view, Matrix4x4 projection, Slice<Graphics.Model> models)
        {
            fixed (Graphics.Model* pModel = &models.Items[models.Begin])
            {
                Graphics.Model* model = pModel;

                BeginDraw(ref view, ref projection);

                for (int i = 0; i < models.Length; i++)
                {
                    Draw(model);
                    model++;
                }

                EndDraw();
            }
        }

        protected abstract void EndDraw();
        protected abstract unsafe void Draw(Graphics.Model* model);
        protected abstract void BeginDraw(ref Matrix4x4 view, ref Matrix4x4 projection);
    }
}
