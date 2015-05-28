namespace Nine.Graphics
{
    public interface IRenderer<T>
    {
        void Draw(Slice<T> drawables);
    }
}
