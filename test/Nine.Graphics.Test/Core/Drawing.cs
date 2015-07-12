namespace Nine.Graphics
{
    using Nine.Graphics.Rendering;
    using Nine.Injection;
    using System;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class Drawing : IXunitSerializable
    {
        public string Name;

        private readonly Func<IContainer, Task> beforeDraw;
        private readonly Action<IContainer, int, int> draw;

        public Drawing() { } // Needed for deserializer
        public Drawing(Action<IContainer, int, int> draw, Func<IContainer, Task> beforeDraw = null, string name = null)
        {
            this.Name = name;
            this.draw = draw;
            this.beforeDraw = beforeDraw;
        }

        public async Task Draw(IContainer container)
        {
            if (beforeDraw != null)
            {
                await beforeDraw(container);
            }

            var host = container.Get<IGraphicsHost>();
            if (host != null)
            {
                host.DrawFrame((w, h) => draw?.Invoke(container, w, h), Name ?? "<noname>");
            }
        }

        public void Deserialize(IXunitSerializationInfo info)
        {

        }

        public void Serialize(IXunitSerializationInfo info)
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
