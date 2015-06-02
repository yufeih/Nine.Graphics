namespace Nine.Graphics.OpenGL
{
    using System;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    public class GraphicsHost : IGraphicsHost
    {
        private readonly GameWindow window;

        private byte[] framePixels;

        public int Width => window.Width;
        public int Height => window.Height;

        public GraphicsHost(int width, int height, GraphicsMode mode = null, bool hidden = false)
            : this(new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.FixedWindow), hidden)
        { }

        public GraphicsHost(GameWindow window, bool hidden = false)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;
            if (!hidden)
            {
                this.window.Visible = true;
            }

            GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        }

        public void BeginFrame()
        {
            window.ProcessEvents();

            // TODO: Check window.IsExiting

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void EndFrame()
        {
            // TODO: Check window.IsExiting

            window.SwapBuffers();
        }

        public TextureContent GetTexture()
        {
            framePixels = framePixels ?? new byte[Width * Height * 4];
            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgra, PixelType.UnsignedByte, framePixels);
            return new TextureContent(Width, Height, framePixels);
        }

        public void Dispose() => window?.Dispose();
    }
}
