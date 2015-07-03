namespace Nine.Graphics
{
    using System;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using OpenTK;

    public static class GraphicsContainer
    {
        public static IContainer CreateOpenGLContainer(int width, int height, bool hide = false, Action<IContainer> setup = null)
        {
            var container = new Container();

            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<ITexturePreloader, OpenGL.TextureFactory>()
               .Map<IFontPreloader, OpenGL.FontTextureFactory>()
               .Map<ISpriteRenderer, OpenGL.SpriteRenderer>()
               .Map<ITextSpriteRenderer, OpenGL.TextSpriteRenderer>()
               .Map<IGraphicsHost>(new OpenGL.GraphicsHost(width, height, null, false, hide));

            if (setup != null)
            {
                setup(container);
            }

            container.Freeze();
            return container;
        }

        public static IContainer CreateDirectXContainer(int width, int height, bool hide = false, Action<IContainer> setup = null)
        {
            var container = new Container();
            var host = new DirectX.GraphicsHost(width, height, hide);

            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<ITexturePreloader, DirectX.TextureFactory>()
               .Map<IFontPreloader, DirectX.FontTextureFactory>()
               .Map<ISpriteRenderer, DirectX.SpriteRenderer>()
               .Map<ITextSpriteRenderer, DirectX.TextSpriteRenderer>()
               .Map<IGraphicsHost>(host)
               .Map(host.Device);

            if (setup != null)
            {
                setup(container);
            }

            container.Freeze();
            return container;
        }
    }
}
