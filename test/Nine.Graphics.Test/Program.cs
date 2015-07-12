namespace Nine.Graphics
{
    using Nine.Hosting;
    using Nine.Injection;
    using System;
    using System.Diagnostics;

    public class Program
    {
        public void Main(string[] args)
        {
            if (!CheckIfGacIsNotPatched()) return;

            var container = GraphicsContainer.CreateOpenGLContainer(400, 300);
            var host = container.Get<OpenGL.GraphicsHost>();
            
            var testCase = new SpriteTest();
            //testCase.draw_an_image(new Lazy<IContainer>(() => container));

            //while (host.DrawFrame(draw)) { }
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
