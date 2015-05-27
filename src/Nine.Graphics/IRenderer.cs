namespace Nine.Graphics
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRenderer { }
    public interface IRenderer<T>
    {
        void Draw(T[] drawables);
    }
}
