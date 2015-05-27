namespace Nine.Graphics.OpenGL
{
    using System;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL4;

    public class GraphicsHost : IGraphicsHost
    {
        private readonly GameWindow window;

        //private readonly int frameBuffer;
        //private readonly int depthBuffer;

        private byte[] framePixels;

        public int Width => window.Width;
        public int Height => window.Height;

        public GraphicsHost(int width, int height, GraphicsMode mode = null)
            : this(new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.FixedWindow))
        { }

        public GraphicsHost(GameWindow window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;

            //this.frameBuffer = CreateFrameBuffer();
            //this.depthBuffer = CreateDepthBuffer();

            this.window.Visible = true;
        }

        public void BeginFrame()
        {
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, frameBuffer, 0);
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

            GL.Viewport(0, 0, Width, Height);
            GL.ClearColor(1, 1, 1, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void EndFrame()
        {
            window.SwapBuffers();
        }

        public TextureContent GetTexture()
        {
            framePixels = framePixels ?? new byte[Width * Height * 4];
            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgra, PixelType.UnsignedByte, framePixels);
            return new TextureContent(Width, Height, framePixels);
        }

        public int CreateFrameBuffer()
        {
            var texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            return texture;
        }

        private int CreateDepthBuffer()
        {
            var texture = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, texture);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, texture);

            return texture;
        }

        public void Dispose() => window?.Dispose();
    }
}
