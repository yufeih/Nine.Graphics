namespace Nine.Graphics
{
    using System;
    using Xunit;

    public class GraphicsHostTest
    {
        public static readonly DrawingTheoryData<string> Scenes = new DrawingTheoryData<string> { { "" } };

        [Theory, MemberData(nameof(Scenes))]
        public static void empty_scene(Lazy<DrawingContext> contextFactory, string scene)
        {
            var context = contextFactory.Value;
            if (context == null) return;

            context.DrawFrame<GraphicsHostTest>((width, height) => { });
        }
    }
}
