namespace Nine.Graphics
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;
    using Nine.Injection;
    using Xunit;

    [Trait("ci", "false")]
    public class SpriteRendererTest : GraphicsTest
    {
        public static readonly TheoryData<Type, Type, string> Dimensions = new TheoryData<Type, Type, string>
        {
            { typeof(OpenGL.GraphicsHost), typeof(OpenGL.SpriteRenderer), "Content/Logo.png" },
        };

        [Theory]
        [MemberData(nameof(Dimensions))]
        public async Task draw_an_image(Type hostType, Type rendererType, string texture)
        {
            await Container.Get<IContentLoader>().Load(texture);

            var renderer = Container.Get(rendererType) as IRenderer<Sprite>;

            Draw(hostType, () =>
            {
                renderer.Draw(new[] { new Sprite(texture, size: new Vector2(100, 50)) }, null);
            });
        }
    }
}
