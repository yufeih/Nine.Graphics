namespace Nine.Graphics.Rendering
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IGraphicsHost
    {
        bool IsAvailable { get; }

        /// <summary>
        /// Draws one frame using the draw action.
        /// </summary>
        /// <param name="draw">
        /// An action that takes the width and height of the host
        /// and performs the actual drawing operations for this frame.
        /// </param>
        /// <returns>
        /// Returns true if the host can accept more frames.
        /// </returns>
        bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null);
    }
}
