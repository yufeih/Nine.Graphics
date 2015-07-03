namespace Nine.Graphics
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using Xunit;

    public class SpriteRendererTest : GraphicsTest
    {
        public static readonly TextureId[] textures =
        {
            "http://findicons.com/files/icons/1700/2d/512/game.png",
            "https://avatars0.githubusercontent.com/u/511355?v=3&s=460",
            "Content/Logo.png",
            TextureId.White,
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
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:10),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(200, 200), rotation:20),
                new Sprite(textures[0], size:new Vector2(100, 50), position:new Vector2(300, 300), scale:new Vector2(2), rotation:100),
                new Sprite(textures[0], size:new Vector2(200, 50), position:new Vector2(300, 300), scale:new Vector2(2), rotation:10, origin:new Vector2(0.333f, 0.666f)),
            },
            new []
            {
                new Sprite(textures[0], size:new Vector2(80, 80)),
                new Sprite(textures[1], size:new Vector2(80, 80), position:new Vector2(80, 0)),
                new Sprite(textures[2], size:new Vector2(80, 80), position:new Vector2(160, 0)),
                new Sprite(textures[3], size:new Vector2(80, 80), position:new Vector2(240, 0)),
            },
            new []
            {
                new Sprite(textures[1], size:new Vector2(80, 80)),
                new Sprite(textures[1], size:new Vector2(80, 80), color:Color.White * 0.2f, position:new Vector2(60, 0)),
                new Sprite(textures[1], size:new Vector2(80, 80), color:new Color(255, 255, 0), position:new Vector2(120, 0)),
            },
        };
        
        [Theory]
        [MemberData(nameof(Containers))]
        public async Task draw_an_image(Lazy<IContainer> container)
        {
            var renderer = container.Value.Get<ISpriteRenderer>();
            var camera = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);

            await container.Value.Get<ITexturePreloader>().Preload(textures);

            foreach (var scene in scenes)
            {
                Frame(container.Value, () => renderer.Draw(camera, scene));
            }
        }
    }
}
