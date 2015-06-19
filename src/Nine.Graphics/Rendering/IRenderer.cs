namespace Nine.Graphics.Rendering
{
    public interface IRenderer<T>
    {
        /// <summary>
        /// Draws the input drawables on to the current graphics context.
        /// </summary>
        /// <param name="drawables">
        /// This renderer will only read the drawables as if they are immutable.
        /// </param>
        void Draw(Slice<T> drawables);
    }
}
