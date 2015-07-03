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
        
        public TestGraphicsHost(
            int width, int height, GraphicsMode mode = null,
            int frameTime = 1000, float epsilon = 0.001f, string outputPath = "TestResults")
        {
            GLDebug.CheckAccess();

            this.width = width;
            this.height = height;
            this.frameTime = frameTime;
            this.epsilon = epsilon;
            this.outputPath = outputPath;
            this.window = new GameWindow(width, height, mode) { VSync = VSyncMode.Off };
            this.framePixelsA = new byte[width * height * 4];
            this.framePixelsB = new byte[width * height * 4];

            GL.ClearColor(Color.FromArgb(Branding.Color.A, Branding.Color.R, Branding.Color.G, Branding.Color.B));
        }

        private void PlatformBeginFrame()
        {
            GLDebug.CheckAccess();

            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void PlatformEndFrame(byte[] pixels)
        {
            GLDebug.CheckAccess();

            if (pixels != null)
            {
                GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

                // Flip Y
                for (int y = 0; y < height / 2; y++)
                {
                    var a = y * width * 4;
                    var b = (height - y - 1) * width * 4;

                    for (int x = 0; x < width * 4; x++)
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
