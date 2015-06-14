namespace Nine.Graphics
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;
    using Nine.Injection;
    using Xunit;
    using System.Linq;

    [Trait("ci", "false")]
    public class SpriteRendererTest : GraphicsTest
    {
        public static readonly string[] textures =
        {
            "Content/Logo.png",
        };

        public static readonly Sprite[][] scenes =
        {
            new [] { new Sprite(), new Sprite("not exist"), new Sprite(textures[0]) },
            new [] 
            {
                new Sprite(textures[0], size:new Vector2(100, 50)),
                new Sprite(textures[0], size:new Vector2(100, 100), position:new Vector2(100, 0)),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:(float)(Math.PI * 0.01)),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:(float)(Math.PI * 0.02)),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(300, 300), scale:new Vector2(2), rotation:(float)(Math.PI * 0.52)),
                new Sprite(textures[0], size:new Vector2(200, 50), position:new Vector2(300, 300), scale:new Vector2(2), rotation:(float)(Math.PI * 0.02), origin:new Vector2(0.333f, 0.666f)),
            },
        };

        public static readonly TheoryData<Type, Type> Dimensions = new TheoryData<Type, Type>()
        {
            { typeof(OpenGL.GraphicsHost), typeof(OpenGL.SpriteRenderer) },
        };

        [Theory]
        [MemberData(nameof(Dimensions))]
        public async Task draw_an_image(Type hostType, Type rendererType)
        {
            var contentLoader = Container.Get<IContentLoader>();

            try
            {
                await Task.WhenAll(textures.Select(contentLoader.Load));
            }
            catch { }

            var renderer = Container.Get(rendererType) as IRenderer<Sprite>;

            foreach (var scene in scenes)
            {
                Draw(hostType, () =>
                {
                    renderer.Draw(scene, null);
                });
            }
        }
    }
}
