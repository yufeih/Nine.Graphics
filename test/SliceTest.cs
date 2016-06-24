namespace Nine.Graphics
{
    using System;
    using System.Linq;
    using Xunit;

    public class SliceTest
    {
        [Fact]
        public void enumerate_a_slice()
        {
            Assert.True( 
                new Slice<int>(new[] { 0, 1, 2, 3 }, 1, 2)
                .SequenceEqual(new[] { 1, 2 }));
        }
    }
}
