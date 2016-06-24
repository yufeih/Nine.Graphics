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
        public string FrameName;

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
                host.DrawFrame((w, h) => draw?.Invoke(container, w, h), FrameName ?? "<noname>");
            }
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
