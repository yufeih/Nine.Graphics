namespace Nine.Graphics.Rendering
{
    using System;
    using System.Runtime.CompilerServices;

    public interface ITestGraphicsHost : IGraphicsHost
    {
        bool DrawFrame(Action<int, int> draw, float epsilon, [CallerMemberName]string frameName = null);
    }
}
