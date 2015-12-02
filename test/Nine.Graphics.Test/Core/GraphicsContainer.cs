namespace Nine.Graphics
{
    using System;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using Microsoft.Framework.Runtime.Infrastructure;
    using Microsoft.Framework.Runtime;
    using Nine.Hosting;
    using System.Runtime.InteropServices;

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

            SetupCallContextDependencies(container);

            if (test)
            {
                container.Map<IGraphicsHost>(new OpenGL.TestGraphicsHost(width, height));
            }
            else
            {
                var host = new OpenGL.GraphicsHost(width, height);
                var hostWindow = container.Get<IHostWindow>();
                if (hostWindow != null)
                {
                    // OpenTK internally creates a child window docked inside a parent window,
                    // the handle returned here is the child window, we need to attach the
                    // parent window to the host.
                    var handle = host.Window.WindowInfo.Handle;
                    var parentHandle = GetParent(handle);
                    hostWindow.Attach(parentHandle != IntPtr.Zero ? parentHandle : handle);
                }

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

            SetupCallContextDependencies(container);

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

        private static void SetupCallContextDependencies(IContainer container)
        {
            try
            {
                // https://github.com/xunit/dnx.xunit/issues/27
                var sp = CallContextServiceLocator.Locator.ServiceProvider;
                container.Map((IApplicationEnvironment)sp.GetService(typeof(IApplicationEnvironment)))
                         .Map((NuGetDependencyResolver)sp.GetService(typeof(NuGetDependencyResolver)))
                         .Map((IHostWindow)sp.GetService(typeof(IHostWindow)))
                         .Map((ISharedMemory)sp.GetService(typeof(ISharedMemory)));
            }
            catch
            {

            }
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);
    }
}
