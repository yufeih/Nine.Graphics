namespace Nine.Graphics
{
    using System;
    using Xunit;

    public class EmptyGraphicsHostTest : GraphicsTest
    {
        [Theory]
        [InlineData(typeof(OpenGL.GraphicsHost))]
        public void DummyLoop(Type hostType)
        {
            Frame(hostType, () => { });
        }
    }
}
