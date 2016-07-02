namespace Nine.Graphics
{
    using System;
    using Nine.Graphics.Rendering;
    using Xunit;

    public class DrawingTheoryData<T> : TheoryData<Lazy<DrawingContext>, T>
    {
        public void Add(T value)
        {
            if (GLGraphicsHost.IsAvailable)
            {
                Add(DrawingContext.OpenGL, value);
            }

            // Add(DrawingContext.DirectX, value);
        }
    }
}
