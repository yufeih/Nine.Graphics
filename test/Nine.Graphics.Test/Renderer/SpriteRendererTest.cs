namespace Nine.Graphics
{
    using System;
    using System.Numerics;
    using Xunit;
    using Nine.Imaging;
    using System.Threading.Tasks;

    public class SpriteRendererTest : GraphicsTest
    {
        public static readonly string[] textures =
        {
            "http://findicons.com/files/icons/1700/2d/512/game.png",
            "https://avatars0.githubusercontent.com/u/511355?v=3&s=460",
            "Content/Logo.png",
            TextureId.White.Name,
        };

        public static readonly Sprite[][] scenes =
        {
            new [] { new Sprite(), new Sprite("not exist"), new Sprite(textures[0]) },
            new [] 
            {
                new Sprite(textures[0], size:new Vector2(100, 50)),
                new Sprite(textures[0], size:new Vector2(100, 100), position:new Vector2(100, 0), color:new Color(r:255, g:0, b:0)),
                new Sprite(textures[0], size:new Vector2(100, 100), position:new Vector2(200, 0), color:new Color(r:0, g:255, b:0)),
                new Sprite(textures[0], size:new Vector2(100, 100), position:new Vector2(300, 0), color:new Color(r:0, g:0, b:255)),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:(float)(Math.PI * 0.01)),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:(float)(Math.PI * 0.02)),
                new Sprite(textures[0], size:new Vector2(100, 50), transform:Matrix3x2.CreateTranslation(200, 200), rotation:-(float)(Math.PI * 0.02)),
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
            await PreloadTextures(textures);

            var renderer = Container.Get(rendererType) as IRenderer<Sprite>;

            foreach (var scene in scenes)
            {
                Frame(hostType, () =>
                {
                    renderer.Draw(scene, null);
                });
            }
        }
    }
}
