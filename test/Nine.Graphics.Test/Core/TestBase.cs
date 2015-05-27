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
                .Map<ITextureLoader, TextureLoader>();
        }
    }
}
