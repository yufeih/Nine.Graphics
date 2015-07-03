namespace Nine.Graphics
{
    using System;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;

    public static class GraphicsContainer
    {
        public static IContainer CreateOpenGLContainer(int width, int height, bool test = false, Action<IContainer> setup = null)
        {
            var container = new Container();

            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<ITexturePreloader, OpenGL.TextureFactory>()
               .Map<IFontPreloader, OpenGL.FontTextureFactory>()
               .Map<ISpriteRenderer, OpenGL.SpriteRenderer>()
               .Map<ITextSpriteRenderer, OpenGL.TextSpriteRenderer>();

            if (test)
            {
                container.Map<IGraphicsHost>(new OpenGL.TestGraphicsHost(width, height));
            }
            else
            {
                container.Map<IGraphicsHost>(new OpenGL.GraphicsHost(width, height));
            }

            if (setup != null)
            {
                setup(container);
            }

            container.Freeze();
            return container;
        }

        public static IContainer CreateDirectXContainer(int width, int height, bool test = false, Action<IContainer> setup = null)
        {
            var container = new Container();

            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<ITexturePreloader, DirectX.TextureFactory>()
               .Map<IFontPreloader, DirectX.FontTextureFactory>()
               .Map<ISpriteRenderer, DirectX.SpriteRenderer>()
               .Map<ITextSpriteRenderer, DirectX.TextSpriteRenderer>();

            if (test)
            {
                var host = new DirectX.TestGraphicsHost();
                container.Map<IGraphicsHost>(host);
                container.Map(host.Device);
            }
            else
            {
                var host = new DirectX.GraphicsHost(width, height);
                container.Map<IGraphicsHost>(host);
                container.Map(host.Device);
            }

            if (setup != null)
            {
                setup(container);
            }

            container.Freeze();
            return container;
        }
    }
}
