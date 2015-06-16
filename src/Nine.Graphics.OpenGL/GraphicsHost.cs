namespace Nine.Graphics.OpenGL
{
    using System;
    using Nine.Graphics.Content;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    public class GraphicsHost : IGraphicsHost
    {
        private readonly GameWindow window;

        private byte[] framePixels;

        public int Width => window.Width;
        public int Height => window.Height;

        public IntPtr WindowHandle => window.WindowInfo.Handle;

        public GraphicsHost(int width, int height, GraphicsMode mode = null, bool hidden = false, bool vSync = true)
            : this(new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.FixedWindow) { VSync = vSync ? VSyncMode.On : VSyncMode.Off }, hidden)
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

        public bool BeginFrame()
        {
            window.ProcessEvents();

            if (window.IsExiting)
                return false;

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            return true;
        }

        public void EndFrame()
        {
            window?.SwapBuffers();
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
