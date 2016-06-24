namespace Nine.Graphics
{
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class ModelTest : DrawTest<ModelTest>, IDrawTest
    {
        private static readonly ModelId[] models =
        {
            "Content/cube.fbx"
        };

        private static readonly Model[][] scenes =
        {
            new [] { new Model(models[0]) },
        };

        public IEnumerable<Drawing> GetDrawings()
        {
            return scenes.Select(CreateDrawing);
        }

        private static Drawing CreateDrawing(Model[] scene)
        {
            return new Drawing(

                draw: (container, width, height) =>
                {
                    var renderer = container.Get<IModelRenderer>();
                    var view = Matrix4x4.CreateLookAt(new Vector3(-500), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                    var projection = Matrix4x4.CreatePerspectiveFieldOfView(45, width / height, 0.1f, 1000.0f);
                    renderer.Draw(view, projection, scene);
                },

                beforeDraw: container =>
                {
                    return container.Get<IModelPreloader>().Preload(ModelTest.models);
                }
            );
        }
    }
}
