﻿namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Drawing;
    using Nine.Graphics.Rendering;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using System.Runtime.CompilerServices;

    public sealed class GraphicsHost : IGraphicsHost, IDisposable
    {
        public readonly GameWindow Window;

        public GraphicsHost(int width, int height, GraphicsMode mode = null, bool vSync = true)
            : this(new GameWindow(width, height, mode, "Nine.Graphics", GameWindowFlags.Default) { VSync = vSync ? VSyncMode.On : VSyncMode.Off, Visible = true })
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

        public bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null)
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
