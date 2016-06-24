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
               .Map<ITexturePreloader, OpenGL.TextureFactory>()
               .Map<IFontPreloader, OpenGL.FontTextureFactory>()
               .Map<IModelPreloader, OpenGL.ModelFactory>()
               .Map<ISpriteRenderer, OpenGL.SpriteRenderer>()
               .Map<ITextSpriteRenderer, OpenGL.TextSpriteRenderer>()
               .Map<IModelRenderer, OpenGL.ModelRenderer>();
            
            if (test)
            {
                container.Map<IGraphicsHost>(new OpenGL.TestGraphicsHost(width, height));
            }
            else
            {
                var host = new OpenGL.GraphicsHost(width, height);

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
               .Map<ITexturePreloader, DirectX.TextureFactory>()
               .Map<IFontPreloader, DirectX.FontTextureFactory>()
               .Map<IModelPreloader, DirectX.ModelFactory>()
               .Map<ISpriteRenderer, DirectX.SpriteRenderer>()
               .Map<ITextSpriteRenderer, DirectX.TextSpriteRenderer>()
               .Map<IModelRenderer, DirectX.ModelRenderer>();
            
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

            container.Freeze();
            return container;
        }
    }
}
