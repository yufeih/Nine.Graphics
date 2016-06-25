namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Nine.Injection;
    using Xunit;

    public interface IDrawingTest
    {
        IEnumerable<Drawing> GetDrawings();
    }

    public abstract class DrawingTest<T> where T : IDrawingTest, new()
    {
        public static string OutputPath = "TestResults";

        public static IContainer OpenGlContainer => glContainer.Value;
        public static IContainer DirectXContainer => dxContainer.Value;

        private static readonly Lazy<IContainer> glContainer = new Lazy<IContainer>(() => GraphicsContainer.CreateOpenGLContainer(400, 300, true));
        private static readonly Lazy<IContainer> dxContainer = new Lazy<IContainer>(() => GraphicsContainer.CreateDirectXContainer(400, 300, true));

        public static readonly TheoryData<Drawing> Drawings = new DrawingBuilder();

        [Theory, MemberData(nameof(Drawings))]
        public Task gl(Drawing scene) => scene?.Draw(OpenGlContainer, "gl") ?? Task.FromResult(0);

        [Theory, MemberData(nameof(Drawings))]
        public Task dx(Drawing scene) => scene?.Draw(DirectXContainer, "dx") ?? Task.FromResult(0);

        class DrawingBuilder : TheoryData<Drawing>
        {
            public DrawingBuilder()
            {
                var test = new T();
                var i = 0;

                foreach (var drawing in test.GetDrawings())
                {
                    drawing.Name = Path.Combine(typeof(T).Name, $"{++i}");
                    Add(drawing);
                }
            }
        }
    }
}
