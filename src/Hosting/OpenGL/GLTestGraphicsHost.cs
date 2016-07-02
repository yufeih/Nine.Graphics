namespace Nine.Graphics.Rendering
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    public class GLTestGraphicsHost : TestGraphicsHost, IDisposable
    {
        private readonly GameWindow _window;

        public GLTestGraphicsHost(
            int width, int height, GraphicsMode mode = null,
            int testDuration = 1000, float epsilon = 0.001f, string outputPath = null)
            : base("gl", width, height, testDuration, epsilon, outputPath)
        {
            GLDebug.CheckAccess();

            _window = new GameWindow(width, height, mode) { VSync = VSyncMode.Off };

            GL.ClearColor(Color.Transparent);
        }

        protected override void BeginFrame()
        {
            GLDebug.CheckAccess();

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        protected override void EndFrame(byte[] pixels)
        {
            GLDebug.CheckAccess();

            if (pixels != null)
            {
                GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

                // Flip Y
                for (int y = 0; y < Height / 2; y++)
                {
                    var a = y * Width * 4;
                    var b = (Height - y - 1) * Width * 4;

                    for (int x = 0; x < Width * 4; x++)
                    {
                        var tmp = pixels[a + x];
                        pixels[a + x] = pixels[b + x];
                        pixels[b + x] = tmp;
                    }
                }
            }

            _window.SwapBuffers();
        }

        public void Dispose()
        {
            GLDebug.CheckAccess();

            _window.Dispose();
        }
    }
}
