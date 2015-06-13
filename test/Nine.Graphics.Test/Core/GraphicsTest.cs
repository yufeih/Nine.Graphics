namespace Nine.Graphics
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using Nine.Injection;

    public class GraphicsTest
    {
        private int frameCounter = 0;

        public readonly IContainer Container = new Container();

        public GraphicsTest()
        {
            Container
                .Map<IContentLoader, ContentLoader>()
                .Map<ITextureLoader, TextureLoader>()
                .Map(new OpenGL.GraphicsHost(1024, 768, null, true));
        }

        public void Draw(Type hostType, Action draw, [CallerMemberName]string name = null)
        {
            var host = Container.Get(hostType) as IGraphicsHost;
            if (host.BeginFrame())
            {
                draw();
                SaveFrame(host.GetTexture(), $"bin/TestResults/{ GetType().Name }/{ name }-{ frameCounter++ }.png");
                host.EndFrame();
            }
        }

        private void SaveFrame(TextureContent texture, string filename)
        {
            var image = new Image();
            image.SetPixels(texture.Width, texture.Height, texture.Pixels);

            // glGetPixels read pixels with Y axis flipped.
            image = image.FlipY();

            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = File.OpenWrite(filename))
            {
                image.SaveAsPng(stream);
            }
        }
    }
}
