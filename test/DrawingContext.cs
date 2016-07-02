namespace Nine.Graphics
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Nine.Graphics.Content;
    using Nine.Graphics.Rendering;

    public class DrawingContext
    {
        public IGraphicsHost Host;

        public ITexturePreloader TexturePreloader;
        public IFontPreloader FontPreloader;
        public IModelPreloader ModelPreloader;

        public ISpriteRenderer SpriteRenderer;
        public ITextSpriteRenderer TextSpriteRenderer;
        public IModelRenderer ModelRenderer;


        public bool DrawFrame<T>(Action<int, int> draw, [CallerMemberName]string frameName = null)
            => Host.DrawFrame(draw, Path.Combine(typeof(T).Name, frameName));

        public static readonly IContentProvider ContentProvider = new ContentProvider();
        public static readonly ITextureLoader TextureLoader = new TextureLoader(ContentProvider);
        public static readonly IFontLoader FontLoader = new FontLoader(ContentProvider);
        public static readonly IModelLoader ModelLoader = new ModelLoader(ContentProvider);


        public static readonly Lazy<DrawingContext> OpenGL = new Lazy<DrawingContext>(() => CreateOpenGL(400, 300, true));
        public static readonly Lazy<DrawingContext> DirectX = new Lazy<DrawingContext>(() => CreateDirectX(400, 300, true));


        public static DrawingContext CreateOpenGL(int width, int height, bool test = false)
        {
            var host = test ? (IGraphicsHost)new GLTestGraphicsHost(width, height) : new GLGraphicsHost(width, height);

            var textureFactory = new GLTextureFactory(TextureLoader);
            var fontFactory = new GLFontTextureFactory(FontLoader);
            var modelFactory = new GLModelFactory(ModelLoader);

            return new DrawingContext
            {
                Host = host,
                
                TexturePreloader = textureFactory,
                FontPreloader = fontFactory,
                ModelPreloader = modelFactory,

                SpriteRenderer = new GLSpriteRenderer(textureFactory),
                TextSpriteRenderer = new GLTextSpriteRenderer(fontFactory),
                ModelRenderer = new GLModelRenderer(),
            };
        }

        public static DrawingContext CreateDirectX(int width, int height, bool test = false)
        {
            var host = new DXGraphicsHost(width, height);

            var textureFactory = new DXTextureFactory(TextureLoader);
            var fontFactory = new DXFontTextureFactory(FontLoader);
            var modelFactory = new DXModelFactory(ModelLoader);

            return new DrawingContext
            {
                Host = host,

                TexturePreloader = textureFactory,
                FontPreloader = fontFactory,
                ModelPreloader = modelFactory,

                SpriteRenderer = new DXSpriteRenderer(host, textureFactory),
                TextSpriteRenderer = new DXTextSpriteRenderer(fontFactory),
                ModelRenderer = new DXModelRenderer(),
            };
        }
    }
}
