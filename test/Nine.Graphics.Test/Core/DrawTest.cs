namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Nine.Injection;
    using Xunit;
    using System.Collections.Generic;

    public interface IDrawTest
    {
        IEnumerable<Drawing> GetDrawings();
    }

    public abstract class DrawTest<T> where T : IDrawTest, new()
    {
        public static string OutputPath = "TestResults";

        public static IContainer OpenGlContainer => glContainer.Value;
        public static IContainer DirectXContainer => dxContainer.Value;

        private static readonly Lazy<IContainer> glContainer = new Lazy<IContainer>(() => GraphicsContainer.CreateOpenGLContainer(1024, 768, true));
        private static readonly Lazy<IContainer> dxContainer = new Lazy<IContainer>(() => GraphicsContainer.CreateDirectXContainer(1024, 768, true));

        public static readonly TheoryData<Drawing> TestDimensions = new DrawingBuilder();

        [Theory]
        [MemberData(nameof(TestDimensions))]
        public Task gl(Drawing scene) => scene?.Draw(OpenGlContainer) ?? Task.FromResult(0);

        [Theory]
        [MemberData(nameof(TestDimensions))]
        public Task dx(Drawing scene) => scene?.Draw(DirectXContainer) ?? Task.FromResult(0);

        class DrawingBuilder : TheoryData<Drawing>
        {
            public DrawingBuilder()
            {
                var test = new T();
                var i = 0;

                foreach (var drawing in test.GetDrawings())
                {
                    var name = $"{ i++ }";
                    if (drawing.Name != null)
                    {
                        name += "-" + drawing.Name;
                    }

                    drawing.Name = name;
                    Add(drawing);
                }
            }
        }
    }
}
