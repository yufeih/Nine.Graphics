namespace Nine.Graphics
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Nine.Imaging;
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
                .Map(new OpenGL.GraphicsHost(1024, 768, null, false));
        }

        public IDisposable Frame(Type hostType, [CallerMemberName]string name = null)
        {
            var host = Container.Get(hostType) as IGraphicsHost;
            host.BeginFrame();
            return new Disposable(() => 
            {
                var texture = host.GetTexture();
                host.EndFrame();
                SaveFrame(texture, $"bin/TestResults/{ GetType().Name }/{ name }-{ frameCounter++ }.png");
            });
        }

        private void SaveFrame(TextureContent texture, string filename)
        {
            var image = new Image();
            image.SetPixels(texture.Width, texture.Height, texture.Pixels);

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

        class Disposable : IDisposable
        {
            private readonly Action onDisposed;

            public Disposable(Action onDisposed)
            {
                this.onDisposed = onDisposed;
            }

            public void Dispose() => onDisposed?.Invoke();
        }
    }
}
