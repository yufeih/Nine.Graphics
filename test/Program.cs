namespace Nine.Graphics
{
    using Nine.Injection;
    using System;
    using System.Diagnostics;

    public class Program
    {
        public void Main(string[] args)
        {
            var container = GraphicsContainer.CreateOpenGLContainer(400, 300);
            var host = container.Get<OpenGL.GraphicsHost>();
            
            var testCase = new SpriteTest();
            //testCase.draw_an_image(new Lazy<IContainer>(() => container));

            //while (host.DrawFrame(draw)) { }
        }

        private void UseMatrix4x4()
        {
            System.Numerics.Matrix4x4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
        }
    }
}
