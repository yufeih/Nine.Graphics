namespace Nine.Graphics.Rendering
{
    using System;
    using Nine.Graphics.Content;

    public class DXModelFactory : ModelFactory<int>
    {
        public DXModelFactory(IGraphicsHost graphicsHost, IModelLoader loader, int capacity = 1024)
            : base(graphicsHost, loader, capacity)
        { }

        public override Model CreateModel(ModelContent data)
        {
            throw new NotImplementedException();
        }
    }
}
