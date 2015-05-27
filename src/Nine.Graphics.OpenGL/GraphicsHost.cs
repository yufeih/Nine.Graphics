namespace Nine.Graphics.OpenGL
{
    using System;
    using OpenTK;
    using OpenTK.Graphics;

    public class GraphicsHost : IGraphicsHost
    {
        private readonly GameWindow window;

        public int Height => window.Width;
        public int Width => window.Height;
        
        public GraphicsHost(int width, int height, GraphicsMode mode = null)
        {
            this.window = new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.FixedWindow);
        }

        public GraphicsHost(GameWindow window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;
        }

        public void BeginFrame()
        {

        }

        public void EndFrame()
        {
            window.SwapBuffers();
        }

        public TextureContent GetTexture()
        {
            return null;
        }

        public void Dispose() => window?.Dispose();
    }
}
