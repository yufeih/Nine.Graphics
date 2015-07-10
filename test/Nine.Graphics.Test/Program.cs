namespace Nine.Graphics
{
    using Nine.Graphics.Rendering;
    using Nine.Graphics.Runner;
    using Nine.Injection;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class Program
    {
        private readonly IHostWindow hostWindow;
        private readonly ISharedMemory sharedMemory;

        public Program(IHostWindow hostWindow, ISharedMemory sharedMemory)
        {
            this.hostWindow = hostWindow;
            this.sharedMemory = sharedMemory;
        }

        public void Main(string[] args)
        {
            if (!CheckIfGacIsNotPatched()) return;

            RunOpenGL();
            // RunWinForm();
        }

        private void RunWinForm()
        {
            var form = new Form { BackColor = System.Drawing.Color.DarkGray };
            hostWindow.Attach(form.Handle);
            Application.Run(form);
        }

        private void RunOpenGL()
        {
            // Delay loaded assemblies will block the current thread due to 
            // DesignTimeHostProjectCompiler consumes the result of an async task. 
            var img = new Nine.Imaging.Image();

            var container = GraphicsContainer.CreateOpenGLContainer(100, 100);
            var host = (OpenGL.GraphicsHost)container.Get<IGraphicsHost>();

            // OpenTK internally creates a child window docked inside a parent window,
            // the handle returned here is the child window, we need to attach the
            // parent window to the host.
            var handle = host.Window.WindowInfo.Handle;
            var parentHandle = GetParent(handle);
            hostWindow.Attach(parentHandle != IntPtr.Zero ? parentHandle : handle);

            var texture = "https://avatars0.githubusercontent.com/u/511355?v=3&s=460";
            // var texture = TextureId.White;
            var sprites = new[]
            {
                new Sprite(texture, size:new Vector2(80, 80), rotation:50),
                new Sprite(texture, size:new Vector2(80, 80), position:new Vector2(80, 0)),
                new Sprite(texture, size:new Vector2(80, 80), position:new Vector2(160, 0)),
                new Sprite(texture, size:new Vector2(80, 80), position:new Vector2(240, 0)),
            };

            var renderer = container.Get<ISpriteRenderer>();

            while (true)
            {
                host.DrawFrame((w, h) =>
                {
                    var camera = Matrix4x4.CreateOrthographicOffCenter(0, w, h, 0, 0, 1);
                    renderer.Draw(camera, sprites);
                });
            }
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        private bool CheckIfGacIsNotPatched()
        {
            try
            {
                UseMatrix4x4();
                return true;
            }
            catch (MissingMethodException)
            {
                Trace.TraceError(
                    "Please patch System.Numerics.Vectors.dll using the following command:\n" +
                    @"    gacutil / i % DNX_HOME %\packages\System.Numerics.Vectors\4.0.0\lib\win8\System.Numerics.Vectors.dll / f" + "\n\n" +
                    @"See https://github.com/dotnet/corefx/issues/313 for details");
                return false;
            }
        }

        private void UseMatrix4x4()
        {
            System.Numerics.Matrix4x4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
        }
    }
}
