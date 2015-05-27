namespace Nine.Graphics
{
    using Nine.Injection;

    public class TestBase
    {
        public readonly IContainer Container = new Container();

        public TestBase()
        {
            Container
                .Map<IContentLoader, ContentLoader>()
                .Map<ITextureLoader, TextureLoader>()
                .Map(new OpenGL.GraphicsHost(1024, 768));
        }
    }
}
