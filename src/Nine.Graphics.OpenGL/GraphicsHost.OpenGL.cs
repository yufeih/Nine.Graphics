namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Drawing;
    using Nine.Graphics.Rendering;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    public sealed class GraphicsHost : IGraphicsHost, IDisposable
    {
        public readonly GameWindow Window;

        public GraphicsHost(int width, int height, GraphicsMode mode = null, bool vSync = true, bool hidden = false)
            : this(new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.FixedWindow)
            {
                VSync = vSync ? VSyncMode.On : VSyncMode.Off,
                Visible = !hidden,
            })
        { }

        public GraphicsHost(GameWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            this.Window = window;

            GLDebug.CheckAccess();

            GL.ClearColor(Color.FromArgb(Branding.Color.R, Branding.Color.G, Branding.Color.B, Branding.Color.A));
        }

        public bool DrawFrame(Action<int, int> draw)
        {
            GLDebug.CheckAccess();

            Window.ProcessEvents();

            if (Window.IsExiting)
            {
                return false;
            }

            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            draw(Window.Width, Window.Height);

            Window.SwapBuffers();

            return true;
        }

        public void Dispose()
        {
            GLDebug.CheckAccess();

            Window.Dispose();
        }
    }
}
