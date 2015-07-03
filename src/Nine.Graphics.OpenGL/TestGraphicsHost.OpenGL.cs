namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    partial class TestGraphicsHost : IDisposable
    {
        private readonly GameWindow window;
        
        public TestGraphicsHost(int width, int height, GraphicsMode mode = null)
        {
            GLDebug.CheckAccess();

            this.Width = width;
            this.Height = height;
            this.window = new GameWindow(width, height, mode);
            this.framePixelsA = new byte[width * height * 4];
            this.framePixelsB = new byte[width * height * 4];

            GL.ClearColor(Color.FromArgb(Branding.Color.R, Branding.Color.G, Branding.Color.B, Branding.Color.A));
        }

        private void PlatformBeginFrame()
        {
            GLDebug.CheckAccess();

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        }

        private void PlatformEndFrame(byte[] pixels)
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

            window.SwapBuffers();
        }

        public void Dispose()
        {
            GLDebug.CheckAccess();

            window.Dispose();
        }
    }
}
