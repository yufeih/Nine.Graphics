namespace Nine.Graphics
{
    using System;
    using System.Numerics;
    using Xunit;
    using Nine.Imaging;
    using System.Threading.Tasks;
    using Nine.Graphics.Rendering;
    using Nine.Graphics.Content;
    using Nine.Injection;
    using Microsoft.Framework.Runtime;

    public class FontRendererTest : GraphicsTest
    {
        [Fact]
        public void build_font_face()
        {
            var fontLoader = Container.Get<Content.OpenGL.FontLoader>();
            
        }
    }
}
