namespace Nine.Graphics
{
    using System;
    using Nine.Injection;
    using Xunit;

    public class EmptyGraphicsHostTest : GraphicsTest
    {
        [Theory]
        [MemberData(nameof(Containers))]
        public void DummyLoop(Lazy<IContainer> container)
        {
            Frame(container.Value, () => { });
        }
    }
}
