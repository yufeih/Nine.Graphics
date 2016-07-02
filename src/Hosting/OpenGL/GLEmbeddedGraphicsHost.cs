namespace Nine.Graphics.Rendering
{
    using System;
    using System.Drawing;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Platform;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a graphics host that is embedded inside a designated window.
    /// </summary>
    public sealed class EmbeddedGraphicsHost : IGraphicsHost, IDisposable
    {
        public readonly GraphicsContext GraphicsContext;

        public EmbeddedGraphicsHost(IntPtr windowHandle, GraphicsMode mode = null)
        {
            GLDebug.CheckAccess();

            GraphicsContext = new GraphicsContext(mode ?? GraphicsMode.Default, Utilities.CreateWindowsWindowInfo(windowHandle));

            GL.ClearColor(Color.Transparent);
        }

        public bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null)
        {
            GLDebug.CheckAccess();

            GL.Viewport(0, 0, 100, 100); // TODO:
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            draw(100, 100);

            GraphicsContext.SwapBuffers();

            return true;
        }

        public void Dispose()
        {
            GLDebug.CheckAccess();

            GraphicsContext.Dispose();
        }
    }
}
