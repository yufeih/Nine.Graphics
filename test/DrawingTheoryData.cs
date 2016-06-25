namespace Nine.Graphics
{
    using System;
    using Xunit;

    public class DrawingTheoryData<T> : TheoryData<Lazy<DrawingContext>, T>
    {
        public void Add(T value)
        {
            Add(DrawingContext.OpenGL, value);
            // Add(DrawingContext.DirectX, value);
        }
    }
}
