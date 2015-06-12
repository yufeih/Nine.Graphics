namespace Nine.Graphics
{
    using System.Collections.Generic;

    public interface IRenderer<T>
    {
        void Draw(IEnumerable<T> input, ObjectPool output);
    }
}
