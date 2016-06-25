namespace Nine.Graphics
{
    using Nine.Injection;
    using Nine.Graphics.Rendering;

    public class Program
    {
        public static void Main(string[] args)
        {
            var container = GraphicsContainer.CreateOpenGLContainer(400, 300);
            var host = container.Get<GLGraphicsHost>();
            
            var testCase = new SpriteTest();
            //testCase.draw_an_image(new Lazy<IContainer>(() => container));

            //while (host.DrawFrame(draw)) { }
        }
    }
}
