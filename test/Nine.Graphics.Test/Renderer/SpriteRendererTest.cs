namespace Nine.Graphics
{
    using System.Threading.Tasks;
    using Xunit;

    public class SpriteRendererTest
    {
        public static readonly TheoryData<IRenderer<Sprite>, string> Dimensions = new TheoryData<IRenderer<Sprite>, string>
        {
            { new OpenGL.SpriteRenderer(), "Content/Logo.png" },
        };

        [Theory]
        [MemberData(nameof(Dimensions))]
        public async Task draw_an_image(IRenderer<Sprite> renderer, string texture)
        {
            renderer.Draw(new[] { new Sprite(texture) });
        }
    }
}
