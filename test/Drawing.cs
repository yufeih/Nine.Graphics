namespace Nine.Graphics
{
    using System;
    using System.Threading.Tasks;
    using Nine.Graphics.Rendering;
    using Nine.Injection;

    public class Drawing
    {
        public string Name;

        private readonly Func<IContainer, Task> _beforeDraw;
        private readonly Action<IContainer, int, int> _draw;

        public Drawing(Action<IContainer, int, int> draw, Func<IContainer, Task> beforeDraw = null, string name = null)
        {
            Name = name;
            _draw = draw;
            _beforeDraw = beforeDraw;
        }

        public async Task Draw(IContainer container, string hostName)
        {
            if (_beforeDraw != null)
            {
                await _beforeDraw(container);
            }

            var host = container.Get<IGraphicsHost>();
            if (host != null)
            {
                host.DrawFrame((w, h) => _draw?.Invoke(container, w, h), $"{Name}-{hostName}");
            }
        }

        public override string ToString() => Name;
    }
}
