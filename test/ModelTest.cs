namespace Nine.Graphics
{
    using System.Numerics;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ModelTest
    {
        private static readonly ModelId[] Models =
        {
            "Content/cube.fbx"
        };

        public static readonly DrawingTheoryData<Model[]> Scenes = new DrawingTheoryData<Model[]>
        {
            new [] { new Model(Models[0]) },
        };

        // [Theory, MemberData(nameof(Scenes))]
        public static async Task draw_models(Lazy<DrawingContext> context, Model[] scene)
        {
            await context.Value.ModelPreloader.Preload(Models);

            var renderer = context.Value.ModelRenderer;

            context.Value.Host.DrawFrame((width, height) =>
            {
                var view = Matrix4x4.CreateLookAt(new Vector3(-500), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadius(45), width / height, 0.1f, 1000.0f);

                renderer.Draw(view, projection, scene);
            });
        }
    }
}
