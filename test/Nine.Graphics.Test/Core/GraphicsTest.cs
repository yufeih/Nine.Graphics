namespace Nine.Graphics
{
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using System;
    using System.Runtime.CompilerServices;
    using Xunit;

    [Trait("ci", "false")]
    public class GraphicsTest
    {
        public static bool IsTest;
        public static int Width = 1024;
        public static int Height = 768;
        public static string OutputPath = "TestResults";

        public static TheoryData<Lazy<IContainer>> Containers => new TheoryData<Lazy<IContainer>>
        {
            openGlContainer,
            // directXContainer,
        };

        public static IContainer DefaultContainer => openGlContainer.Value;

        private static readonly Lazy<IContainer> openGlContainer =
            new Lazy<IContainer>(() => GraphicsContainer.CreateOpenGLContainer(Width, Height, IsTest));

        private static readonly Lazy<IContainer> directXContainer =
            new Lazy<IContainer>(() => GraphicsContainer.CreateDirectXContainer(Width, Height, IsTest));

        public void Frame(IContainer container, Action draw, [CallerMemberName]string name = null)
        {
            var host = container.Get<IGraphicsHost>();
            host.DrawFrame((w, h) => draw(), $"{ GetType().Name }/{ name }");
        }
    }
}
