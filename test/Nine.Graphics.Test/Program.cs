namespace Nine.Graphics
{
    using System;
    using System.Diagnostics;
    using Microsoft.Framework.Runtime;
    using Nine.Injection;
    using Nine.Graphics.Runner;

    public class Program
    {
        private readonly IHostWindow hostWindow;

        public Program(
            IApplicationEnvironment appEnv, IServiceProvider services,
            IHostWindow hostWindow, NuGetDependencyResolver nuget)
        {
            GraphicsTest.Setup = container =>
            {
                container.Map(nuget).Map(appEnv);
            };

            this.hostWindow = hostWindow;
        }

        public void Main(string[] args)
        {
            if (!CheckIfGacIsNotPatched()) return;

            var host = new OpenGL.GraphicsHost(100, 100);
            hostWindow.Attach(host.Window.WindowInfo.Handle);

            //host.DrawFrame((w, h) =>
            //{

            //});
        }

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
