namespace Nine.Graphics
{
    using System;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;

    public static class GraphicsContainer
    {
        public static IContainer CreateOpenGLContainer(int width, int height, bool test = false)
        {
            var container = new Container();
            
            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<IModelLoader, ModelLoader>()
               .Map<ITexturePreloader, GLTextureFactory>()
               .Map<IFontPreloader, GLFontTextureFactory>()
               .Map<IModelPreloader, GLModelFactory>()
               .Map<ISpriteRenderer, GLSpriteRenderer>()
               .Map<ITextSpriteRenderer, GLTextSpriteRenderer>()
               .Map<IModelRenderer, GLModelRenderer>();
            
            if (test)
            {
                container.Map<IGraphicsHost>(new GLTestGraphicsHost(width, height));
            }
            else
            {
                var host = new GLGraphicsHost(width, height);

                container.Map<IGraphicsHost>(host);
            }

            container.Freeze();
            return container;
        }

        public static IContainer CreateDirectXContainer(int width, int height, bool test = false)
        {
            var container = new Container();

            container
               .Map<IContentProvider, ContentProvider>()
               .Map<ITextureLoader, TextureLoader>()
               .Map<IFontLoader, FontLoader>()
               .Map<IModelLoader, ModelLoader>()
               .Map<ITexturePreloader, DXTextureFactory>()
               .Map<IFontPreloader, DXFontTextureFactory>()
               .Map<IModelPreloader, DXModelFactory>()
               .Map<ISpriteRenderer, DXSpriteRenderer>()
               .Map<ITextSpriteRenderer, DXTextSpriteRenderer>()
               .Map<IModelRenderer, DXModelRenderer>();
            
            if (test)
            {
                var host = new DXTestGraphicsHost(width, height);
                container.Map<IGraphicsHost>(host);
                container.Map(host.Device);
            }
            else
            {
                var host = new DXGraphicsHost(width, height);
                container.Map<IGraphicsHost>(host);
                container.Map(host.Device);
            }

            container.Freeze();
            return container;
        }
    }
}
