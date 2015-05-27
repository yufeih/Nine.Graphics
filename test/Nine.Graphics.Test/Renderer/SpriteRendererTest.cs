namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class SpriteRendererTest : TestBase
    {
        public static readonly TheoryData<Type, string> Dimensions = new TheoryData<Type, string>
        {
            { typeof(OpenGL.SpriteRenderer), "Content/Logo.png" },
        };

        [Theory]
        [MemberData(nameof(Dimensions))]
        public async Task draw_an_image(Type rendererType, string texture)
        {
            var renderer = (IRenderer<Sprite>)Container.Get(rendererType);
            renderer.Draw(new[] { new Sprite(texture) });
        }
    }
}
