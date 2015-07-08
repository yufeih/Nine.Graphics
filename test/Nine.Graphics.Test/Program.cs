namespace Nine.Graphics
{
    using Nine.Graphics.Runner;
    using Nine.Injection;
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    public class Program
    {
        private readonly IHostWindow hostWindow;

        public Program(IHostWindow hostWindow)
        {
            this.hostWindow = hostWindow;
        }

        public void Main(string[] args)
        {
            if (!CheckIfGacIsNotPatched()) return;

            RunOpenGL();
            // RunWinForm();
        }

        private void RunWinForm()
        {
            var form = new Form { BackColor = System.Drawing.Color.Blue };
            hostWindow.Attach(form.Handle);
            Application.Run(form);
        }

        private void RunOpenGL()
        {
            var host = new OpenGL.GraphicsHost(100, 100);

            // OpenTK internally creates a child window docked inside a parent window,
            // the handle returned here is the child window, we need to attach the
            // parent window to the host.
            var handle = host.Window.WindowInfo.Handle;
            var parentHandle = GetParent(handle);
            hostWindow.Attach(parentHandle != IntPtr.Zero ? parentHandle : handle);

            while (true)
            {
                host.DrawFrame((w, h) =>
                {

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
