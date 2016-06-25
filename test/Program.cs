namespace Nine.Graphics
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var context = DrawingContext.CreateOpenGL(1024, 600);

            while (context.Host.DrawFrame(Draw)) { }
        }

        private static void Draw(int width, int height)
        {

        }
    }
}
